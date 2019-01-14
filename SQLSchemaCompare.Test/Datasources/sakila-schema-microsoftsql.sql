/*

Sakila for Microsoft SQL Server is a port of the Sakila example database available for MySQL, which was originally developed by Mike Hillyer of the MySQL AB documentation team. 
This project is designed to help database administrators to decide which database to use for development of new products
The user can run the same SQL against different kind of databases and compare the performance

License: BSD
Copyright DB Software Laboratory
http://www.etl-tools.com

*/

--
-- Schemas
--

CREATE SCHEMA inventory
GO
CREATE SCHEMA business
GO
CREATE SCHEMA customer_data
GO

--
-- User-Defined Data Types
--

CREATE TYPE custom_decimal
    FROM decimal(29, 8) NULL
GO

CREATE TYPE custom_varchar_max
    FROM varchar(max) NOT NULL
GO

CREATE TYPE phone_number
    FROM varchar(14) NOT NULL
GO

--
-- Functions
--

GO
CREATE FUNCTION StripWWWandCom (@Input VARCHAR(250))
RETURNS VARCHAR(250)
AS BEGIN
    DECLARE @Work VARCHAR(250)

    SET @Work = @Input

    SET @Work = REPLACE(@Work, 'www.', '')
    SET @Work = REPLACE(@Work, '.com', '')

    RETURN @Work
END
GO

GO
CREATE FUNCTION ufnGetAccountingEndDate()
RETURNS DATETIME
AS BEGIN
    RETURN DATEADD(millisecond, -2, CONVERT(DATETIME, '20040701', 112))
END
GO


--
-- Table structure for table actor
--

CREATE TABLE customer_data.actor (
  actor_id int NOT NULL IDENTITY ,
  first_name VARCHAR(45) NOT NULL,
  last_name VARCHAR(45) NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_actor_actor_id PRIMARY KEY NONCLUSTERED (actor_id)
  )
GO
 ALTER TABLE customer_data.actor ADD CONSTRAINT [DF_actor_last_update] DEFAULT (getdate()) FOR last_update
GO
 CREATE  INDEX idx_actor_last_name ON customer_data.actor(last_name) WHERE (last_name IS NOT NULL)
GO

--
-- Table structure for table country
--


CREATE TABLE customer_data.country (
  country_id SMALLINT NOT NULL IDENTITY ,
  country VARCHAR(50) NOT NULL,
  last_update DATETIME,
  last_update2 timestamp,
  CONSTRAINT PK_country_country_id PRIMARY KEY NONCLUSTERED (country_id)
)
GO
 ALTER TABLE customer_data.country ADD CONSTRAINT [DF_country_last_update] DEFAULT (getdate()) FOR last_update
GO

--
-- Table structure for table city
--

CREATE TABLE customer_data.city (
  city_id int NOT NULL IDENTITY ,
  city VARCHAR(50) NOT NULL,
  country_id SMALLINT NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_city_city_id PRIMARY KEY NONCLUSTERED (city_id),
  CONSTRAINT fk_city_country FOREIGN KEY (country_id) REFERENCES customer_data.country (country_id) ON DELETE NO ACTION ON UPDATE CASCADE
)
GO
 ALTER TABLE customer_data.city ADD CONSTRAINT [DF_city_last_update] DEFAULT (getdate()) FOR last_update
GO
 CREATE  INDEX idx_fk_country_id ON customer_data.city(country_id) 
GO

--
-- Table structure for table address
--

CREATE TABLE customer_data.address (
  address_id int NOT NULL IDENTITY ,
  address VARCHAR(50) NOT NULL,
  address2 VARCHAR(50) DEFAULT NULL,
  district VARCHAR(20) NOT NULL,
  city_id INT  NOT NULL,
  postal_code VARCHAR(10) DEFAULT NULL,
  phone phone_number NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_address_address_id PRIMARY KEY NONCLUSTERED (address_id)
)
GO
ALTER TABLE customer_data.address ADD CONSTRAINT [DF_address_last_update] DEFAULT (getdate()) FOR last_update
GO
CREATE  INDEX idx_fk_city_id ON customer_data.address(city_id) 
GO
ALTER TABLE customer_data.address WITH NOCHECK ADD  CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES customer_data.city (city_id) ON DELETE NO ACTION ON UPDATE CASCADE
GO
ALTER TABLE customer_data.address NOCHECK CONSTRAINT fk_address_city
GO

