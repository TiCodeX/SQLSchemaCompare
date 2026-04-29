namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;

/// <summary>
/// Script helper class specific for PostgreSql database
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PostgreSqlScriptHelper"/> class.
/// </remarks>
/// <param name="options">The project options</param>
public class PostgreSqlScriptHelper(ProjectOptions options) : AScriptHelper(options)
{
    /// <summary>
    /// Scripts the function argument data type
    /// </summary>
    /// <param name="argType">The argument data type</param>
    /// <param name="dataTypes">The list of database data types</param>
    /// <returns>The scripted function argument data type</returns>
    public static string ScriptFunctionArgumentType(uint argType, IEnumerable<ABaseDbDataType> dataTypes)
    {
        var type = (PostgreSqlDataType)dataTypes.FirstOrDefault(x => ((PostgreSqlDataType)x).TypeId == argType) ?? throw new ArgumentException($"Unknown argument data type: {argType}");
        if (type.IsArray && type.ArrayType != null)
        {
            return $"{ScriptDataTypeName(type.ArrayType.Schema.ToUpperInvariant() != "PUBLIC" ? type.ArrayType.Schema + "." + type.ArrayType.Name : type.ArrayType.Name)}[]";
        }

        return ScriptDataTypeName(type.Schema.ToUpperInvariant() != "PUBLIC" ? type.Schema + "." + type.Name : type.Name);
    }

