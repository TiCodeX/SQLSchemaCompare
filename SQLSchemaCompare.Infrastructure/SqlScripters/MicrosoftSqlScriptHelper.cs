﻿namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Script helper class specific for MicrosoftSql database
    /// </summary>
    public class MicrosoftSqlScriptHelper : AScriptHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlScriptHelper"/> class.
        /// </summary>
        /// <param name="options">The project options</param>
        public MicrosoftSqlScriptHelper(ProjectOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Script the foreign key reference action
        /// </summary>
        /// <param name="action">The reference action</param>
        /// <returns>The scripted action</returns>
        public static string ScriptForeignKeyAction(MicrosoftSqlForeignKey.ReferentialAction action)
        {
            switch (action)
            {
                case MicrosoftSqlForeignKey.ReferentialAction.NOACTION: return "NO ACTION";
                case MicrosoftSqlForeignKey.ReferentialAction.CASCADE: return "CASCADE";
                case MicrosoftSqlForeignKey.ReferentialAction.SETDEFAULT: return "SET DEFAULT";
                case MicrosoftSqlForeignKey.ReferentialAction.SETNULL: return "SET NULL";
                default:
                    throw new ArgumentException($"Invalid referential action: {action}", nameof(action));
            }
        }

        /// <inheritdoc/>
        public override string ScriptObjectName(string objectSchema, string objectName)
        {
            return !string.IsNullOrWhiteSpace(objectSchema) ?
                $"[{objectSchema}].[{objectName}]" :
                $"[{objectName}]";
        }

        /// <inheritdoc/>
        [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "TODO")]
        public override string ScriptColumn(ABaseDbColumn column, bool scriptDefaultConstraint)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var col = (MicrosoftSqlColumn)column;

            var sb = new StringBuilder();

            sb.Append($"{this.ScriptObjectName(col.Name)} {this.ScriptDataType(col)}");

            if (!col.IsComputed)
            {
                if (col.IsIdentity)
                {
                    sb.Append($" IDENTITY({col.IdentitySeed},{col.IdentityIncrement})");
                }

                if (col.GeneratedAlwaysType != 0)
                {
                    sb.Append(" GENERATED ALWAYS AS ");

                    switch (col.GeneratedAlwaysType)
                    {
                        case 1:
                            sb.Append("ROW START");
                            break;
                        case 2:
                            sb.Append("ROW END");
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }

                if (col.IsHidden)
                {
                    sb.Append(" HIDDEN");
                }

                sb.Append(col.IsNullable ? " NULL" : " NOT NULL");

                if (col.IsRowGuidCol)
                {
                    sb.Append(" ROWGUIDCOL");
                }

                if (!string.IsNullOrWhiteSpace(col.ColumnDefault) && scriptDefaultConstraint)
                {
                    if (!string.IsNullOrWhiteSpace(col.DefaultConstraintName))
                    {
                        sb.Append($" CONSTRAINT {this.ScriptObjectName(col.DefaultConstraintName)}");
                    }

                    sb.Append($" DEFAULT {col.ColumnDefault}");
                }
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
                case "bigint":
                case "int":
                case "smallint":
                case "tinyint":
                case "bit":
                case "smallmoney":
                case "money":
                case "numeric":
                case "decimal":
                    return "0";

                // Approximate numerics
                case "float":
                case "real":
                    return "0";

                // Date and time
                case "date":
                case "datetime":
                case "datetime2":
                case "datetimeoffset":
                case "smalldatetime":
                case "time":
                    return "GETUTCDATE()";

                // Character strings
                // Unicode character strings
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "text":
                case "ntext":
                    return "''";

                // Binary strings
                case "binary":
                case "varbinary":
                case "image":
                    return "0x00";

                // Other data types
                case "uniqueidentifier":
                    return "NEWID()";
                case "xml":
                    return "''";
                case "geography":
                case "geometry":
                    return "'POINT(0 0)'";
                case "cursor":
                case "rowversion":
                case "hierarchyid":
                case "sql_variant":
                    throw new NotSupportedException($"Unknown default value for data type: {column.DataType}");

                default:
                    throw new ArgumentException($"Unknown data type: {column.DataType}");
            }
        }

        /// <inheritdoc />
        public override string ScriptCommitTransaction()
        {
            var sb = new StringBuilder();
            sb.AppendLine("GO");
            return sb.ToString();
        }

        /// <summary>
        /// Script the column data type
        /// </summary>
        /// <param name="column">The column</param>
        /// <returns>The scripted data type</returns>
        [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "TODO")]
        private string ScriptDataType(MicrosoftSqlColumn column)
        {
            // Computed columns return AS + definition
            if (column.IsComputed)
            {
                return $"AS {column.Definition}";
            }

            if (!string.IsNullOrWhiteSpace(column.UserDefinedDataType))
            {
                return !string.IsNullOrWhiteSpace(column.UserDefinedDataTypeSchema) ?
                    $"{this.ScriptObjectName(column.UserDefinedDataTypeSchema, column.UserDefinedDataType)}" :
                    $"{this.ScriptObjectName(column.UserDefinedDataType)}";
            }

            switch (column.DataType)
            {
                // Exact numerics
                case "bigint":
                case "int":
                case "smallint":
                case "tinyint":
                case "bit":
                case "smallmoney":
                case "money":
                case "timestamp":
                    return this.ScriptObjectName(column.DataType);
                case "numeric":
                case "decimal":
                    return $"{this.ScriptObjectName(column.DataType)}({column.NumericPrecision}, {column.NumericScale})";

                // Approximate numerics
                case "float":
                    return column.NumericPrecision == 53 ? $"{this.ScriptObjectName(column.DataType)}" : $"{this.ScriptObjectName(column.DataType)}({column.NumericPrecision})";
                case "real":
                    return this.ScriptObjectName(column.DataType);

                // Date and time
                case "date":
                case "datetime":
                case "smalldatetime":
                    return this.ScriptObjectName(column.DataType);
                case "datetimeoffset":
                case "datetime2":
                case "time":
                    return $"{this.ScriptObjectName(column.DataType)}({column.DateTimePrecision})";

                // Character strings
                // Unicode character strings
                case "char":
                case "nchar":
                    {
                        var collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"{this.ScriptObjectName(column.DataType)}({column.CharacterMaxLength}){collate}";
                    }

                case "varchar":
                case "nvarchar":
                    {
                        var length = column.CharacterMaxLength == -1 ? "max" : $"{column.CharacterMaxLength}";
                        var collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";

                        return $"{this.ScriptObjectName(column.DataType)}({length}){collate}";
                    }

                case "text":
                case "ntext":
                    {
                        var collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"{this.ScriptObjectName(column.DataType)}{collate}";
                    }

                // Binary strings
                case "binary":
                    return $"{this.ScriptObjectName(column.DataType)}({column.CharacterMaxLength})";
                case "varbinary":
                    {
                        var length = column.CharacterMaxLength == -1 ? "max" : $"{column.CharacterMaxLength}";
                        return $"{this.ScriptObjectName(column.DataType)}({length})";
                    }

                case "image":
                    return this.ScriptObjectName(column.DataType);

                // Other data types
                case "cursor":
                case "rowversion":
                case "hierarchyid":
                case "uniqueidentifier":
                case "sql_variant":
                case "xml":
                case "geography":
                case "geometry":
                    return this.ScriptObjectName(column.DataType);
                default: throw new ArgumentException($"Unknown data type: {column.DataType}");
            }
        }
    }
}