--
-- Table structure for table language
--

CREATE TABLE language (
  language_id TINYINT NOT NULL IDENTITY,
  name CHAR(20) NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_language_language_id PRIMARY KEY NONCLUSTERED (language_id)
)
GO
 ALTER TABLE language ADD CONSTRAINT [DF_language_last_update] DEFAULT (getdate()) FOR last_update
GO

--
-- Table structure for table category
--

CREATE TABLE category (
  category_id TINYINT NOT NULL IDENTITY,
  name VARCHAR(25) NOT NULL,
  last_update DATETIME NOT NULL,
  language_id TINYINT,
  CONSTRAINT PK_category_category_id PRIMARY KEY NONCLUSTERED (category_id)
)
GO
 ALTER TABLE category ADD CONSTRAINT [DF_category_last_update] DEFAULT (getdate()) FOR last_update
GO

--
-- Table structure for table customer
--

CREATE TABLE customer_data.customer (
  customer_id INT NOT NULL IDENTITY ,
  store_id INT NOT NULL,
  first_name VARCHAR(45) NOT NULL,
  last_name VARCHAR(45) NOT NULL,
  email VARCHAR(50) DEFAULT NULL,
  address_id INT NOT NULL,
  active CHAR(1) NOT NULL DEFAULT 'Y',
  create_date DATETIME NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_customer_customer_id PRIMARY KEY NONCLUSTERED (customer_id),
  CONSTRAINT fk_customer_address FOREIGN KEY (address_id) REFERENCES customer_data.address (address_id) ON DELETE NO ACTION ON UPDATE CASCADE
)
GO
 ALTER TABLE customer_data.customer ADD CONSTRAINT [DF_customer_last_update] DEFAULT (getdate()) FOR last_update
GO
 ALTER TABLE customer_data.customer ADD CONSTRAINT [DF_customer_create_date] DEFAULT (getdate()) FOR create_date
GO
 CREATE  INDEX idx_fk_store_id ON customer_data.customer(store_id) 
GO
 CREATE  INDEX idx_fk_address_id ON customer_data.customer(address_id) 
GO
 CREATE  INDEX idx_last_name ON customer_data.customer(last_name) 
GO

--
-- Table structure for table film
--

CREATE TABLE inventory.film (
  film_id int NOT NULL IDENTITY ,
  title VARCHAR(255) NOT NULL,
  description TEXT DEFAULT NULL,
  release_year VARCHAR(4) NULL,
  language_id TINYINT NOT NULL,
  original_language_id TINYINT DEFAULT NULL,
  rental_duration TINYINT NOT NULL DEFAULT 3,
  rental_rate DECIMAL(4,2) NOT NULL DEFAULT 4.99,
  length SMALLINT DEFAULT NULL,
  replacement_cost DECIMAL(5,2) NOT NULL DEFAULT 19.99,
  rating VARCHAR(10) DEFAULT 'G',
  special_features VARCHAR(255) DEFAULT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_film_film_id PRIMARY KEY NONCLUSTERED (film_id),
  CONSTRAINT fk_film_language FOREIGN KEY (language_id) REFERENCES language (language_id) ,
  CONSTRAINT fk_film_language_original FOREIGN KEY (original_language_id) REFERENCES language (language_id) 
)
GO
ALTER TABLE inventory.film ADD CONSTRAINT CHECK_special_features CHECK(special_features is null or 
                                                              special_features like '%Trailers%' or 
                                                              special_features like '%Commentaries%' or 
                                                              special_features like '%Deleted Scenes%' or 
                                                              special_features like '%Behind the Scenes%')
