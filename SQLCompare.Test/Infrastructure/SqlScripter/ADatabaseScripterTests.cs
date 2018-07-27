using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Infrastructure.SqlScripters;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Infrastructure.SqlScripter
{
    /// <summary>
    /// Test class for the abstract class ADatabaseScripter
    /// </summary>
    public class ADatabaseScripterTests : BaseTests<ADatabaseScripterTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ADatabaseScripterTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public ADatabaseScripterTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test for the GetSortedTableColumns when options specify for alphabetical order
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsAlphabetically()
        {
            MicrosoftSqlScripter scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions() { Scripting = new ScriptingOptions() { OrderColumnAlphabetically = true } });

            var table = new MicrosoftSqlTable() { };
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 3 });

            IEnumerable<ABaseDbColumn> columns = (IEnumerable<ABaseDbColumn>)scripter.GetType().GetTypeInfo().BaseType.GetTypeInfo().GetDeclaredMethod("GetSortedTableColumns").Invoke(scripter, new[] { table, null });
            columns.Select(x => x.Name).Should().ContainInOrder(new[] { "a", "b", "c", "d", "e" });
        }

        /// <summary>
        /// Test for the GetSortedTableColumns (default uses column ordinal position)
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsByOrdinalPosition()
        {
            MicrosoftSqlScripter scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions() { Scripting = new ScriptingOptions() { OrderColumnAlphabetically = false } });

            var table = new MicrosoftSqlTable() { };
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 3 });

            IEnumerable<ABaseDbColumn> columns = (IEnumerable<ABaseDbColumn>)scripter.GetType().GetTypeInfo().BaseType.GetTypeInfo().GetDeclaredMethod("GetSortedTableColumns").Invoke(scripter, new[] { table, null });
            columns.Select(x => x.Name).Should().ContainInOrder(new[] { "b", "e", "a", "d", "c" });
        }

        /// <summary>
        /// Test for the GetSortedTableColumns with reference table sorted alphabetically
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsWithReferenceTableAlphabetically()
        {
            MicrosoftSqlScripter scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions() { Scripting = new ScriptingOptions() { OrderColumnAlphabetically = true } });

            var refTable = new MicrosoftSqlTable() { };
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "m", OrdinalPosition = 3 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "z", OrdinalPosition = 2 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 0 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 4 });

            var table = new MicrosoftSqlTable() { };
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 3 });

            IEnumerable<ABaseDbColumn> columns = (IEnumerable<ABaseDbColumn>)scripter.GetType().GetTypeInfo().BaseType.GetTypeInfo().GetDeclaredMethod("GetSortedTableColumns").Invoke(scripter, new[] { table, refTable });
            columns.Select(x => x.Name).Should().ContainInOrder(new[] { "c", "d", "e", "a", "b" });
        }

        /// <summary>
        /// Test for the GetSortedTableColumns with reference table sorted by ordinal position
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsWithReferenceTableByOrdinalPosition()
        {
            MicrosoftSqlScripter scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions() { Scripting = new ScriptingOptions() { OrderColumnAlphabetically = false } });

            var refTable = new MicrosoftSqlTable() { };
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "m", OrdinalPosition = 3 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "z", OrdinalPosition = 2 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 0 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 4 });

            var table = new MicrosoftSqlTable() { };
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 3 });

            IEnumerable<ABaseDbColumn> columns = (IEnumerable<ABaseDbColumn>)scripter.GetType().GetTypeInfo().BaseType.GetTypeInfo().GetDeclaredMethod("GetSortedTableColumns").Invoke(scripter, new[] { table, refTable });
            columns.Select(x => x.Name).Should().ContainInOrder(new[] { "c", "e", "d", "b", "a" });
        }

        /// <summary>
        /// Test for the GetSortedTableColumns with reference table sorted by ordinal position
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsWithReferenceTableByOrdinalPositionIgnoreReference()
        {
            MicrosoftSqlScripter scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions() { Scripting = new ScriptingOptions() { OrderColumnAlphabetically = false, IgnoreReferenceTableColumnOrder = true } });

            var refTable = new MicrosoftSqlTable() { };
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "m", OrdinalPosition = 3 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "z", OrdinalPosition = 2 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 0 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 4 });

            var table = new MicrosoftSqlTable() { };
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 3 });

            IEnumerable<ABaseDbColumn> columns = (IEnumerable<ABaseDbColumn>)scripter.GetType().GetTypeInfo().BaseType.GetTypeInfo().GetDeclaredMethod("GetSortedTableColumns").Invoke(scripter, new[] { table, refTable });
            columns.Select(x => x.Name).Should().ContainInOrder(new[] { "b", "e", "a", "d", "c" });
        }

        /// <summary>
        /// Test for the GetSortedTableColumns with reference table sorted alphabetically
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsWithReferenceTableAlphabeticallyIgnoreReference()
        {
            MicrosoftSqlScripter scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions() { Scripting = new ScriptingOptions() { OrderColumnAlphabetically = true, IgnoreReferenceTableColumnOrder = true } });

            var refTable = new MicrosoftSqlTable() { };
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "m", OrdinalPosition = 3 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "z", OrdinalPosition = 2 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 0 });
            refTable.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 4 });

            var table = new MicrosoftSqlTable() { };
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn() { Name = "d", OrdinalPosition = 3 });

            IEnumerable<ABaseDbColumn> columns = (IEnumerable<ABaseDbColumn>)scripter.GetType().GetTypeInfo().BaseType.GetTypeInfo().GetDeclaredMethod("GetSortedTableColumns").Invoke(scripter, new[] { table, refTable });
            columns.Select(x => x.Name).Should().ContainInOrder(new[] { "a", "b", "c", "d", "e" });
        }
    }
}