    /// <summary>
    /// Scripts the function attributes
    /// </summary>
    /// <param name="function">The function</param>
    /// <returns>The scripted function attributes</returns>
    public static string ScriptFunctionAttributes(PostgreSqlFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var sb = new StringBuilder();

        switch (function.Volatile)
        {
            case 'i':
                sb.Append("IMMUTABLE");
                break;
            case 's':
                sb.Append("STABLE");
                break;
            case 'v':
                sb.Append("VOLATILE");
                break;
            default: throw new ArgumentException($"Unknown volatile attribute: {function.Volatile}");
        }

        if (function.SecurityType == "DEFINER")
        {
            sb.Append(" SECURITY DEFINER");
        }

        if (function.IsStrict)
        {
            sb.Append(" STRICT");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Scripts the function arguments
    /// </summary>
    /// <param name="argType">The argument type</param>
    /// <param name="argMode">The argument mode</param>
    /// <param name="argName">The argument name</param>
    /// <param name="dataTypes">The list of database data types</param>
    /// <returns>The scripted function argument</returns>
    public static string ScriptFunctionArgument(uint argType, char argMode, string argName, IEnumerable<ABaseDbDataType> dataTypes)
    {
        var sb = new StringBuilder();

        switch (argMode)
        {
            case 'o':
                sb.Append("OUT ");
                break;
            case 'b':
                sb.Append("INOUT ");
                break;
            case 'v':
                sb.Append("VARIADIC ");
                break;
            default:
                // Do nothing
                break;
        }

        if (!string.IsNullOrEmpty(argName))
        {
            sb.Append($"{argName} ");
        }

        sb.Append(ScriptFunctionArgumentType(argType, dataTypes));
        return sb.ToString();
    }

    /// <summary>
    /// Script the foreign key match option
    /// </summary>
    /// <param name="matchOption">The match option string</param>
    /// <returns>The scripted match option</returns>
    public static object ScriptForeignKeyMatchOption(string matchOption)
    {
        return matchOption switch
        {
            "NONE" => "MATCH SIMPLE",
            "FULL" => "MATCH FULL",
            "PARTIAL" => "MATCH PARTIAL",
            _ => throw new ArgumentException($"Unknown foreign key match option: {matchOption}"),
        };
    }

    /// <inheritdoc/>
    public override string ScriptObjectName(string objectSchema, string objectName)
    {
        return !string.IsNullOrWhiteSpace(objectSchema) ?
            $"\"{objectSchema}\".\"{objectName}\"" :
            $"\"{objectName}\"";
    }

    /// <inheritdoc/>
    public override string ScriptColumn(ABaseDbColumn column, bool scriptDefaultConstraint)
    {
        if (column is not PostgreSqlColumn col)
        {
            throw new ArgumentNullException(nameof(column));
        }

        var sb = new StringBuilder();

        sb.Append($"\"{col.Name}\" {this.ScriptDataType(col)}");

        sb.Append(col.IsNullable ? " NULL" : " NOT NULL");

        // Always script the default if present
        if (!string.IsNullOrWhiteSpace(col.ColumnDefault))
        {
            sb.Append($" DEFAULT {col.ColumnDefault}");
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public override string ScriptColumnDefaultValue(ABaseDbColumn column)
    {
        ArgumentNullException.ThrowIfNull(column);

        return column.DataType switch
        {
            // Exact numerics
            "smallint" or "integer" or "bigint" or "numeric" or "decimal" => "0",

            // Approximate numerics
            "real" or "double precision" => "0",

            // Money
            "money" => "0",

            // Character strings
            "character" or "character varying" or "char" or "varchar" or "text" => "''",

            // Date and time
            "date" => "CURRENT_DATE",
            "abstime" or "reltime" or "timetz" or "time with time zone" or "time" or "time without time zone" => "CURRENT_TIME",
            "timestamptz" or "timestamp with time zone" or "timestamp" or "timestamp without time zone" => "CURRENT_TIMESTAMP",
            "interval" or "tinterval" => "INTERVAL '0'",

            // Binary strings
            "bit" or "bit varying" => "B'0'",
            "bytea" => "'\\000'",

            // Boolean
            "boolean" => "false",

            // Network Address
            "cidr" or "inet" => "'0.0.0.0'",
            "macaddr" => "'00:00:00:00:00:00'",
            "macaddr8" => "'00:00:00:00:00:00:00:00'",

            // Geometric
            "point" => "'(0,0)'",
            "line" => "'{1,1,0}'",
            "lseg" => "'[(0,0),(0,0)]'",
            "box" => "'((0,0),(0,0))'",
            "path" => "'[(0,0),(0,0)]'",
            "polygon" => "'((0,0),(0,0))'",
            "circle" => "'<(0,0),0>'",

            // Range
            "int4range" or "int8range" or "numrange" or "tsrange" or "tstzrange" or "daterange" => "'empty'",

            // User defined types
            "USER-DEFINED" => column.ColumnDefault,

            // Other data types
            "json" or "jsonb" => "'{}'",
            "name" or "tsquery" or "tsvector" => "''",
            "uuid" => $"'{Guid.Empty}'",
            "xml" => "''",
            _ => throw new ArgumentException($"Unknown data type: {column.DataType}"),
        };
    }

    /// <inheritdoc />
    public override string ScriptCommitTransaction()
    {
        return string.Empty;
    }

    /// <summary>
    /// Script the column data type
    /// </summary>
    /// <param name="column">The column</param>
    /// <returns>The scripted data type</returns>
    [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "TODO")]
    [SuppressMessage("Major Code Smell", "S138:Functions should not have too many lines of code", Justification = "TODO")]
    public string ScriptDataType(PostgreSqlColumn column)
    {
        ArgumentNullException.ThrowIfNull(column);

        switch (column.DataType)
        {
            // Exact numerics
            case "smallint":
            case "integer":
            case "bigint":
            case "smallserial":
            case "serial":
            case "bigserial":
                return $"{column.DataType}";

            case "numeric":
            case "decimal":
                return column.NumericPrecision.HasValue && column.NumericScale.HasValue
                    ? $"{column.DataType}({column.NumericPrecision},{column.NumericScale})"
                    : $"{column.DataType}";

            // Approximate numerics
            case "real":
            case "double precision":

            // Money
            case "money":
                return $"{column.DataType}";

            // Character strings
            case "character":
            case "character varying":
            case "char":
            case "varchar":
                {
                    var collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                    return column.CharacterMaxLenght.HasValue
                        ? $"{column.DataType}({column.CharacterMaxLenght}){collate}"
                        : $"{column.DataType}{collate}";
                }

            case "text":
                {
                    var collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                    return $"{column.DataType}{collate}";
                }

            // Date and time
            case "date":
            case "abstime":
            case "reltime":
            case "tinterval":
                return $"{column.DataType}";

            case "timetz":
            case "time with time zone":
                {
                    var precision = column.DateTimePrecision != 6 ? $"({column.DateTimePrecision})" : string.Empty;
                    return $"time{precision} with time zone";
                }

            case "time":
            case "time without time zone":
                {
                    var precision = column.DateTimePrecision != 6 ? $"({column.DateTimePrecision})" : string.Empty;
                    return $"time{precision} without time zone";
                }

            case "timestamptz":
            case "timestamp with time zone":
                {
                    var precision = column.DateTimePrecision != 6 ? $"({column.DateTimePrecision})" : string.Empty;
                    return $"timestamp{precision} with time zone";
                }

            case "timestamp":
            case "timestamp without time zone":
                {
                    var precision = column.DateTimePrecision != 6 ? $"({column.DateTimePrecision})" : string.Empty;
                    return $"timestamp{precision} without time zone";
                }

            case "interval":
                {
                    var precision = string.Empty;
                    if (column.IntervalType != null)
                    {
                        precision = $" ({column.IntervalType})";
                    }
                    else if (column.DateTimePrecision != 6)
                    {
                        precision = $"({column.DateTimePrecision})";
                    }
                    else
                    {
                        // Do nothing
                    }

                    return $"interval{precision}";
                }

            // Binary strings
            case "bit":
            case "bit varying":
                {
                    var precision = column.CharacterMaxLenght != null ? $"({column.CharacterMaxLenght})" : string.Empty;
                    return $"{column.DataType}{precision}";
                }

            case "bytea":

            // Boolean
            case "boolean":
                return $"{column.DataType}";

            // Network Address
            case "cidr":
            case "inet":
            case "macaddr":
            case "macaddr8":
                return $"{column.DataType}";

            // Geometric
            case "point":
            case "line":
            case "lseg":
            case "box":
            case "path":
            case "polygon":
            case "circle":
                return $"{column.DataType}";

            // Range
            case "int4range":
            case "int8range":
            case "numrange":
            case "tsrange":
            case "tstzrange":
            case "daterange":
                return $"{column.DataType}";

            // User defined types
            case "USER-DEFINED":
                return $"{column.UdtName}";

            // Other data types
            case "aclitem":
            case "cid":
            case "gtsvector":
            case "json":
            case "jsonb":
            case "name":
            case "oid":
            case "pg_dependencies":
            case "pg_ndistinct":
            case "pg_node_tree":
            case "pg_lsn":
            case "refcursor":
            case "regclass":
            case "regconfig":
            case "regdictionary":
            case "regnamespace":
            case "regoper":
            case "regoperator":
            case "regproc":
            case "regprocedure":
            case "regrole":
            case "regtype":
            case "smgr":
            case "tid":
            case "tsquery":
            case "tsvector":
            case "txid_snapshot":
            case "uuid":
            case "xid":
            case "xml":
                return $"{column.DataType}";

            case "ARRAY":
                // TODO: Array type initialization does not consider parameters and no collation considered for string types
                // E.g.: "myColumnName character varying(55)[4][4]"
                // ==> currently will result in "myColumnName varchar[]"
                if (column.UdtName.StartsWith("_", StringComparison.Ordinal))
                {
                    return $"{column.UdtName[1..]}[]";
                }

                return $"{column.UdtName}";

            default: throw new ArgumentException($"Unknown data type: {column.DataType}");
        }
    }

    /// <summary>
    /// Script the data type name
    /// </summary>
    /// <param name="dataTypeName">The data type name</param>
    /// <returns>The scripted data type name</returns>
    private static string ScriptDataTypeName(string dataTypeName)
    {
        return dataTypeName switch
        {
            "int2" => "smallint",
            "int" or "int4" => "integer",
            "int8" => "bigint",
            "serial2" => "smallserial",
            "serial4" => "serial",
            "serial8" => "bigserial",
            "float4" => "real",
            "float8" => "double precision",
            "varbit" => "bit varying",
            "bool" => "boolean",
            "decimal" => "numeric",
            "char" => "character",
            "varchar" => "character varying",
            "time" => "time without time zone",
            "timetz" => "time with time zone",
            "timestamp" => "timestamp without time zone",
            "timestamptz" => "timestamp with time zone",
            _ => dataTypeName,
        };
    }
}