GO
ALTER TABLE inventory.film ADD CONSTRAINT CHECK_special_rating CHECK(rating in ('G','PG','PG-13','R','NC-17'))
GO
ALTER TABLE inventory.film ADD CONSTRAINT [DF_film_last_update] DEFAULT (getdate()) FOR last_update
GO
CREATE  INDEX idx_fk_language_id ON inventory.film(language_id) 
GO
CREATE  INDEX idx_fk_original_language_id ON inventory.film(original_language_id) 
GO


--
-- Table structure for table film_actor
--

CREATE TABLE inventory.film_actor (
  actor_id INT NOT NULL,
  film_id  INT NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_film_actor_actor_id_film_id PRIMARY KEY NONCLUSTERED (actor_id,film_id),
  CONSTRAINT fk_film_actor_actor FOREIGN KEY (actor_id) REFERENCES customer_data.actor (actor_id) ON DELETE NO ACTION ON UPDATE CASCADE,
  CONSTRAINT fk_film_actor_film FOREIGN KEY (film_id) REFERENCES inventory.film (film_id) ON DELETE NO ACTION ON UPDATE CASCADE
)
GO
 ALTER TABLE inventory.film_actor ADD CONSTRAINT [DF_film_actor_last_update] DEFAULT (getdate()) FOR last_update
GO
 CREATE  INDEX idx_fk_film_actor_film ON inventory.film_actor(film_id) 
GO
 CREATE  INDEX idx_fk_film_actor_actor ON inventory.film_actor(actor_id) 
GO

--
-- Table structure for table film_actor_description
--

CREATE TABLE inventory.film_actor_description (
  film_actor_description_id INT NOT NULL,
  actor_id INT NOT NULL,
  film_id  INT NOT NULL,
  CONSTRAINT PK_film_actor_description_film_actor_description_id PRIMARY KEY NONCLUSTERED (film_actor_description_id),
  CONSTRAINT fk_film_actor_description_film_actor FOREIGN KEY (actor_id,film_id) REFERENCES inventory.film_actor (actor_id,film_id) ON DELETE NO ACTION ON UPDATE CASCADE
)

--
-- Table structure for table film_category
--

CREATE TABLE inventory.film_category (
  film_id INT NOT NULL,
  category_id TINYINT  NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_film_category_film_id_category_id PRIMARY KEY NONCLUSTERED (film_id, category_id),
  CONSTRAINT fk_film_category_film FOREIGN KEY (film_id) REFERENCES inventory.film (film_id) ON DELETE NO ACTION ON UPDATE CASCADE,
  CONSTRAINT fk_film_category_category FOREIGN KEY (category_id) REFERENCES category (category_id) ON DELETE NO ACTION ON UPDATE CASCADE
)
GO
 ALTER TABLE inventory.film_category ADD CONSTRAINT [DF_film_category_last_update] DEFAULT (getdate()) FOR last_update
GO
 CREATE  INDEX idx_fk_film_category_film ON inventory.film_category(film_id) 
GO
 CREATE  INDEX idx_fk_film_category_category ON inventory.film_category(category_id) 
GO
--
-- Table structure for table film_text
--

CREATE TABLE inventory.film_text (
  film_id SMALLINT NOT NULL,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  last_update DATETIME NOT NULL,
  rating SMALLINT NOT NULL,
)
GO
ALTER TABLE inventory.film_text ADD CONSTRAINT CHECK_film_text CHECK(last_update IS NOT NULL AND rating > 0)
GO
--
-- Table structure for table inventory
--

CREATE TABLE inventory.inventory (
  inventory_id INT NOT NULL IDENTITY,
  film_id INT NOT NULL,
  store_id INT NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_inventory_inventory_id PRIMARY KEY NONCLUSTERED (inventory_id),
  CONSTRAINT fk_inventory_film FOREIGN KEY (film_id) REFERENCES inventory.film (film_id) ON DELETE NO ACTION ON UPDATE CASCADE
)
GO
 ALTER TABLE inventory.inventory ADD CONSTRAINT [DF_inventory_last_update] DEFAULT (getdate()) FOR last_update
GO
 CREATE  INDEX idx_fk_film_id ON inventory.inventory(film_id) 
GO
 CREATE  INDEX idx_fk_film_id_store_id ON inventory.inventory(store_id DESC, film_id ASC) 
GO

