--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.3
-- Dumped by pg_dump version 10beta1

SET statement_timeout = 0;
SET lock_timeout = 0;
--SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
--SET row_security = off;

--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner:
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner:
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


SET search_path = public, pg_catalog;

--
-- Schemas
--

CREATE SCHEMA inventory;
CREATE SCHEMA business;
CREATE SCHEMA customer_data;


--
-- Name: mpaa_rating; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE mpaa_rating AS ENUM (
    'G',
    'PG',
    'PG-13',
    'R',
    'NC-17'
);


ALTER TYPE mpaa_rating OWNER TO postgres;

--
-- Name: year; Type: DOMAIN; Schema: public; Owner: postgres
--

CREATE DOMAIN year AS integer
	CONSTRAINT year_check CHECK (((VALUE >= 1901) AND (VALUE <= 2155)));


ALTER DOMAIN year OWNER TO postgres;

CREATE DOMAIN my_int_null AS integer NULL;

CREATE DOMAIN my_int_not_null AS integer NOT NULL;

CREATE TYPE compfoo AS (f1 int, f2 text);

CREATE TYPE my_type_with_array AS (primo int, secondo int[], terzo text);

CREATE TYPE bug_status AS ENUM ('new', 'open', 'closed');

CREATE TYPE float8_range AS RANGE (subtype = float8, subtype_diff = float8mi);

