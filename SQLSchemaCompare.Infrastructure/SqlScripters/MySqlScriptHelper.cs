namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Text.RegularExpressions;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Project;

    /// <summary>
    /// Script helper class specific for MySql database
    /// </summary>
    public class MySqlScriptHelper : AScriptHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlScriptHelper"/> class.
        /// </summary>
        /// <param name="options">The project options</param>
        public MySqlScriptHelper(ProjectOptions options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        public override string ScriptObjectName(string objectSchema, string objectName)
        {
            return $"`{objectName}`";
        }

        /// <inheritdoc/>
        public override string ScriptColumn(ABaseDbColumn column, bool scriptDefaultConstraint = true)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var col = (MySqlColumn)column;

            var sb = new StringBuilder();
            sb.Append($"`{col.Name}` {this.ScriptDataType(col)}");

            if (col.Extra.Equals("VIRTUAL GENERATED", StringComparison.OrdinalIgnoreCase))
            {
                return sb.ToString();
            }

            sb.Append(col.IsNullable == "YES" ? " NULL" : " NOT NULL");

            // Always script the default if present
            if (!string.IsNullOrWhiteSpace(col.ColumnDefault))
            {
                switch (col.DataType)
                {
                    // Character strings
                    case "enum":
                    case "char":
                    case "varchar":
                    case "text":
                    case "tinytext":
                    case "mediumtext":
                    case "longtext":
                        sb.Append($" DEFAULT '{col.ColumnDefault.Trim('\'')}'");
                        break;
                    default:
                        sb.Append($" DEFAULT {col.ColumnDefault}");
                        break;
                }
            }

            // Append the Extra properties only if it's not auto_increment because it has
            // to be defined when the primary key is added
            if (!string.IsNullOrWhiteSpace(col.Extra) &&
                col.Extra.ToUpperInvariant() != "AUTO_INCREMENT")
            {
                var extra = col.Extra;
                if (extra.Contains("DEFAULT_GENERATED", StringComparison.OrdinalIgnoreCase))
                {
                    extra = Regex.Replace(extra, "DEFAULT_GENERATED\\s", string.Empty, RegexOptions.IgnoreCase);
                }

                sb.Append($" {extra}");
            }

            return sb.ToString();
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
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Switch with lot of cases")]
        private string ScriptDataType(MySqlColumn column)
        {
            if (column.Extra.Equals("VIRTUAL GENERATED", StringComparison.OrdinalIgnoreCase))
            {
                return $"{column.ColumnType} AS {column.GenerationExpression} VIRTUAL";
            }

            if (column.Extra.Equals("STORED GENERATED", StringComparison.OrdinalIgnoreCase))
            {
                return $"{column.ColumnType} AS {column.GenerationExpression} PERSISTENT";
            }

            switch (column.DataType)
            {
                // Exact numerics
                case "bit":
                case "tinyint":
                case "smallint":
                case "mediumint":
                case "int":
                case "integer":
                case "bigint":
                case "numeric":
                case "decimal":

                // Approximate numerics
                case "real":
                case "double":
                case "float":

                // Date and time
                case "date":
                case "year":
                case "time":
                case "timestamp":
                case "datetime":

                // Binary strings
                case "binary":
                case "varbinary":
                case "tinyblob":
                case "blob":
                case "mediumblob":
                case "longblob":

                // Other data types
                case "enum":
                case "set":
                case "json":
                case "geometry":
                case "point":
                case "linestring":
                case "polygon":
                case "multipoint":
                case "multilinestring":
                case "multipolygon":
                case "geometrycollection":
                    return $"{column.ColumnType}";

                // Character strings
                case "char":
                case "varchar":
                case "text":
                case "tinytext":
                case "mediumtext":
                case "longtext":
                    {
                        var collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        var binary = column.CollationName == column.CharacterSetName + "_bin" ? " BINARY" : string.Empty;
                        var charachterSet = $" CHARACTER SET {column.CharacterSetName}";
                        return $"{column.ColumnType}{binary}{charachterSet}{collate}";
                    }

                default: throw new ArgumentException($"Unknown column data type: {column.DataType}");
            }
        }
    }
}