--
-- Table structure for table staff
--

CREATE TABLE business.staff (
  staff_id TINYINT NOT NULL IDENTITY,
  first_name VARCHAR(45) NOT NULL,
  last_name VARCHAR(45) NOT NULL,
  address_id INT NOT NULL,
  picture IMAGE DEFAULT NULL,
  email VARCHAR(50) DEFAULT NULL,
  store_id INT NOT NULL,
  active BIT NOT NULL DEFAULT 1,
  username VARCHAR(16) NOT NULL,
  password VARCHAR(40) DEFAULT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_staff_staff_id PRIMARY KEY NONCLUSTERED (staff_id),
  CONSTRAINT fk_staff_address FOREIGN KEY (address_id) REFERENCES customer_data.address (address_id) ON DELETE NO ACTION ON UPDATE CASCADE
)
GO
 ALTER TABLE business.staff ADD CONSTRAINT [DF_staff_last_update] DEFAULT (getdate()) FOR last_update
GO
 CREATE  INDEX idx_fk_store_id ON business.staff(store_id) 
GO
 CREATE  INDEX idx_fk_address_id ON business.staff(address_id) 
GO

--
-- Table structure for table store
--

CREATE TABLE business.store (
  store_id INT NOT NULL IDENTITY,
  manager_staff_id TINYINT NOT NULL,
  address_id INT NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_store_store_id PRIMARY KEY NONCLUSTERED (store_id),
  CONSTRAINT fk_store_staff FOREIGN KEY (manager_staff_id) REFERENCES business.staff (staff_id) ,
  CONSTRAINT fk_store_address FOREIGN KEY (address_id) REFERENCES customer_data.address (address_id) 
)

GO
 ALTER TABLE business.store ADD CONSTRAINT [DF_store_last_update] DEFAULT (getdate()) FOR last_update
GO
 CREATE UNIQUE NONCLUSTERED INDEX idx_fk_address_id ON business.store(manager_staff_id) 
GO
 CREATE  INDEX idx_fk_store_address ON business.store(address_id) 
GO


--
-- Table structure for table payment
--

CREATE TABLE business.payment (
  payment_id int NOT NULL IDENTITY ,
  payment_id_new INT NOT NULL,
  customer_id INT  NOT NULL,
  staff_id TINYINT NOT NULL,
  rental_id INT DEFAULT NULL,
  amount DECIMAL(5,2) NOT NULL,
  payment_date DATETIME NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_payment_payment_id PRIMARY KEY NONCLUSTERED (payment_id),
  CONSTRAINT fk_payment_customer FOREIGN KEY (customer_id) REFERENCES customer_data.customer (customer_id) ,
  CONSTRAINT fk_payment_staff FOREIGN KEY (staff_id) REFERENCES business.staff (staff_id) 
)
GO
 ALTER TABLE business.payment ADD CONSTRAINT [DF_payment_last_update] DEFAULT (getdate()) FOR last_update
GO
 CREATE  INDEX idx_fk_staff_id ON business.payment(staff_id) 
GO
 CREATE  INDEX idx_fk_customer_id ON business.payment(customer_id) 
GO

--
-- Table structure for table rental
--

CREATE TABLE business.rental (
  rental_id INT NOT NULL IDENTITY,
  rental_date DATETIME NOT NULL,
  inventory_id INT  NOT NULL,
  customer_id INT  NOT NULL,
  return_date DATETIME DEFAULT NULL,
  staff_id TINYINT  NOT NULL,
  last_update DATETIME NOT NULL,
  CONSTRAINT PK_rental_rental_id PRIMARY KEY NONCLUSTERED (rental_id),
  CONSTRAINT fk_rental_staff FOREIGN KEY (staff_id) REFERENCES business.staff (staff_id) ,
  CONSTRAINT fk_rental_inventory FOREIGN KEY (inventory_id) REFERENCES inventory.inventory (inventory_id) ,
  CONSTRAINT fk_rental_customer FOREIGN KEY (customer_id) REFERENCES customer_data.customer (customer_id) 
)
GO
 ALTER TABLE business.rental ADD CONSTRAINT [DF_rental_last_update] DEFAULT (getdate()) FOR last_update
