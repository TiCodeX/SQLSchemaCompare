using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Entities.Project;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Infrastructure.SqlScripters
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

        /// <inheritdoc/>
        public override string ScriptTableName(string tableSchema, string tableName)
        {
            if (this.Options.Scripting.UseSchemaName)
            {
                return $"\"{tableSchema}\".\"{tableName}\"";
            }
            else
            {
                return $"\"{tableName}\"";
            }
        }

        /// <inheritdoc/>
        public override string ScriptColumn(ABaseDbColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var col = column as PostgreSqlColumn;

            var sb = new StringBuilder();

            sb.Append($"\"{col.Name}\" {this.ScriptDataType(col)} ");

            if (col.IsNullable)
            {
                sb.Append($"NULL ");
            }
            else
            {
                sb.Append($"NOT NULL ");
            }

            if (col.ColumnDefault != null)
            {
                sb.Append($"DEFAULT {col.ColumnDefault} ");
            }

            return sb.ToString();
        }

        private string ScriptDataType(PostgreSqlColumn column)
        {
            switch (column.DataType)
            {
                // Exact numerics
                case "smallint":
                case "integer":
                case "bigint":
                case "decimal":
                    return $"{column.DataType}";

                case "numeric":
                    return $"{column.DataType}({column.NumericPrecision},{column.NumericScale})"; // TODO: check if it's possible to only specify numeric without params

                // Approximate numerics
                case "real":
                case "double precision":

                // TODO: check if float can be specified
                // case "float":
                // return $"{column.DataType}({column.NumericPrecision})";

                // Money
                case "money":
                    return $"{column.DataType}";

                // Character strings
                case "character":
                case "character varying":
                    {
                        string collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"{column.DataType}({column.CharacterMaxLenght}){collate}";
                    }

                case "text":
                    {
                        string collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"{column.DataType}{collate}";
                    }

                // Date and time
                case "date":
                    return $"{column.DataType}";
                case "time with time zone":
                    {
                        var precision = (column.DateTimePrecision != 6) ? $"({column.DateTimePrecision})" : string.Empty;
                        return $"time{precision} with time zone";
                    }

                case "time without time zone":
                    {
                        var precision = (column.DateTimePrecision != 6) ? $"({column.DateTimePrecision})" : string.Empty;
                        return $"time{precision} without time zone";
                    }

                case "timestamp with time zone":
                    {
                        var precision = (column.DateTimePrecision != 6) ? $"({column.DateTimePrecision})" : string.Empty;
                        return $"timestamp{precision} with time zone";
                    }

                case "timestamp without time zone":
                    {
                        var precision = (column.DateTimePrecision != 6) ? $"({column.DateTimePrecision})" : string.Empty;
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

                        return $"interval{precision}";
                    }

                // Binary strings
                case "bit":
                case "bit varying":
                    {
                        var precision = (column.CharacterMaxLenght != null) ? $"({column.CharacterMaxLenght})" : string.Empty;
                        return $"{column.DataType}{precision}";
                    }

                case "bytea":

                // Boolean
                case "boolean":
                    return $"{column.DataType}";

                // User defined types
                case "USER-DEFINED":
                    return $"{column.UdtName}";

                // Other data types
                case "point":
                case "line":
                case "lseg":
                case "box":
                case "path":
                case "polygon":
                case "circle":
                case "inet":
                case "cidr":
                case "macaddr":
                case "macaddr8":
                case "tsvector":
                case "tsquery":

                case "uuid":
                case "xml":
                case "json":
                case "jsonb":
                case "pg_lsn":
                case "txid_snapshot":

                    return $"{column.DataType}";
                case "ARRAY":
                    // TODO: Array type initialization does not consider parameters and no collation considered for string types
                    // E.g.: "myColumnName character varying(55)[4][4]"
                    // ==> currently will result in "myColumnName varchar[]"
                    return $"{column.UdtName.Replace("_", string.Empty)}[]";

                default: throw new ArgumentException($"Unknown column data type: {column.DataType}");
            }
        }
    }
}