--
-- Name: _group_concat(text, text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION _group_concat(text, text) RETURNS text
    LANGUAGE sql IMMUTABLE
    AS $_$
SELECT CASE
  WHEN $2 IS NULL THEN $1
  WHEN $1 IS NULL THEN $2
  ELSE $1 || ', ' || $2
END
$_$;


ALTER FUNCTION public._group_concat(text, text) OWNER TO postgres;

--
-- Name: film_in_stock(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION film_in_stock(p_film_id integer, p_store_id integer, OUT p_film_count integer) RETURNS SETOF integer
    LANGUAGE sql
    AS $_$
     SELECT inventory_id
     FROM inventory
     WHERE film_id = $1
     AND store_id = $2
     AND inventory_in_stock(inventory_id);
$_$;


ALTER FUNCTION public.film_in_stock(p_film_id integer, p_store_id integer, OUT p_film_count integer) OWNER TO postgres;

--
-- Name: film_not_in_stock(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION film_not_in_stock(p_film_id integer, p_store_id integer, OUT p_film_count integer) RETURNS SETOF integer
    LANGUAGE sql
    AS $_$
    SELECT inventory_id
    FROM inventory
    WHERE film_id = $1
    AND store_id = $2
    AND NOT inventory_in_stock(inventory_id);
$_$;


ALTER FUNCTION public.film_not_in_stock(p_film_id integer, p_store_id integer, OUT p_film_count integer) OWNER TO postgres;

--
-- Name: get_customer_balance(integer, timestamp without time zone); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION get_customer_balance(p_customer_id integer, p_effective_date timestamp without time zone) RETURNS numeric
    LANGUAGE plpgsql
    AS $$
       --#OK, WE NEED TO CALCULATE THE CURRENT BALANCE GIVEN A CUSTOMER_ID AND A DATE
       --#THAT WE WANT THE BALANCE TO BE EFFECTIVE FOR. THE BALANCE IS:
       --#   1) RENTAL FEES FOR ALL PREVIOUS RENTALS
       --#   2) ONE DOLLAR FOR EVERY DAY THE PREVIOUS RENTALS ARE OVERDUE
       --#   3) IF A FILM IS MORE THAN RENTAL_DURATION * 2 OVERDUE, CHARGE THE REPLACEMENT_COST
       --#   4) SUBTRACT ALL PAYMENTS MADE BEFORE THE DATE SPECIFIED
DECLARE
    v_rentfees DECIMAL(5,2); --#FEES PAID TO RENT THE VIDEOS INITIALLY
    v_overfees INTEGER;      --#LATE FEES FOR PRIOR RENTALS
    v_payments DECIMAL(5,2); --#SUM OF PAYMENTS MADE PREVIOUSLY
BEGIN
    SELECT COALESCE(SUM(f.rental_rate),0) INTO v_rentfees
    FROM inventory.film f, inventory, business.rental r
    WHERE f.film_id = inventory.film_id
      AND inventory.inventory_id = r.inventory_id
      AND r.rental_date <= p_effective_date
      AND r.customer_id = p_customer_id;

    SELECT COALESCE(SUM(IF((r.return_date - r.rental_date) > (f.rental_duration * '1 day'::interval),
        ((r.return_date - r.rental_date) - (f.rental_duration * '1 day'::interval)),0)),0) INTO v_overfees
    FROM business.rental r, inventory, film f
    WHERE f.film_id = inventory.film_id
      AND inventory.inventory_id = r.inventory_id
      AND r.rental_date <= p_effective_date
      AND r.customer_id = p_customer_id;

    SELECT COALESCE(SUM(p.amount),0) INTO v_payments
    FROM business.payment AS p
    WHERE p.payment_date <= p_effective_date
    AND p.customer_id = p_customer_id;

    RETURN v_rentfees + v_overfees - v_payments;
END
$$;


ALTER FUNCTION public.get_customer_balance(p_customer_id integer, p_effective_date timestamp without time zone) OWNER TO postgres;

--
-- Name: inventory_held_by_customer(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION inventory_held_by_customer(p_inventory_id integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_customer_id INTEGER;
BEGIN

  SELECT customer_id INTO v_customer_id
  FROM business.rental
  WHERE return_date IS NULL
  AND inventory_id = p_inventory_id;

  RETURN v_customer_id;
END $$;


ALTER FUNCTION public.inventory_held_by_customer(p_inventory_id integer) OWNER TO postgres;

--
-- Name: inventory_in_stock(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION inventory_in_stock(p_inventory_id integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_rentals INTEGER;
    v_out     INTEGER;
BEGIN
    -- AN ITEM IS IN-STOCK IF THERE ARE EITHER NO ROWS IN THE rental TABLE
    -- FOR THE ITEM OR ALL ROWS HAVE return_date POPULATED

    SELECT count(*) INTO v_rentals
    FROM business.rental
    WHERE inventory_id = p_inventory_id;

    IF v_rentals = 0 THEN
      RETURN TRUE;
    END IF;

    SELECT COUNT(rental_id) INTO v_out
    FROM inventory LEFT JOIN business.rental AS r USING(inventory_id)
    WHERE inventory.inventory_id = p_inventory_id
    AND r.return_date IS NULL;

    IF v_out > 0 THEN
      RETURN FALSE;
    ELSE
      RETURN TRUE;
    END IF;
END $$;


ALTER FUNCTION public.inventory_in_stock(p_inventory_id integer) OWNER TO postgres;

--
-- Name: last_day(timestamp without time zone); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION last_day(timestamp without time zone) RETURNS date
    LANGUAGE sql IMMUTABLE STRICT
    AS $_$
  SELECT CASE
    WHEN EXTRACT(MONTH FROM $1) = 12 THEN
      (((EXTRACT(YEAR FROM $1) + 1) operator(pg_catalog.||) '-01-01')::date - INTERVAL '1 day')::date
    ELSE
      ((EXTRACT(YEAR FROM $1) operator(pg_catalog.||) '-' operator(pg_catalog.||) (EXTRACT(MONTH FROM $1) + 1) operator(pg_catalog.||) '-01')::date - INTERVAL '1 day')::date
    END
$_$;


ALTER FUNCTION public.last_day(timestamp without time zone) OWNER TO postgres;

--
-- Name: last_updated(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION last_updated() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.last_update = CURRENT_TIMESTAMP;
    RETURN NEW;
END $$;


ALTER FUNCTION public.last_updated() OWNER TO postgres;

--
-- Name: customer_customer_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE customer_customer_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CYCLE
    CACHE 1;


ALTER TABLE customer_customer_id_seq OWNER TO postgres;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: customer; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE customer_data.customer (
    customer_id integer DEFAULT nextval('customer_customer_id_seq'::regclass) NOT NULL,
    store_id smallint NOT NULL,
    first_name character varying(45) NOT NULL,
    last_name character varying(45) NOT NULL,
    email character varying(50),
    address_id smallint NOT NULL,
    activebool boolean DEFAULT true NOT NULL,
    create_date date DEFAULT ('now'::text)::date NOT NULL,
    last_update timestamp without time zone DEFAULT now(),
    active integer
);


ALTER TABLE customer_data.customer OWNER TO postgres;

--
-- Name: rewards_report(integer, numeric); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION rewards_report(min_monthly_purchases integer, min_dollar_amount_purchased numeric) RETURNS SETOF customer_data.customer
    LANGUAGE plpgsql SECURITY DEFINER
    AS $_$
DECLARE
    last_month_start DATE;
    last_month_end DATE;
rr RECORD;
tmpSQL TEXT;
BEGIN

    /* Some sanity checks... */
    IF min_monthly_purchases = 0 THEN
        RAISE EXCEPTION 'Minimum monthly purchases parameter must be > 0';
    END IF;
    IF min_dollar_amount_purchased = 0.00 THEN
        RAISE EXCEPTION 'Minimum monthly dollar amount purchased parameter must be > $0.00';
    END IF;

    last_month_start := CURRENT_DATE - '3 month'::interval;
    last_month_start := to_date((extract(YEAR FROM last_month_start) || '-' || extract(MONTH FROM last_month_start) || '-01'),'YYYY-MM-DD');
    last_month_end := LAST_DAY(last_month_start);

    /*
    Create a temporary storage area for Customer IDs.
    */
    CREATE TEMPORARY TABLE tmpCustomer (customer_id INTEGER NOT NULL PRIMARY KEY);

    /*
    Find all customers meeting the monthly purchase requirements
    */

    tmpSQL := 'INSERT INTO tmpCustomer (customer_id)
        SELECT p.customer_id
        FROM business.payment AS p
        WHERE DATE(p.payment_date) BETWEEN '||quote_literal(last_month_start) ||' AND '|| quote_literal(last_month_end) || '
        GROUP BY customer_id
        HAVING SUM(p.amount) > '|| min_dollar_amount_purchased || '
        AND COUNT(customer_id) > ' ||min_monthly_purchases ;

    EXECUTE tmpSQL;

    /*
    Output ALL customer information of matching rewardees.
    Customize output as needed.
    */
    FOR rr IN EXECUTE 'SELECT c.* FROM tmpCustomer AS t INNER JOIN customer_data.customer AS c ON t.customer_id = c.customer_id' LOOP
        RETURN NEXT rr;
    END LOOP;

    /* Clean up */
    tmpSQL := 'DROP TABLE tmpCustomer';
    EXECUTE tmpSQL;

RETURN;
END
$_$;


ALTER FUNCTION public.rewards_report(min_monthly_purchases integer, min_dollar_amount_purchased numeric) OWNER TO postgres;

--
-- Name: group_concat(text); Type: AGGREGATE; Schema: public; Owner: postgres
--

CREATE AGGREGATE group_concat(text) (
    SFUNC = _group_concat,
    STYPE = text
);


ALTER AGGREGATE public.group_concat(text) OWNER TO postgres;


CREATE AGGREGATE avg (float8)
(
    sfunc = float8_accum,
    stype = float8[],
    finalfunc = float8_avg,
    initcond = '{0,0,0}'
);


CREATE FUNCTION sum_product_fn(int,int,int) RETURNS int AS $$
    SELECT $1 + ($2 * $3);
$$ LANGUAGE SQL;

CREATE AGGREGATE sum_product(int, int) (
    sfunc = sum_product_fn,
    stype = int,
    initcond = 0
);


--
-- Name: actor_actor_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE actor_actor_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE actor_actor_id_seq OWNER TO postgres;

--
-- Name: actor; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE inventory.actor (
    actor_id integer DEFAULT nextval('actor_actor_id_seq'::regclass) NOT NULL,
    first_name character varying(45) NOT NULL,
    last_name character varying(45) NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL,
    test_type01 char,
    test_type03 aclitem,
    test_type04 bigserial,
    test_type05 cid,
    test_type06 daterange,
    test_type07 gtsvector,
    test_type08 int2vector,
    test_type09 int4range,
    test_type10 int8range,
    test_type11 name,
    test_type12 numrange,
    test_type13 oid,
    test_type14 oidvector,
    test_type17 pg_node_tree,
    test_type18 refcursor,
    test_type19 regclass,
    test_type20 regconfig,
    test_type21 regdictionary,
    test_type23 regoper,
    test_type24 regoperator,
    test_type25 regproc,
    test_type26 regprocedure,
    test_type28 regtype,
    test_type30 serial,
    test_type31 smallserial,
    test_type33 tid,
    test_type35 tsrange,
    test_type36 tstzrange,
    test_type37 xid,
    test_type39 compfoo,
    test_type40 float8_range,
    test_type41 mpaa_rating,
    test_type42 my_int_not_null,
    test_type43 my_int_null,
    test_type44 my_type_with_array,
    test_type45 year,
    test_type46 int8,
    test_type47 serial8,
    test_type48 varbit,
    test_type49 bool,
    test_type50 char(50),
    test_type51 varchar(42),
    test_type52 float8,
    test_type53 int,
    test_type54 int4,
    test_type55 decimal(12,3),
    test_type56 float4,
    test_type57 int2,
    test_type58 serial2,
    test_type59 serial4,
    test_type60 timetz,
    test_type61 timestamptz
);


ALTER TABLE inventory.actor OWNER TO postgres;

--
-- Name: category_category_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE category_category_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE category_category_id_seq OWNER TO postgres;

--
-- Name: category; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE category (
    category_id integer DEFAULT nextval('category_category_id_seq'::regclass) NOT NULL,
    name character varying(25) NOT NULL,
    language_id smallint NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE category OWNER TO postgres;

--
-- Name: film_film_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE film_film_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CYCLE
    CACHE 5;


ALTER TABLE film_film_id_seq OWNER TO postgres;

--
-- Name: film; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE inventory.film (
    film_id integer DEFAULT nextval('film_film_id_seq'::regclass) NOT NULL,
    title character varying(255) NOT NULL,
    description text,
    release_year year,
    language_id smallint NOT NULL,
    original_language_id smallint,
    rental_duration smallint DEFAULT 3 NOT NULL,
    rental_rate numeric(4,2) DEFAULT 4.99 NOT NULL,
    length smallint,
    replacement_cost numeric(5,2) DEFAULT 19.99 NOT NULL,
    rating mpaa_rating DEFAULT 'G'::mpaa_rating,
    last_update timestamp without time zone DEFAULT now() NOT NULL,
    special_features text[],
    fulltext tsvector NOT NULL
);


ALTER TABLE inventory.film OWNER TO postgres;

CREATE TABLE inventory.film_text (
  film_id integer NOT NULL,
  title VARCHAR(255) NOT NULL,
  description TEXT
);

--
-- Name: film_actor; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE inventory.film_actor (
    actor_id smallint NOT NULL,
    film_id smallint NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE inventory.film_actor OWNER TO postgres;

--
-- Name: film_category; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE inventory.film_category (
    film_id smallint NOT NULL,
    category_id smallint NOT NULL,
    film_text_id integer NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE inventory.film_category OWNER TO postgres;

--
-- Name: actor_info; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW actor_info AS
 SELECT a.actor_id,
    a.first_name,
    a.last_name,
    group_concat(DISTINCT (((c.name)::text || ': '::text) || ( SELECT group_concat((f.title)::text) AS group_concat
           FROM ((inventory.film f
             JOIN inventory.film_category fc_1 ON ((f.film_id = fc_1.film_id)))
             JOIN inventory.film_actor fa_1 ON ((f.film_id = fa_1.film_id)))
          WHERE ((fc_1.category_id = c.category_id) AND (fa_1.actor_id = a.actor_id))
          GROUP BY fa_1.actor_id))) AS film_info
   FROM (((inventory.actor a
     LEFT JOIN inventory.film_actor fa ON ((a.actor_id = fa.actor_id)))
     LEFT JOIN inventory.film_category fc ON ((fa.film_id = fc.film_id)))
     LEFT JOIN category c ON ((fc.category_id = c.category_id)))
  GROUP BY a.actor_id, a.first_name, a.last_name;


ALTER TABLE actor_info OWNER TO postgres;

--
-- Name: address_address_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE address_address_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE address_address_id_seq OWNER TO postgres;

--
-- Name: address; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE customer_data.address (
    address_id integer DEFAULT nextval('address_address_id_seq'::regclass) NOT NULL,
    address character varying(50) NOT NULL,
    address2 character varying(50),
    district character varying(20) NOT NULL,
    city_id smallint NOT NULL,
    postal_code character varying(10),
    phone character varying(20) NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE customer_data.address OWNER TO postgres;

--
-- Name: city_city_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE city_city_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 25;


ALTER TABLE city_city_id_seq OWNER TO postgres;

--
-- Name: city; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE customer_data.city (
    city_id integer DEFAULT nextval('city_city_id_seq'::regclass) NOT NULL,
    city character varying(50) NOT NULL,
    country_id smallint NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE customer_data.city OWNER TO postgres;

--
-- Name: country_country_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE country_country_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE country_country_id_seq OWNER TO postgres;

--
-- Name: country; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE customer_data.country (
    country_id integer DEFAULT nextval('country_country_id_seq'::regclass) NOT NULL,
    country character varying(50) NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE customer_data.country OWNER TO postgres;

--
-- Name: customer_list; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW customer_list AS
 SELECT cu.customer_id AS id,
    (((cu.first_name)::text || ' '::text) || (cu.last_name)::text) AS name,
    a.address,
    a.postal_code AS "zip code",
    a.phone,
    c.city,
    co.country,
        CASE
            WHEN cu.activebool THEN 'active'::text
            ELSE ''::text
        END AS notes,
    cu.store_id AS sid
   FROM (((customer_data.customer cu
     JOIN customer_data.address a ON ((cu.address_id = a.address_id)))
     JOIN customer_data.city AS c ON ((a.city_id = c.city_id)))
     JOIN customer_data.country AS co ON ((c.country_id = co.country_id)));


ALTER TABLE customer_list OWNER TO postgres;

--
-- Name: film_list; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW film_list AS
 SELECT f.film_id AS fid,
    f.title,
    f.description,
    category.name AS category,
    f.rental_rate AS price,
    f.length,
    f.rating,
    group_concat((((a.first_name)::text || ' '::text) || (a.last_name)::text)) AS actors
   FROM ((((category
     LEFT JOIN inventory.film_category fc ON ((category.category_id = fc.category_id)))
     LEFT JOIN inventory.film f ON ((fc.film_id = f.film_id)))
     JOIN inventory.film_actor fa ON ((f.film_id = fa.film_id)))
     JOIN inventory.actor a ON ((fa.actor_id = a.actor_id)))
  GROUP BY f.film_id, f.title, f.description, category.name, f.rental_rate, f.length, f.rating;


ALTER TABLE film_list OWNER TO postgres;

--
-- Name: inventory_inventory_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE inventory_inventory_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE inventory_inventory_id_seq OWNER TO postgres;

--
-- Name: inventory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE inventory (
    inventory_id integer DEFAULT nextval('inventory_inventory_id_seq'::regclass) NOT NULL,
    film_id smallint NOT NULL,
    store_id smallint NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE inventory OWNER TO postgres;

--
-- Name: language_language_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE language_language_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE language_language_id_seq OWNER TO postgres;

--
-- Name: language; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE language (
    language_id integer DEFAULT nextval('language_language_id_seq'::regclass) NOT NULL,
    name character(20) NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE language OWNER TO postgres;

--
-- Name: nicer_but_slower_film_list; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW nicer_but_slower_film_list AS
 SELECT f.film_id AS fid,
    f.title,
    f.description,
    category.name AS category,
    f.rental_rate AS price,
    f.length,
    f.rating,
    group_concat((((upper("substring"((a.first_name)::text, 1, 1)) || lower("substring"((a.first_name)::text, 2))) || upper("substring"((a.last_name)::text, 1, 1))) || lower("substring"((a.last_name)::text, 2)))) AS actors
   FROM ((((category
     LEFT JOIN inventory.film_category AS fc ON ((category.category_id = fc.category_id)))
     LEFT JOIN inventory.film AS f ON ((fc.film_id = f.film_id)))
     JOIN inventory.film_actor AS fa ON ((f.film_id = fa.film_id)))
     JOIN inventory.actor AS a ON ((fa.actor_id = a.actor_id)))
  GROUP BY f.film_id, f.title, f.description, category.name, f.rental_rate, f.length, f.rating;


ALTER TABLE nicer_but_slower_film_list OWNER TO postgres;

--
-- Name: payment_payment_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE payment_payment_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE payment_payment_id_seq OWNER TO postgres;

--
-- Name: payment; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE business.payment (
    payment_id integer DEFAULT nextval('payment_payment_id_seq'::regclass) NOT NULL,
    payment_id_new integer NOT NULL,
    customer_id smallint NOT NULL,
    staff_id smallint NOT NULL,
    rental_id integer NOT NULL,
    amount numeric(5,2) NOT NULL,
    payment_date timestamp without time zone NOT NULL
);


ALTER TABLE business.payment OWNER TO postgres;

--
-- Name: payment_p2017_01; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE payment_p2017_01 (
    CONSTRAINT payment_p2017_01_payment_date_check CHECK (((payment_date >= '2017-01-01 00:00:00'::timestamp without time zone) AND (payment_date < '2017-02-01 00:00:00'::timestamp without time zone)))
)
INHERITS (business.payment);


ALTER TABLE payment_p2017_01 OWNER TO postgres;

--
-- Name: payment_p2017_02; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE payment_p2017_02 (
    CONSTRAINT payment_p2017_02_payment_date_check CHECK (((payment_date >= '2017-02-01 00:00:00'::timestamp without time zone) AND (payment_date < '2017-03-01 00:00:00'::timestamp without time zone)))
)
INHERITS (business.payment);


ALTER TABLE payment_p2017_02 OWNER TO postgres;

--
-- Name: payment_p2017_03; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE payment_p2017_03 (
    CONSTRAINT payment_p2017_03_payment_date_check CHECK (((payment_date >= '2017-03-01 00:00:00'::timestamp without time zone) AND (payment_date < '2017-04-01 00:00:00'::timestamp without time zone)))
)
INHERITS (business.payment);


ALTER TABLE payment_p2017_03 OWNER TO postgres;

--
-- Name: payment_p2017_04; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE payment_p2017_04 (
    CONSTRAINT payment_p2017_04_payment_date_check CHECK (((payment_date >= '2017-04-01 00:00:00'::timestamp without time zone) AND (payment_date < '2017-05-01 00:00:00'::timestamp without time zone)))
)
INHERITS (business.payment);


ALTER TABLE payment_p2017_04 OWNER TO postgres;

--
-- Name: payment_p2017_05; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE payment_p2017_05 (
    CONSTRAINT payment_p2017_05_payment_date_check CHECK (((payment_date >= '2017-05-01 00:00:00'::timestamp without time zone) AND (payment_date < '2017-06-01 00:00:00'::timestamp without time zone)))
)
INHERITS (business.payment);


ALTER TABLE payment_p2017_05 OWNER TO postgres;

--
-- Name: payment_p2017_06; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE payment_p2017_06 (
    CONSTRAINT payment_p2017_06_payment_date_check CHECK (((payment_date >= '2017-06-01 00:00:00'::timestamp without time zone) AND (payment_date < '2017-07-01 00:00:00'::timestamp without time zone)))
)
INHERITS (business.payment);


ALTER TABLE payment_p2017_06 OWNER TO postgres;

--
-- Name: rental_rental_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE rental_rental_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE rental_rental_id_seq OWNER TO postgres;

--
-- Name: rental; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE business.rental (
    rental_id integer DEFAULT nextval('rental_rental_id_seq'::regclass) NOT NULL,
    rental_date timestamp without time zone NOT NULL,
    inventory_id integer NOT NULL,
    customer_id smallint NOT NULL,
    return_date timestamp without time zone,
    staff_id smallint NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE business.rental OWNER TO postgres;

--
-- Name: sales_by_film_category; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW sales_by_film_category AS
 SELECT c.name AS category,
    sum(p.amount) AS total_sales
   FROM (((((business.payment p
     JOIN business.rental r ON ((p.rental_id = r.rental_id)))
     JOIN inventory i ON ((r.inventory_id = i.inventory_id)))
     JOIN inventory.film f ON ((i.film_id = f.film_id)))
     JOIN inventory.film_category fc ON ((f.film_id = fc.film_id)))
     JOIN category c ON ((fc.category_id = c.category_id)))
  GROUP BY c.name
  ORDER BY (sum(p.amount)) DESC;


ALTER TABLE sales_by_film_category OWNER TO postgres;

--
-- Name: staff_staff_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE staff_staff_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE staff_staff_id_seq OWNER TO postgres;

--
-- Name: staff; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE business.staff (
    staff_id integer DEFAULT nextval('staff_staff_id_seq'::regclass) NOT NULL,
    first_name character varying(45) NOT NULL,
    last_name character varying(45) NOT NULL,
    address_id smallint NOT NULL,
    email character varying(50),
    store_id smallint NOT NULL,
    active boolean DEFAULT true NOT NULL,
    username character varying(16) NOT NULL,
    password character varying(40),
    last_update timestamp without time zone DEFAULT now() NOT NULL,
    picture bytea
);


ALTER TABLE business.staff OWNER TO postgres;

--
-- Name: store_store_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE store_store_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE store_store_id_seq OWNER TO postgres;

--
-- Name: store; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE business.store (
    store_id integer DEFAULT nextval('store_store_id_seq'::regclass) NOT NULL,
    manager_staff_id smallint NOT NULL,
    address_id smallint NOT NULL,
    last_update timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE business.store OWNER TO postgres;

--
-- Name: table_with_data; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE table_with_data (
    table_with_data_id serial primary key,
    data_type_smallint smallint NOT NULL,
    data_type_integer integer NOT NULL,
    data_type_bigint bigint NOT NULL,
    data_type_numeric numeric NOT NULL,
    data_type_decimal decimal NOT NULL,
    data_type_real real NOT NULL,
    data_type_double_precision double precision NOT NULL,
    data_type_money money NOT NULL,
    data_type_character character NOT NULL,
    data_type_character_varying character varying NOT NULL,
    data_type_char char NOT NULL,
    data_type_varchar varchar NOT NULL,
    data_type_text text NOT NULL,
    data_type_date date NOT NULL,
    data_type_time time NOT NULL,
    data_type_timestamp timestamp NOT NULL,
    data_type_interval interval NOT NULL,
    data_type_bit bit NOT NULL,
    data_type_bit_varying bit varying NOT NULL,
    data_type_bytea bytea NOT NULL,
    data_type_boolean boolean NOT NULL,
    data_type_cidr cidr NOT NULL,
    data_type_inet inet NOT NULL,
    data_type_macaddr macaddr NOT NULL,
    data_type_macaddr8 macaddr8 NOT NULL,
    data_type_point point NOT NULL,
    data_type_line line NOT NULL,
    data_type_lseg lseg NOT NULL,
    data_type_box box NOT NULL,
    data_type_path path NOT NULL,
    data_type_polygon polygon NOT NULL,
    data_type_circle circle NOT NULL,
    data_type_int4range int4range NOT NULL,
    data_type_int8range int8range NOT NULL,
    data_type_numrange numrange NOT NULL,
    data_type_tsrange tsrange NOT NULL,
    data_type_tstzrange tstzrange NOT NULL,
    data_type_daterange daterange NOT NULL,
    data_type_json json NOT NULL,
    data_type_jsonb jsonb NOT NULL,
    data_type_name name NOT NULL,
    data_type_tsquery tsquery NOT NULL,
    data_type_tsvector tsvector NOT NULL,
    data_type_uuid uuid NOT NULL,
    data_type_xml xml NOT NULL
);


ALTER TABLE table_with_data OWNER TO postgres;

INSERT INTO table_with_data
VALUES (DEFAULT,
        0, 0, 0, 0, 0, 0, 0, 0,
        '', '', '', '', '',
        CURRENT_DATE, CURRENT_TIME, CURRENT_TIMESTAMP,
        INTERVAL '0',
        B'0', B'0', '\000',
        false,
        '0.0.0.0', '0.0.0.0',
        '00:00:00:00:00:00', '00:00:00:00:00:00:00:00',
        '(0,0)', '{1,1,0}', '[(0,0),(0,0)]', '((0,0),(0,0))',
        '[(0,0),(0,0)]', '((0,0),(0,0))', '<(0,0),0>',
        'empty', 'empty', 'empty', 'empty', 'empty', 'empty',
        '{}', '{}',
        '', '', '',
        '00000000-0000-0000-0000-000000000000',
        '');

--
-- Name: sales_by_store; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW sales_by_store AS
 SELECT (((c.city)::text || ','::text) || (cy.country)::text) AS store,
    (((m.first_name)::text || ' '::text) || (m.last_name)::text) AS manager,
    sum(p.amount) AS total_sales
   FROM (((((((business.payment p
     JOIN business.rental r ON ((p.rental_id = r.rental_id)))
     JOIN inventory i ON ((r.inventory_id = i.inventory_id)))
     JOIN business.store s ON ((i.store_id = s.store_id)))
     JOIN customer_data.address a ON ((s.address_id = a.address_id)))
     JOIN customer_data.city c ON ((a.city_id = c.city_id)))
     JOIN customer_data.country cy ON ((c.country_id = cy.country_id)))
     JOIN business.staff m ON ((s.manager_staff_id = m.staff_id)))
  GROUP BY cy.country, c.city, s.store_id, m.first_name, m.last_name
  ORDER BY cy.country, c.city;


ALTER TABLE sales_by_store OWNER TO postgres;

--
-- Name: staff_list; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW staff_list AS
 SELECT s.staff_id AS id,
    (((s.first_name)::text || ' '::text) || (s.last_name)::text) AS name,
    a.address,
    a.postal_code AS "zip code",
    a.phone,
    c.city,
    co.country,
    s.store_id AS sid
   FROM (((business.staff s
     JOIN customer_data.address a ON ((s.address_id = a.address_id)))
     JOIN customer_data.city AS c ON ((a.city_id = c.city_id)))
     JOIN customer_data.country AS co ON ((c.country_id = co.country_id)));


ALTER TABLE staff_list OWNER TO postgres;

--
-- Name: payment_p2017_01 payment_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_01 ALTER COLUMN payment_id SET DEFAULT nextval('payment_payment_id_seq'::regclass);


--
-- Name: payment_p2017_02 payment_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_02 ALTER COLUMN payment_id SET DEFAULT nextval('payment_payment_id_seq'::regclass);


--
-- Name: payment_p2017_03 payment_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_03 ALTER COLUMN payment_id SET DEFAULT nextval('payment_payment_id_seq'::regclass);


--
-- Name: payment_p2017_04 payment_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_04 ALTER COLUMN payment_id SET DEFAULT nextval('payment_payment_id_seq'::regclass);


--
-- Name: payment_p2017_05 payment_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_05 ALTER COLUMN payment_id SET DEFAULT nextval('payment_payment_id_seq'::regclass);


--
-- Name: payment_p2017_06 payment_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_06 ALTER COLUMN payment_id SET DEFAULT nextval('payment_payment_id_seq'::regclass);


--
-- Name: actor actor_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.actor
    ADD CONSTRAINT actor_pkey PRIMARY KEY (actor_id);


--
-- Name: address address_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY customer_data.address
    ADD CONSTRAINT address_pkey PRIMARY KEY (address_id);


--
-- Name: category category_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY category
    ADD CONSTRAINT category_pkey PRIMARY KEY (category_id);


--
-- Name: city city_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY customer_data.city
    ADD CONSTRAINT city_pkey PRIMARY KEY (city_id);


--
-- Name: country country_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY customer_data.country
    ADD CONSTRAINT country_pkey PRIMARY KEY (country_id);


--
-- Name: customer customer_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY customer_data.customer
    ADD CONSTRAINT customer_pkey PRIMARY KEY (customer_id);


--
-- Name: film_actor film_actor_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.film_actor
    ADD CONSTRAINT film_actor_pkey PRIMARY KEY (actor_id, film_id);


--
-- Name: film_category film_category_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.film_category
    ADD CONSTRAINT film_category_pkey PRIMARY KEY (film_id, category_id);


--
-- Name: film film_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.film
    ADD CONSTRAINT film_pkey PRIMARY KEY (film_id);


--
-- Name: inventory inventory_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory
    ADD CONSTRAINT inventory_pkey PRIMARY KEY (inventory_id);


--
-- Name: language language_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY language
    ADD CONSTRAINT language_pkey PRIMARY KEY (language_id);


--
-- Name: payment payment_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.payment
    ADD CONSTRAINT payment_pkey PRIMARY KEY (payment_id);


--
-- Name: rental rental_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.rental
    ADD CONSTRAINT rental_pkey PRIMARY KEY (rental_id);


--
-- Name: staff staff_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.staff
    ADD CONSTRAINT staff_pkey PRIMARY KEY (staff_id);


--
-- Name: store store_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.store
    ADD CONSTRAINT store_pkey PRIMARY KEY (store_id);


--
-- Name: film_fulltext_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX film_fulltext_idx ON inventory.film USING gist (fulltext);


--
-- Name: idx_actor_last_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_actor_last_name ON inventory.actor USING btree (last_name);


--
-- Name: idx_fk_address_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_address_id ON customer_data.customer USING btree (address_id);


--
-- Name: idx_fk_city_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_city_id ON customer_data.address USING btree (city_id);


--
-- Name: idx_fk_country_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_country_id ON customer_data.city USING btree (country_id);


--
-- Name: idx_fk_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_customer_id ON business.payment USING btree (customer_id);


--
-- Name: idx_fk_film_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_film_id ON inventory.film_actor USING btree (film_id);


--
-- Name: idx_fk_inventory_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_inventory_id ON business.rental USING btree (inventory_id);


--
-- Name: idx_fk_language_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_language_id ON inventory.film USING btree (language_id);


--
-- Name: idx_fk_original_language_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_original_language_id ON inventory.film USING btree (original_language_id);


--
-- Name: idx_fk_payment_p2017_01_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_01_customer_id ON payment_p2017_01 USING btree (customer_id);


--
-- Name: idx_fk_payment_p2017_01_staff_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_01_staff_id ON payment_p2017_01 USING btree (staff_id);


--
-- Name: idx_fk_payment_p2017_02_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_02_customer_id ON payment_p2017_02 USING btree (customer_id);


--
-- Name: idx_fk_payment_p2017_02_staff_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_02_staff_id ON payment_p2017_02 USING btree (staff_id);


--
-- Name: idx_fk_payment_p2017_03_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_03_customer_id ON payment_p2017_03 USING btree (customer_id);


--
-- Name: idx_fk_payment_p2017_03_staff_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_03_staff_id ON payment_p2017_03 USING btree (staff_id);


--
-- Name: idx_fk_payment_p2017_04_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_04_customer_id ON payment_p2017_04 USING btree (customer_id);


--
-- Name: idx_fk_payment_p2017_04_staff_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_04_staff_id ON payment_p2017_04 USING btree (staff_id);


--
-- Name: idx_fk_payment_p2017_05_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_05_customer_id ON payment_p2017_05 USING btree (customer_id);


--
-- Name: idx_fk_payment_p2017_05_staff_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_05_staff_id ON payment_p2017_05 USING btree (staff_id);


--
-- Name: idx_fk_payment_p2017_06_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_06_customer_id ON payment_p2017_06 USING btree (customer_id);


--
-- Name: idx_fk_payment_p2017_06_staff_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_payment_p2017_06_staff_id ON payment_p2017_06 USING btree (staff_id);


--
-- Name: idx_fk_staff_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_staff_id ON business.payment USING btree (staff_id);


--
-- Name: idx_fk_store_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_fk_store_id ON customer_data.customer USING btree (store_id);


--
-- Name: idx_last_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_last_name ON customer_data.customer USING btree (last_name);


--
-- Name: idx_store_id_film_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_store_id_film_id ON inventory USING btree (store_id, film_id);


--
-- Name: idx_title; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_title ON inventory.film USING btree (title);


--
-- Name: idx_unq_manager_staff_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_unq_manager_staff_id ON business.store USING btree (manager_staff_id);


--
-- Name: idx_unq_rental_rental_date_inventory_id_customer_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_unq_rental_rental_date_inventory_id_customer_id ON business.rental USING btree (rental_date, inventory_id, customer_id);


--
-- Name: payment payment_insert_p2017_01; Type: RULE; Schema: public; Owner: postgres
--

--- CREATE RULE payment_insert_p2017_01 AS
---     ON INSERT TO payment
---    WHERE ((new.payment_date >= '2017-01-01 00:00:00'::timestamp without time zone) AND (new.payment_date < '2017-02-01 00:00:00'::timestamp without time zone)) DO INSTEAD  INSERT INTO payment_p2017_01 (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
---   VALUES (DEFAULT, new.customer_id, new.staff_id, new.rental_id, new.amount, new.payment_date);


--
-- Name: payment payment_insert_p2017_02; Type: RULE; Schema: public; Owner: postgres
--

--- CREATE RULE payment_insert_p2017_02 AS
---     ON INSERT TO payment
---    WHERE ((new.payment_date >= '2017-02-01 00:00:00'::timestamp without time zone) AND (new.payment_date < '2017-03-01 00:00:00'::timestamp without time zone)) DO INSTEAD  INSERT INTO payment_p2017_02 (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
---   VALUES (DEFAULT, new.customer_id, new.staff_id, new.rental_id, new.amount, new.payment_date);


--
-- Name: payment payment_insert_p2017_03; Type: RULE; Schema: public; Owner: postgres
--

--- CREATE RULE payment_insert_p2017_03 AS
---     ON INSERT TO payment
---    WHERE ((new.payment_date >= '2017-03-01 00:00:00'::timestamp without time zone) AND (new.payment_date < '2017-04-01 00:00:00'::timestamp without time zone)) DO INSTEAD  INSERT INTO payment_p2017_03 (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
---   VALUES (DEFAULT, new.customer_id, new.staff_id, new.rental_id, new.amount, new.payment_date);


--
-- Name: payment payment_insert_p2017_04; Type: RULE; Schema: public; Owner: postgres
--

--- CREATE RULE payment_insert_p2017_04 AS
---     ON INSERT TO payment
---    WHERE ((new.payment_date >= '2017-04-01 00:00:00'::timestamp without time zone) AND (new.payment_date < '2017-05-01 00:00:00'::timestamp without time zone)) DO INSTEAD  INSERT INTO payment_p2017_04 (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
---   VALUES (DEFAULT, new.customer_id, new.staff_id, new.rental_id, new.amount, new.payment_date);


--
-- Name: payment payment_insert_p2017_05; Type: RULE; Schema: public; Owner: postgres
--

--- CREATE RULE payment_insert_p2017_05 AS
---     ON INSERT TO payment
---    WHERE ((new.payment_date >= '2017-05-01 00:00:00'::timestamp without time zone) AND (new.payment_date < '2017-06-01 00:00:00'::timestamp without time zone)) DO INSTEAD  INSERT INTO payment_p2017_05 (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
---   VALUES (DEFAULT, new.customer_id, new.staff_id, new.rental_id, new.amount, new.payment_date);


--
-- Name: payment payment_insert_p2017_06; Type: RULE; Schema: public; Owner: postgres
--

--- CREATE RULE payment_insert_p2017_06 AS
---     ON INSERT TO payment
---    WHERE ((new.payment_date >= '2017-06-01 00:00:00'::timestamp without time zone) AND (new.payment_date < '2017-07-01 00:00:00'::timestamp without time zone)) DO INSTEAD  INSERT INTO payment_p2017_06 (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
---   VALUES (DEFAULT, new.customer_id, new.staff_id, new.rental_id, new.amount, new.payment_date);


--
-- Name: film film_fulltext_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER film_fulltext_trigger BEFORE INSERT OR UPDATE ON inventory.film FOR EACH ROW EXECUTE PROCEDURE tsvector_update_trigger('fulltext', 'pg_catalog.english', 'title', 'description');


--
-- Name: actor last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON inventory.actor FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: address last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON customer_data.address FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: category last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON category FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: city last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON customer_data.city FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: country last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON customer_data.country FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: customer last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON customer_data.customer FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: film last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON inventory.film FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: film_actor last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON inventory.film_actor FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: film_category last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON inventory.film_category FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: inventory last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON inventory FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: language last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON language FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: rental last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON business.rental FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: staff last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON business.staff FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: store last_updated; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER last_updated BEFORE UPDATE ON business.store FOR EACH ROW EXECUTE PROCEDURE last_updated();


--
-- Name: address address_city_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY customer_data.address
    ADD CONSTRAINT address_city_id_fkey FOREIGN KEY (city_id) REFERENCES customer_data.city(city_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: city city_country_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY customer_data.city
    ADD CONSTRAINT city_country_id_fkey FOREIGN KEY (country_id) REFERENCES customer_data.country(country_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: customer customer_address_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY customer_data.customer
    ADD CONSTRAINT customer_address_id_fkey FOREIGN KEY (address_id) REFERENCES customer_data.address(address_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: customer customer_store_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY customer_data.customer
    ADD CONSTRAINT customer_store_id_fkey FOREIGN KEY (store_id) REFERENCES business.store(store_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: film_actor film_actor_actor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.film_actor
    ADD CONSTRAINT film_actor_actor_id_fkey FOREIGN KEY (actor_id) REFERENCES inventory.actor(actor_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: film_actor film_actor_film_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.film_actor
    ADD CONSTRAINT film_actor_film_id_fkey FOREIGN KEY (film_id) REFERENCES inventory.film(film_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: film_category film_category_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.film_category
    ADD CONSTRAINT film_category_category_id_fkey FOREIGN KEY (category_id) REFERENCES category(category_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: film_category film_category_film_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.film_category
    ADD CONSTRAINT film_category_film_id_fkey FOREIGN KEY (film_id) REFERENCES inventory.film(film_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: film film_language_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.film
    ADD CONSTRAINT film_language_id_fkey FOREIGN KEY (language_id) REFERENCES language(language_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: film film_original_language_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory.film
    ADD CONSTRAINT film_original_language_id_fkey FOREIGN KEY (original_language_id) REFERENCES language(language_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: inventory inventory_film_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory
    ADD CONSTRAINT inventory_film_id_fkey FOREIGN KEY (film_id) REFERENCES inventory.film(film_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: inventory inventory_store_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY inventory
    ADD CONSTRAINT inventory_store_id_fkey FOREIGN KEY (store_id) REFERENCES business.store(store_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: payment payment_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.payment
    ADD CONSTRAINT payment_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES customer_data.customer(customer_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: payment_p2017_01 payment_p2017_01_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_01
    ADD CONSTRAINT payment_p2017_01_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES customer_data.customer(customer_id);


--
-- Name: payment_p2017_01 payment_p2017_01_rental_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_01
    ADD CONSTRAINT payment_p2017_01_rental_id_fkey FOREIGN KEY (rental_id) REFERENCES business.rental(rental_id);


--
-- Name: payment_p2017_01 payment_p2017_01_staff_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_01
    ADD CONSTRAINT payment_p2017_01_staff_id_fkey FOREIGN KEY (staff_id) REFERENCES business.staff(staff_id);


--
-- Name: payment_p2017_02 payment_p2017_02_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_02
    ADD CONSTRAINT payment_p2017_02_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES customer_data.customer(customer_id);


--
-- Name: payment_p2017_02 payment_p2017_02_rental_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_02
    ADD CONSTRAINT payment_p2017_02_rental_id_fkey FOREIGN KEY (rental_id) REFERENCES business.rental(rental_id);


--
-- Name: payment_p2017_02 payment_p2017_02_staff_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_02
    ADD CONSTRAINT payment_p2017_02_staff_id_fkey FOREIGN KEY (staff_id) REFERENCES business.staff(staff_id);


--
-- Name: payment_p2017_03 payment_p2017_03_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_03
    ADD CONSTRAINT payment_p2017_03_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES customer_data.customer(customer_id);


--
-- Name: payment_p2017_03 payment_p2017_03_rental_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_03
    ADD CONSTRAINT payment_p2017_03_rental_id_fkey FOREIGN KEY (rental_id) REFERENCES business.rental(rental_id);


--
-- Name: payment_p2017_03 payment_p2017_03_staff_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_03
    ADD CONSTRAINT payment_p2017_03_staff_id_fkey FOREIGN KEY (staff_id) REFERENCES business.staff(staff_id);


--
-- Name: payment_p2017_04 payment_p2017_04_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_04
    ADD CONSTRAINT payment_p2017_04_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES customer_data.customer(customer_id);


--
-- Name: payment_p2017_04 payment_p2017_04_rental_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_04
    ADD CONSTRAINT payment_p2017_04_rental_id_fkey FOREIGN KEY (rental_id) REFERENCES business.rental(rental_id);


--
-- Name: payment_p2017_04 payment_p2017_04_staff_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_04
    ADD CONSTRAINT payment_p2017_04_staff_id_fkey FOREIGN KEY (staff_id) REFERENCES business.staff(staff_id);


--
-- Name: payment_p2017_05 payment_p2017_05_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_05
    ADD CONSTRAINT payment_p2017_05_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES customer_data.customer(customer_id);


--
-- Name: payment_p2017_05 payment_p2017_05_rental_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_05
    ADD CONSTRAINT payment_p2017_05_rental_id_fkey FOREIGN KEY (rental_id) REFERENCES business.rental(rental_id);


--
-- Name: payment_p2017_05 payment_p2017_05_staff_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_05
    ADD CONSTRAINT payment_p2017_05_staff_id_fkey FOREIGN KEY (staff_id) REFERENCES business.staff(staff_id);


--
-- Name: payment_p2017_06 payment_p2017_06_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_06
    ADD CONSTRAINT payment_p2017_06_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES customer_data.customer(customer_id);


--
-- Name: payment_p2017_06 payment_p2017_06_rental_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_06
    ADD CONSTRAINT payment_p2017_06_rental_id_fkey FOREIGN KEY (rental_id) REFERENCES business.rental(rental_id);


--
-- Name: payment_p2017_06 payment_p2017_06_staff_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY payment_p2017_06
    ADD CONSTRAINT payment_p2017_06_staff_id_fkey FOREIGN KEY (staff_id) REFERENCES business.staff(staff_id);


--
-- Name: payment payment_rental_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.payment
    ADD CONSTRAINT payment_rental_id_fkey FOREIGN KEY (rental_id) REFERENCES business.rental(rental_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: payment payment_staff_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.payment
    ADD CONSTRAINT payment_staff_id_fkey FOREIGN KEY (staff_id) REFERENCES business.staff(staff_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: rental rental_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.rental
    ADD CONSTRAINT rental_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES customer_data.customer(customer_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: rental rental_inventory_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.rental
    ADD CONSTRAINT rental_inventory_id_fkey FOREIGN KEY (inventory_id) REFERENCES inventory(inventory_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: rental rental_staff_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.rental
    ADD CONSTRAINT rental_staff_id_fkey FOREIGN KEY (staff_id) REFERENCES business.staff(staff_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: staff staff_address_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.staff
    ADD CONSTRAINT staff_address_id_fkey FOREIGN KEY (address_id) REFERENCES customer_data.address(address_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: staff staff_store_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.staff
    ADD CONSTRAINT staff_store_id_fkey FOREIGN KEY (store_id) REFERENCES business.store(store_id);


--
-- Name: store store_address_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY business.store
    ADD CONSTRAINT store_address_id_fkey FOREIGN KEY (address_id) REFERENCES customer_data.address(address_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