GO
 CREATE INDEX idx_fk_inventory_id ON business.rental(inventory_id) 
GO
 CREATE INDEX idx_fk_customer_id ON business.rental(customer_id) 
GO
 CREATE INDEX idx_fk_staff_id ON business.rental(staff_id) 
GO
 CREATE UNIQUE INDEX   idx_uq  ON business.rental (rental_date,inventory_id,customer_id)
GO

-- FK CONSTRAINTS
ALTER TABLE customer_data.customer ADD CONSTRAINT fk_customer_store FOREIGN KEY (store_id) REFERENCES business.store (store_id) ON DELETE NO ACTION ON UPDATE CASCADE
GO
ALTER TABLE inventory.inventory ADD CONSTRAINT fk_inventory_store FOREIGN KEY (store_id) REFERENCES business.store (store_id) ON DELETE NO ACTION ON UPDATE CASCADE;
GO
ALTER TABLE business.staff ADD CONSTRAINT fk_staff_store FOREIGN KEY (store_id) REFERENCES business.store (store_id) ON DELETE NO ACTION ON UPDATE CASCADE;
GO
ALTER TABLE business.payment ADD CONSTRAINT fk_payment_rental FOREIGN KEY (rental_id) REFERENCES business.rental (rental_id) ON DELETE SET NULL ON UPDATE CASCADE;
GO

CREATE TABLE inventory.film_text_extra (
  film_text_extra_id INT NOT NULL IDENTITY,
  film_id SMALLINT NOT NULL,
  extra VARCHAR(255) DEFAULT NULL,
  CONSTRAINT PK_film_text_extra_id PRIMARY KEY NONCLUSTERED (film_text_extra_id),
)
GO

--
-- View structure for view customer_list
--

CREATE VIEW customer_list
AS
SELECT cu.customer_id AS ID, 
       cu.first_name + ' ' + cu.last_name AS name, 
       a.address AS address, 
       a.postal_code AS zip_code,
	   a.phone AS phone, 
	   customer_data.city.city AS city, 
	   c.country AS country, 
	   case when cu.active=1 then 'active' else '' end AS notes, 
	   cu.store_id AS SID
FROM customer_data.customer AS cu JOIN customer_data.address AS a ON cu.address_id = a.address_id JOIN customer_data.city ON a.city_id = customer_data.city.city_id
	JOIN customer_data.country c ON customer_data.city.country_id = c.country_id
GO
--
-- View structure for view film_list
--

CREATE VIEW film_list
AS
SELECT f.film_id AS FID, 
       f.title AS title, 
       f.description AS description, 
       category.name AS category, 
       f.rental_rate AS price,
	   f.length AS length, 
	   f.rating AS rating, 
	   a.first_name+' '+a.last_name AS actors
FROM category 
  LEFT JOIN inventory.film_category AS fc ON category.category_id = fc.category_id 
  LEFT JOIN inventory.film AS f ON fc.film_id = f.film_id
  JOIN inventory.film_actor AS fa ON f.film_id = fa.film_id
	JOIN customer_data.actor a ON fa.actor_id = a.actor_id
GO

--
-- View structure for view staff_list
--

CREATE VIEW staff_list
AS
SELECT s.staff_id AS ID, 
       s.first_name+' '+s.last_name AS name, 
       a.address AS address, 
       a.postal_code AS zip_code, 
       a.phone AS phone,
	   customer_data.city.city AS city, 
	   c.country AS country, 
	   s.store_id AS SID
FROM business.staff AS s JOIN customer_data.address AS a ON s.address_id = a.address_id JOIN customer_data.city ON a.city_id = customer_data.city.city_id
	JOIN customer_data.country c ON customer_data.city.country_id = c.country_id
GO
--
-- View structure for view sales_by_store
--

CREATE VIEW sales_by_store
AS
SELECT
  s.store_id
 ,c.city+','+cy.country AS store
 ,m.first_name+' '+ m.last_name AS manager
 ,SUM(p.amount) AS total_sales
