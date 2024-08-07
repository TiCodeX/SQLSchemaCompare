﻿namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Script helper class specific for PostgreSql database
    /// </summary>
    public class PostgreSqlScriptHelper : AScriptHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlScriptHelper"/> class.
        /// </summary>
        /// <param name="options">The project options</param>
        public PostgreSqlScriptHelper(ProjectOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Scripts the function argument data type
        /// </summary>
        /// <param name="argType">The argument data type</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The scripted function argument data type</returns>
        public static string ScriptFunctionArgumentType(uint argType, IEnumerable<ABaseDbDataType> dataTypes)
        {
            var type = (PostgreSqlDataType)dataTypes.FirstOrDefault(x => ((PostgreSqlDataType)x).TypeId == argType);
            if (type == null)
            {
                throw new ArgumentException($"Unknown argument data type: {argType}");
            }

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
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

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
            switch (matchOption)
            {
                case "NONE":
                    return "MATCH SIMPLE";
                case "FULL":
                    return "MATCH FULL";
                case "PARTIAL":
                    return "MATCH PARTIAL";
                default:
                    throw new ArgumentException($"Unknown foreign key match option: {matchOption}");
            }
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
            var col = column as PostgreSqlColumn;
            if (col == null)
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
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            switch (column.DataType)
            {
                // Exact numerics
                case "smallint":
                case "integer":
                case "bigint":
                case "numeric":
                case "decimal":
                    return "0";

                // Approximate numerics
                case "real":
                case "double precision":
                    return "0";

                // Money
                case "money":
                    return "0";

                // Character strings
                case "character":
                case "character varying":
                case "char":
                case "varchar":
                case "text":
                    return "''";

                // Date and time
                case "date":
                    return "CURRENT_DATE";

                case "abstime":
                case "reltime":
                case "timetz":
                case "time with time zone":
                case "time":
                case "time without time zone":
                    return "CURRENT_TIME";

                case "timestamptz":
                case "timestamp with time zone":
                case "timestamp":
                case "timestamp without time zone":
                    return "CURRENT_TIMESTAMP";

                case "interval":
                case "tinterval":
                    return "INTERVAL '0'";

                // Binary strings
                case "bit":
                case "bit varying":
                    return "B'0'";

                case "bytea":
                    return "'\\000'";

                // Boolean
                case "boolean":
                    return "false";

                // Network Address
                case "cidr":
                case "inet":
                    return "'0.0.0.0'";
                case "macaddr":
                    return "'00:00:00:00:00:00'";
                case "macaddr8":
                    return "'00:00:00:00:00:00:00:00'";

                // Geometric
                case "point":
                    return "'(0,0)'";
                case "line":
                    return "'{1,1,0}'";
                case "lseg":
                    return "'[(0,0),(0,0)]'";
                case "box":
                    return "'((0,0),(0,0))'";
                case "path":
                    return "'[(0,0),(0,0)]'";
                case "polygon":
                    return "'((0,0),(0,0))'";
                case "circle":
                    return "'<(0,0),0>'";

                // Range
                case "int4range":
                case "int8range":
                case "numrange":
                case "tsrange":
                case "tstzrange":
                case "daterange":
                    return "'empty'";

                // User defined types
                case "USER-DEFINED":
                    return column.ColumnDefault;

                // Other data types
                case "json":
                case "jsonb":
                    return "'{}'";
                case "name":
                case "tsquery":
                case "tsvector":
                    return "''";
                case "uuid":
                    return $"'{Guid.Empty}'";
                case "xml":
                    return "''";

                default: throw new ArgumentException($"Unknown data type: {column.DataType}");
            }
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
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

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
                        return $"{column.UdtName.Substring(1)}[]";
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
            switch (dataTypeName)
            {
                case "int2": return "smallint";
                case "int":
                case "int4": return "integer";
                case "int8": return "bigint";

                case "serial2": return "smallserial";
                case "serial4": return "serial";
                case "serial8": return "bigserial";

                case "float4": return "real";
                case "float8": return "double precision";

                case "varbit": return "bit varying";
                case "bool": return "boolean";
                case "decimal": return "numeric";

                case "char": return "character";
                case "varchar": return "character varying";

                case "time": return "time without time zone";
                case "timetz": return "time with time zone";
                case "timestamp": return "timestamp without time zone";
                case "timestamptz": return "timestamp with time zone";

                default: return dataTypeName;
            }
        }
    }
}
