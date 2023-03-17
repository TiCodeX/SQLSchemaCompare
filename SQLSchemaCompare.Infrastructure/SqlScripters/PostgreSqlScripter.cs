using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql;
using TiCodeX.SQLSchemaCompare.Core.Entities.Project;

namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Sql scripter class specific for PostgreSql database
    /// </summary>
    internal class PostgreSqlScripter : ADatabaseScripter<PostgreSqlScriptHelper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlScripter"/> class.
        /// </summary>
        /// <param name="logger">The injected logger instance</param>
        /// <param name="options">The project options</param>
        public PostgreSqlScripter(ILogger logger, ProjectOptions options)
            : base(logger, options, new PostgreSqlScriptHelper(options))
        {
        }

        /// <inheritdoc/>
        protected override string ScriptCreateSchema(ABaseDbSchema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE SCHEMA {this.ScriptHelper.ScriptObjectName(schema.Name)} AUTHORIZATION {this.ScriptHelper.ScriptObjectName(schema.Owner)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropSchema(ABaseDbSchema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP SCHEMA {this.ScriptHelper.ScriptObjectName(schema.Name)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterSchema(ABaseDbSchema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER SCHEMA {this.ScriptHelper.ScriptObjectName(schema.Name)} OWNER TO {this.ScriptHelper.ScriptObjectName(schema.Owner)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateTable(ABaseDbTable table)
        {
            var tablePostgreSql = table as PostgreSqlTable;
            if (tablePostgreSql == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var ncol = table.Columns.Count;
            var columns = this.GetSortedTableColumns(table);

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {this.ScriptHelper.ScriptObjectName(table)}(");

            var i = 0;
            foreach (var col in columns)
            {
                sb.Append($"{this.Indent}{this.ScriptHelper.ScriptColumn(col)}");
                sb.AppendLine(++i == ncol ? string.Empty : ",");
            }

            sb.Append(")");
            if (!string.IsNullOrWhiteSpace(tablePostgreSql.InheritedTableName))
            {
                sb.AppendLine();
                sb.Append($"INHERITS ({this.ScriptHelper.ScriptObjectName(tablePostgreSql.InheritedTableSchema, tablePostgreSql.InheritedTableName)})");
            }

            sb.AppendLine(";");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TABLE {this.ScriptHelper.ScriptObjectName(table)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTable(ABaseDbTable t)
        {
            var sb = new StringBuilder();

            var targetTable = t.MappedDbObject as ABaseDbTable;
            if (targetTable == null)
            {
                throw new ArgumentException($"{nameof(t.MappedDbObject)} is null");
            }

            // Remove columns
            foreach (var c in targetTable.Columns.Where(x => x.MappedDbObject == null))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} DROP COLUMN {this.ScriptHelper.ScriptObjectName(c.Name)};");
            }

            // Alter columns
            foreach (var c in t.Columns.Where(x => x.MappedDbObject != null && x.CreateScript != x.MappedDbObject.CreateScript))
            {
                var col = c as PostgreSqlColumn;
                var mappedCol = c.MappedDbObject as PostgreSqlColumn;
                if (col == null || mappedCol == null)
                {
                    throw new ArgumentException("Wrong column type");
                }

                if (col.DataType != mappedCol.DataType)
                {
                    sb.Append($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} ALTER COLUMN ");
                    sb.AppendLine($"{this.ScriptHelper.ScriptObjectName(col.Name)} TYPE {this.ScriptHelper.ScriptDataType(col)};");
                }

                if (col.IsNullable != mappedCol.IsNullable)
                {
                    sb.Append($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} ALTER COLUMN ");
                    sb.AppendLine($"{this.ScriptHelper.ScriptObjectName(col.Name)} {(mappedCol.IsNullable ? "DROP" : "SET")} IS NULLABLE;");
                }

                if (col.ColumnDefault != mappedCol.ColumnDefault)
                {
                    if (string.IsNullOrWhiteSpace(col.ColumnDefault))
                    {
                        sb.Append($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} ALTER COLUMN ");
                        sb.AppendLine($"{this.ScriptHelper.ScriptObjectName(col.Name)} DROP DEFAULT;");
                    }
                    else
                    {
                        sb.Append($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} ALTER COLUMN ");
                        sb.AppendLine($"{this.ScriptHelper.ScriptObjectName(col.Name)} SET DEFAULT {col.ColumnDefault};");
                    }
                }
            }

            // Add columns
            foreach (var c in t.Columns.Where(x => x.MappedDbObject == null))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} ADD {this.ScriptHelper.ScriptColumn(c)};");
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddPrimaryKey(ABaseDbPrimaryKey primaryKey)
        {
            var sb = new StringBuilder();

            var columnList = primaryKey.ColumnNames.Select(x => $"{this.ScriptHelper.ScriptObjectName(x)}");

            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(primaryKey.TableSchema, primaryKey.TableName)}");
            sb.AppendLine($"ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(primaryKey.Name)} PRIMARY KEY ({string.Join(",", columnList)});");

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropPrimaryKey(ABaseDbPrimaryKey primaryKey)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(primaryKey.TableSchema, primaryKey.TableName)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(primaryKey.Name)};");
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterPrimaryKey(ABaseDbPrimaryKey sourcePrimaryKey, ABaseDbPrimaryKey targetPrimaryKey)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptAlterTableDropPrimaryKey(targetPrimaryKey));
            sb.Append(this.ScriptAlterTableAddPrimaryKey(sourcePrimaryKey));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddForeignKey(ABaseDbForeignKey foreignKey)
        {
            var key = (PostgreSqlForeignKey)foreignKey;

            var sb = new StringBuilder();

            var columnList = key.ColumnNames.Select(x => $"{this.ScriptHelper.ScriptObjectName(x)}");
            var referencedColumnList = key.ReferencedColumnNames.Select(x => $"{this.ScriptHelper.ScriptObjectName(x)}");

            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(key.TableSchema, key.TableName)}");
            sb.AppendLine($"ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)} FOREIGN KEY ({string.Join(",", columnList)})");
            sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptObjectName(key.ReferencedTableSchema, key.ReferencedTableName)} ({string.Join(",", referencedColumnList)}) {PostgreSqlScriptHelper.ScriptForeignKeyMatchOption(key.MatchOption)}");
            sb.AppendLine($"ON DELETE {key.DeleteRule}");
            sb.AppendLine($"ON UPDATE {key.UpdateRule}");
            sb.AppendLine(key.IsDeferrable ? "DEFERRABLE" : "NOT DEFERRABLE");
            sb.AppendLine(key.IsInitiallyDeferred ? "INITIALLY DEFERRED;" : "INITIALLY IMMEDIATE;");

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropForeignKey(ABaseDbForeignKey foreignKey)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(foreignKey.TableSchema, foreignKey.TableName)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(foreignKey.Name)};");
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterForeignKey(ABaseDbForeignKey sourceForeignKey, ABaseDbForeignKey targetForeignKey)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptAlterTableDropForeignKey(targetForeignKey));
            sb.Append(this.ScriptAlterTableAddForeignKey(sourceForeignKey));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddConstraint(ABaseDbConstraint constraint)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(constraint.TableSchema, constraint.TableName)}");
            sb.AppendLine($"ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(constraint.Name)} {constraint.Definition};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropConstraint(ABaseDbConstraint constraint)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(constraint.TableSchema, constraint.TableName)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(constraint.Name)};");
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterConstraint(ABaseDbConstraint sourceConstraint, ABaseDbConstraint targetConstraint)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptAlterTableDropConstraint(targetConstraint));
            sb.Append(this.ScriptAlterTableAddConstraint(sourceConstraint));
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterTableAddPeriod(ABaseDbTable table)
        {
            throw new NotSupportedException("PostgreSQL doesn't support periods");
        }

        /// <inheritdoc />
        protected override string ScriptAlterTableDropPeriod(ABaseDbTable table)
        {
            throw new NotSupportedException("PostgreSQL doesn't support periods");
        }

        /// <inheritdoc />
        protected override string ScriptAlterPeriod(ABaseDbTable sourceTable, ABaseDbTable targetTable)
        {
            throw new NotSupportedException("PostgreSQL doesn't support periods");
        }

        /// <inheritdoc />
        protected override string ScriptAlterTableAddHistory(ABaseDbTable table)
        {
            throw new NotSupportedException("PostgreSQL doesn't support the history");
        }

        /// <inheritdoc />
        protected override string ScriptAlterTableDropHistory(ABaseDbTable table)
        {
            throw new NotSupportedException("PostgreSQL doesn't support the history");
        }

        /// <inheritdoc />
        protected override string ScriptAlterHistory(ABaseDbTable sourceTable, ABaseDbTable targetTable)
        {
            throw new NotSupportedException("PostgreSQL doesn't support the history");
        }

        /// <inheritdoc/>
        protected override string ScriptCreateIndex(ABaseDbIndex index)
        {
            var indexPostgreSql = (PostgreSqlIndex)index;

            var sb = new StringBuilder();

            // If there is a column with descending order, specify the order on all columns
            var scriptOrder = index.ColumnDescending.Any(x => x);
            var columnList = index.ColumnNames.Select((x, i) => scriptOrder ?
                $"{this.ScriptHelper.ScriptObjectName(x)} {(index.ColumnDescending[i] ? "DESC" : "ASC")}" :
                $"{this.ScriptHelper.ScriptObjectName(x)}");

            sb.Append("CREATE ");
            if (indexPostgreSql.IsUnique)
            {
                sb.Append("UNIQUE ");
            }

            sb.Append($"INDEX {index.Name} ON {this.ScriptHelper.ScriptObjectName(index.TableSchema, index.TableName)} ");

            switch (indexPostgreSql.Type)
            {
                case "btree":
                    // If not specified it will use the BTREE
                    break;

                case "gist":
                    sb.Append("USING gist ");
                    break;

                case "hash":
                    sb.Append("USING hash ");
                    break;

                default:
                    throw new NotSupportedException($"Index of type '{indexPostgreSql.Type}' is not supported");
            }

            sb.AppendLine($"({string.Join(",", columnList)});");

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropIndex(ABaseDbIndex index)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP INDEX {this.ScriptHelper.ScriptObjectName(index)};");
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterIndex(ABaseDbIndex sourceIndex, ABaseDbIndex targetIndex)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptDropIndex(targetIndex));
            sb.Append(this.ScriptCreateIndex(sourceIndex));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateView(ABaseDbView view)
        {
            var sb = new StringBuilder();

            var checkOption = ((PostgreSqlView)view).CheckOption;
            if (checkOption == "NONE")
            {
                sb.AppendLine($"CREATE VIEW {this.ScriptHelper.ScriptObjectName(view)} AS");
            }
            else
            {
                sb.AppendLine($"CREATE VIEW {this.ScriptHelper.ScriptObjectName(view)}");
                sb.AppendLine("WITH(");
                sb.AppendLine($"{this.Indent}CHECK_OPTION = {checkOption}");
                sb.AppendLine(") AS");
            }

            sb.AppendLine($"{view.ViewDefinition}");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropView(ABaseDbView view)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP VIEW {this.ScriptHelper.ScriptObjectName(view)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterView(ABaseDbView sourceView, ABaseDbView targetView)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptDropView(targetView));
            sb.Append(this.ScriptCreateView(sourceView));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateFunction(ABaseDbFunction sqlFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();

            var function = (PostgreSqlFunction)sqlFunction;

            var args = function.AllArgTypes != null ? function.AllArgTypes.ToArray() : function.ArgTypes.ToArray();

            sb.Append($"CREATE {(function.IsAggregate ? "AGGREGATE" : "FUNCTION")} {this.ScriptHelper.ScriptObjectName(function)}(");

            for (var i = 0; i < args.Length; i++)
            {
                var argType = args[i];
                var argMode = function.ArgModes?.ToArray()[i] ?? 'i';
                var argName = function.ArgNames?.ToArray()[i] ?? string.Empty;
                sb.Append($"{PostgreSqlScriptHelper.ScriptFunctionArgument(argType, argMode, argName, dataTypes)}");
                if (i != args.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.AppendLine(")");

            if (function.IsAggregate)
            {
                sb.AppendLine("(");
                sb.AppendLine($"{this.Indent}SFUNC = {function.AggregateTransitionFunction},");
                sb.Append($"{this.Indent}STYPE = {PostgreSqlScriptHelper.ScriptFunctionArgumentType(function.AggregateTransitionType, dataTypes)}");

                if (!string.IsNullOrWhiteSpace(function.AggregateFinalFunction))
                {
                    sb.AppendLine(",");
                    sb.Append($"{this.Indent}FINALFUNC = {function.AggregateFinalFunction}");
                }

                if (!string.IsNullOrWhiteSpace(function.AggregateInitialValue))
                {
                    sb.AppendLine(",");
                    sb.Append($"{this.Indent}INITCOND = '{function.AggregateInitialValue}'");
                }

                sb.AppendLine();
                sb.AppendLine(");");
            }
            else
            {
                var setOfString = function.ReturnSet ? "SETOF " : string.Empty;

                sb.AppendLine($"{this.Indent}RETURNS {setOfString}{PostgreSqlScriptHelper.ScriptFunctionArgumentType(function.ReturnType, dataTypes)}");
                sb.AppendLine($"{this.Indent}LANGUAGE {function.ExternalLanguage}");
                sb.AppendLine();
                sb.AppendLine($"{this.Indent}COST {function.Cost}");

                if (function.Rows > 0)
                {
                    sb.AppendLine($"{this.Indent}ROWS {function.Rows}");
                }

                sb.AppendLine($"{this.Indent}{PostgreSqlScriptHelper.ScriptFunctionAttributes(function)}");
                sb.Append("AS $BODY$");
                sb.Append(function.Definition);
                sb.AppendLine("$BODY$;");
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropFunction(ABaseDbFunction sqlFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var function = (PostgreSqlFunction)sqlFunction;
            var args = function.AllArgTypes != null ? function.AllArgTypes.ToArray() : function.ArgTypes.ToArray();

            var sb = new StringBuilder();
            sb.Append($"DROP {(function.IsAggregate ? "AGGREGATE" : "FUNCTION")} {this.ScriptHelper.ScriptObjectName(sqlFunction)}(");

            for (var i = 0; i < args.Length; i++)
            {
                var argType = args[i];
                var argMode = function.ArgModes?.ToArray()[i] ?? 'i';
                var argName = function.ArgNames?.ToArray()[i] ?? string.Empty;
                sb.Append($"{PostgreSqlScriptHelper.ScriptFunctionArgument(argType, argMode, argName, dataTypes)}");
                if (i != args.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.AppendLine(");");

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterFunction(ABaseDbFunction sourceFunction, ABaseDbFunction targetFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptDropFunction(targetFunction, dataTypes));
            sb.Append(this.ScriptCreateFunction(sourceFunction, dataTypes));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateStoredProcedure(ABaseDbStoredProcedure storedProcedure)
        {
            throw new NotSupportedException("PostgreSQL doesn't have stored procedures, only functions");
        }

        /// <inheritdoc/>
        protected override string ScriptDropStoredProcedure(ABaseDbStoredProcedure storedProcedure)
        {
            throw new NotSupportedException("PostgreSQL doesn't have stored procedures, only functions");
        }

        /// <inheritdoc/>
        protected override string ScriptAlterStoredProcedure(ABaseDbStoredProcedure sourceStoredProcedure, ABaseDbStoredProcedure targetStoredProcedure)
        {
            throw new NotSupportedException("PostgreSQL doesn't have stored procedures, only functions");
        }

        /// <inheritdoc/>
        protected override string ScriptCreateTrigger(ABaseDbTrigger trigger)
        {
            var sb = new StringBuilder();
            sb.Append($"{trigger.Definition}");
            if (!trigger.Definition.EndsWith(";", StringComparison.Ordinal))
            {
                sb.Append(";");
            }

            sb.AppendLine();

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropTrigger(ABaseDbTrigger trigger)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TRIGGER {this.ScriptHelper.ScriptObjectName(trigger.Name)} ON {this.ScriptHelper.ScriptObjectName(trigger.TableSchema, trigger.TableName)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTrigger(ABaseDbTrigger sourceTrigger, ABaseDbTrigger targetTrigger)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptDropTrigger(targetTrigger));
            sb.Append(this.ScriptCreateTrigger(sourceTrigger));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateSequence(ABaseDbSequence sequence)
        {
            var sequencePgsql = sequence as PostgreSqlSequence;
            if (sequencePgsql == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE SEQUENCE {this.ScriptHelper.ScriptObjectName(sequence)}");

            if (sequence.Database.ServerVersion.Major >= 10)
            {
                sb.AppendLine($"{this.Indent}AS {sequence.DataType}");
            }

            sb.AppendLine($"{this.Indent}START WITH {sequence.StartValue}");
            sb.AppendLine($"{this.Indent}INCREMENT BY {sequence.Increment}");
            sb.AppendLine($"{this.Indent}MINVALUE {sequence.MinValue}");
            sb.AppendLine($"{this.Indent}MAXVALUE {sequence.MaxValue}");
            sb.AppendLine(sequence.IsCycling ?
                $"{this.Indent}CYCLE" :
                $"{this.Indent}NO CYCLE");
            sb.AppendLine($"{this.Indent}CACHE {sequencePgsql.Cache};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropSequence(ABaseDbSequence sequence)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP SEQUENCE {this.ScriptHelper.ScriptObjectName(sequence)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterSequence(ABaseDbSequence sourceSequence, ABaseDbSequence targetSequence)
        {
            var sourceSequencePostgreSql = sourceSequence as PostgreSqlSequence;
            if (sourceSequencePostgreSql == null)
            {
                throw new ArgumentNullException(nameof(sourceSequence));
            }

            var targetSequencePostgreSql = targetSequence as PostgreSqlSequence;
            if (targetSequencePostgreSql == null)
            {
                throw new ArgumentNullException(nameof(targetSequence));
            }

            var sb = new StringBuilder();
            sb.AppendLine($"ALTER SEQUENCE {this.ScriptHelper.ScriptObjectName(targetSequence)}");

            var appendNewLine = false;
            if (sourceSequence.DataType != targetSequence.DataType)
            {
                sb.Append($"{this.Indent}AS {sourceSequence.DataType}");
                appendNewLine = true;
            }

            if (sourceSequence.Increment != targetSequence.Increment)
            {
                if (appendNewLine)
                {
                    sb.AppendLine();
                }

                sb.Append($"{this.Indent}INCREMENT BY {sourceSequence.Increment}");
                appendNewLine = true;
            }

            if (sourceSequence.MinValue != targetSequence.MinValue)
            {
                if (appendNewLine)
                {
                    sb.AppendLine();
                }

                sb.Append($"{this.Indent}MINVALUE {sourceSequence.MinValue}");
                appendNewLine = true;
            }

            if (sourceSequence.MaxValue != targetSequence.MaxValue)
            {
                if (appendNewLine)
                {
                    sb.AppendLine();
                }

                sb.Append($"{this.Indent}MAXVALUE {sourceSequence.MaxValue}");
                appendNewLine = true;
            }

            if (sourceSequence.StartValue != targetSequence.StartValue)
            {
                if (appendNewLine)
                {
                    sb.AppendLine();
                }

                sb.Append($"{this.Indent}START WITH {sourceSequence.StartValue}");
                appendNewLine = true;
            }

            if (sourceSequencePostgreSql.Cache != targetSequencePostgreSql.Cache)
            {
                if (appendNewLine)
                {
                    sb.AppendLine();
                }

                sb.Append($"{this.Indent}CACHE {sourceSequencePostgreSql.Cache}");
                appendNewLine = true;
            }

            if (sourceSequence.IsCycling != targetSequence.IsCycling)
            {
                if (appendNewLine)
                {
                    sb.AppendLine();
                }

                sb.Append(sourceSequence.IsCycling ?
                    $"{this.Indent}CYCLE" :
                    $"{this.Indent}NO CYCLE");
            }

            sb.AppendLine(";");
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptCreateType(ABaseDbDataType type, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();
            switch (type)
            {
                case PostgreSqlDataTypeEnumerated typeEnumerated:
                    sb.Append($"CREATE TYPE {this.ScriptHelper.ScriptObjectName(typeEnumerated)} AS ENUM (");

                    var labels = typeEnumerated.Labels.ToArray();
                    for (var i = 0; i < labels.Length; i++)
                    {
                        sb.AppendLine();
                        sb.Append($"{this.Indent}'{labels[i]}'");
                        if (i != labels.Length - 1)
                        {
                            sb.Append(",");
                        }
                    }

                    sb.AppendLine();
                    sb.AppendLine(");");
                    break;

                case PostgreSqlDataTypeComposite typeComposite:
                    sb.Append($"CREATE TYPE {this.ScriptHelper.ScriptObjectName(typeComposite)} AS (");

                    var attributeNames = typeComposite.AttributeNames.ToArray();
                    var attributeTypeIds = typeComposite.AttributeTypeIds.ToArray();
                    for (var i = 0; i < attributeNames.Length; i++)
                    {
                        sb.AppendLine();
                        sb.Append($"{this.Indent}{attributeNames[i]} {PostgreSqlScriptHelper.ScriptFunctionArgumentType(attributeTypeIds[i], dataTypes)}");
                        if (i != attributeNames.Length - 1)
                        {
                            sb.Append(",");
                        }
                    }

                    sb.AppendLine();
                    sb.AppendLine(");");
                    break;

                case PostgreSqlDataTypeRange typeRange:
                    sb.AppendLine($"CREATE TYPE {this.ScriptHelper.ScriptObjectName(typeRange)} AS RANGE (");
                    sb.Append($"{this.Indent}SUBTYPE = {PostgreSqlScriptHelper.ScriptFunctionArgumentType(typeRange.SubTypeId, dataTypes)}");
                    if (!string.IsNullOrEmpty(typeRange.Canonical))
                    {
                        sb.AppendLine(",");
                        sb.Append($"{this.Indent}CANONICAL = {typeRange.Canonical}");
                    }

                    if (!string.IsNullOrEmpty(typeRange.SubTypeDiff))
                    {
                        sb.AppendLine(",");
                        sb.Append($"{this.Indent}SUBTYPE_DIFF = {typeRange.SubTypeDiff}");
                    }

                    sb.AppendLine();
                    sb.AppendLine(");");
                    break;

                case PostgreSqlDataTypeDomain typeDomain:
                    sb.AppendLine($"CREATE DOMAIN {this.ScriptHelper.ScriptObjectName(typeDomain)}");
                    sb.AppendLine($"{this.Indent}AS {PostgreSqlScriptHelper.ScriptFunctionArgumentType(typeDomain.BaseTypeId, dataTypes)}");
                    if (!string.IsNullOrEmpty(typeDomain.ConstraintName))
                    {
                        sb.AppendLine($"{this.Indent}CONSTRAINT {this.ScriptHelper.ScriptObjectName(string.Empty, typeDomain.ConstraintName)}");
                    }

                    sb.Append(string.IsNullOrEmpty(typeDomain.ConstraintDefinition)
                        ? $"{this.Indent}{(typeDomain.NotNull ? "NOT NULL" : "NULL")}"
                        : $"{this.Indent}{typeDomain.ConstraintDefinition}");
                    sb.AppendLine(";");

                    break;

                default:
                    throw new NotSupportedException();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropType(ABaseDbDataType type)
        {
            var sb = new StringBuilder();
            switch (type)
            {
                case PostgreSqlDataTypeEnumerated _:
                case PostgreSqlDataTypeComposite _:
                case PostgreSqlDataTypeRange _:
                    sb.AppendLine($"DROP TYPE {this.ScriptHelper.ScriptObjectName(type)};");
                    break;

                case PostgreSqlDataTypeDomain _:
                    sb.AppendLine($"DROP DOMAIN {this.ScriptHelper.ScriptObjectName(type)};");
                    break;
                default:
                    throw new NotSupportedException();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterType(ABaseDbDataType sourceType, ABaseDbDataType targetType, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptDropType(targetType));
            sb.Append(this.ScriptCreateType(sourceType, dataTypes));
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbTable> GetSortedTables(List<ABaseDbTable> tables, bool dropOrder)
        {
            var tablesPostgreSql = tables.Cast<PostgreSqlTable>().OrderBy(x => x.Schema).ThenBy(x => x.Name).ToList();

            var sortedTables = new List<PostgreSqlTable>();
            foreach (var table in tablesPostgreSql)
            {
                if (string.IsNullOrWhiteSpace(table.InheritedTableName))
                {
                    if (!sortedTables.Contains(table))
                    {
                        sortedTables.Add(table);
                    }
                }
                else
                {
                    List<PostgreSqlTable> GetInheritedTables(PostgreSqlTable t)
                    {
                        var inheritedTable = tablesPostgreSql.FirstOrDefault(x => x.Schema == t.InheritedTableSchema && x.Name == t.InheritedTableName);
                        if (inheritedTable == null)
                        {
                            throw new KeyNotFoundException($"Unable to find inherited table {this.ScriptHelper.ScriptObjectName(t.InheritedTableSchema, t.InheritedTableName)}");
                        }

                        var inheritedTables = new List<PostgreSqlTable>();
                        if (!sortedTables.Contains(inheritedTable))
                        {
                            if (string.IsNullOrWhiteSpace(inheritedTable.InheritedTableName))
                            {
                                inheritedTables.Add(inheritedTable);
                            }
                            else
                            {
                                inheritedTables.AddRange(GetInheritedTables(inheritedTable).FindAll(x => !sortedTables.Contains(x)));
                            }
                        }

                        inheritedTables.Add(t);
                        return inheritedTables;
                    }

                    sortedTables.AddRange(GetInheritedTables(table).FindAll(x => !sortedTables.Contains(x)));
                }
            }

            if (dropOrder)
            {
                sortedTables.Reverse();
            }

            return sortedTables;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbColumn> OrderColumnsByOrdinalPosition(ABaseDbTable table)
        {
            return table.Columns.OrderBy(x => ((PostgreSqlColumn)x).OrdinalPosition);
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbFunction> GetSortedFunctions(List<ABaseDbFunction> functions, bool dropOrder)
        {
            return dropOrder ?
                functions.Cast<PostgreSqlFunction>().OrderByDescending(x => x.IsAggregate).ThenBy(x => x.Schema).ThenBy(x => x.Name) :
                functions.Cast<PostgreSqlFunction>().OrderBy(x => x.IsAggregate).ThenBy(x => x.Schema).ThenBy(x => x.Name);
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbIndex> GetSortedIndexes(List<ABaseDbIndex> indexes)
        {
            return indexes.OrderBy(x => x.Schema).ThenBy(x => x.Name);
        }
    }
}