FROM business.payment AS p
INNER JOIN business.rental AS r ON p.rental_id = r.rental_id
INNER JOIN inventory.inventory AS i ON r.inventory_id = i.inventory_id
INNER JOIN business.store AS s ON i.store_id = s.store_id
INNER JOIN customer_data.address AS a ON s.address_id = a.address_id
INNER JOIN customer_data.city AS c ON a.city_id = c.city_id
INNER JOIN customer_data.country AS cy ON c.country_id = cy.country_id
INNER JOIN business.staff AS m ON s.manager_staff_id = m.staff_id
GROUP BY   
  s.store_id
, c.city+ ','+cy.country 
, m.first_name+' '+ m.last_name
GO
--
-- View structure for view sales_by_film_category
--
-- Note that total sales will add up to >100% because
-- some titles belong to more than 1 category
--

CREATE VIEW sales_by_film_category
AS
SELECT
c.name AS category
, SUM(p.amount) AS total_sales
FROM business.payment AS p
INNER JOIN business.rental AS r ON p.rental_id = r.rental_id
INNER JOIN inventory.inventory AS i ON r.inventory_id = i.inventory_id
INNER JOIN inventory.film AS f ON i.film_id = f.film_id
INNER JOIN inventory.film_category AS fc ON f.film_id = fc.film_id
INNER JOIN category AS c ON fc.category_id = c.category_id
GROUP BY c.name
GO

--
-- View structure for view actor_info
--

/*
CREATE VIEW actor_info
AS
SELECT
a.actor_id,
a.first_name,
a.last_name,
GROUP_CONCAT(DISTINCT CONCAT(c.name, ': ',
		(SELECT GROUP_CONCAT(f.title ORDER BY f.title SEPARATOR ', ')
                    FROM sakila.film f
                    INNER JOIN sakila.film_category fc
                      ON f.film_id = fc.film_id
                    INNER JOIN sakila.film_actor fa
                      ON f.film_id = fa.film_id
                    WHERE fc.category_id = c.category_id
                    AND fa.actor_id = a.actor_id
                 )
             )
             ORDER BY c.name SEPARATOR '; ')
AS film_info
FROM sakila.actor a
LEFT JOIN sakila.film_actor fa
  ON a.actor_id = fa.actor_id
LEFT JOIN sakila.film_category fc
  ON fa.film_id = fc.film_id
LEFT JOIN sakila.category c
  ON fc.category_id = c.category_id
GROUP BY a.actor_id, a.first_name, a.last_name;
*/

GO
CREATE VIEW dbo.v_country_city_codes
WITH SCHEMABINDING
AS
SELECT co.country_id,
       ci.city_id,
       co.last_update
FROM customer_data.country co,
     customer_data.city ci
WHERE ci.country_id = co.country_id;
GO
CREATE UNIQUE CLUSTERED INDEX idx_country_city_codes ON dbo.v_country_city_codes(last_update);
GO

-- STORED PROCEDURES
GO
CREATE PROCEDURE uspGetAddress @City nvarchar(30) = NULL
AS
SELECT *
FROM customer_data.city
WHERE city = ISNULL(@City,city)
GO

-- TRIGGERS
GO

CREATE TRIGGER customer_data.reminder1
ON customer_data.country
AFTER INSERT, UPDATE
AS RAISERROR ('BLABLABLA', 16, 10)
GO

-- SEQUENCES
IF CAST(SERVERPROPERTY('ProductMajorVersion') AS INT) > 10
BEGIN
  EXECUTE('CREATE SEQUENCE actor_seq START WITH 1 INCREMENT BY 1')
END
GO

-- USERS
DECLARE @DB varchar(50)
SELECT @DB = DB_NAME()
EXECUTE('ALTER DATABASE [' + @DB + '] SET CONTAINMENT = PARTIAL')
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'sakila_user_ten')
BEGIN
  CREATE LOGIN sakila_user_login WITH PASSWORD = 'test1234'
END
GO

CREATE USER sakila_user_one WITH PASSWORD = 'test1234'
GO
CREATE USER sakila_user_two FOR LOGIN sakila_user_login
GO
CREATE USER sakila_user_three WITHOUT LOGIN
GO
