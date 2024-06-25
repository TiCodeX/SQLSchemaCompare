namespace TiCodeX.SQLSchemaCompare.Test.Infrastructure.SqlScripter
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
            var scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions { Scripting = new ScriptingOptions { OrderColumnAlphabetically = true } });
            var exposedScripter = Exposed.From(scripter);

            var table = new MicrosoftSqlTable();
            table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

            table.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Source };

            var columns = (IEnumerable<ABaseDbColumn>)exposedScripter.GetSortedTableColumns(table);
            columns.Select(x => x.Name).Should().ContainInOrder("a", "b", "c", "d", "e");
        }

        /// <summary>
        /// Test for the GetSortedTableColumns (default uses column ordinal position)
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsByOrdinalPosition()
        {
            var scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions { Scripting = new ScriptingOptions { OrderColumnAlphabetically = false } });
            var exposedScripter = Exposed.From(scripter);

            var table = new MicrosoftSqlTable();
            table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

            table.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Source };

            var columns = (IEnumerable<ABaseDbColumn>)exposedScripter.GetSortedTableColumns(table);
            columns.Select(x => x.Name).Should().ContainInOrder("b", "e", "a", "d", "c");
        }

        /// <summary>
        /// Test for the GetSortedTableColumns with reference table sorted alphabetically
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsWithReferenceTableAlphabetically()
        {
            var scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions { Scripting = new ScriptingOptions { OrderColumnAlphabetically = true } });
            var exposedScripter = Exposed.From(scripter);

            var refTable = new MicrosoftSqlTable();
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "m", OrdinalPosition = 3 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "z", OrdinalPosition = 2 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 0 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 4 });

            refTable.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Source };

            var table = new MicrosoftSqlTable();
            table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

            table.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Target };
            table.MappedDbObject = refTable;

            var columns = (IEnumerable<ABaseDbColumn>)exposedScripter.GetSortedTableColumns(table);
            columns.Select(x => x.Name).Should().ContainInOrder("c", "d", "e", "a", "b");
        }

        /// <summary>
        /// Test for the GetSortedTableColumns with reference table sorted by ordinal position
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsWithReferenceTableByOrdinalPosition()
        {
            var scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions { Scripting = new ScriptingOptions { OrderColumnAlphabetically = false } });
            var exposedScripter = Exposed.From(scripter);

            var refTable = new MicrosoftSqlTable();
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "m", OrdinalPosition = 3 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "z", OrdinalPosition = 2 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 0 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 4 });

            refTable.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Source };

            var table = new MicrosoftSqlTable();
            table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

            table.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Target };
            table.MappedDbObject = refTable;

            var columns = (IEnumerable<ABaseDbColumn>)exposedScripter.GetSortedTableColumns(table);
            columns.Select(x => x.Name).Should().ContainInOrder("c", "e", "d", "b", "a");
        }

        /// <summary>
        /// Test for the GetSortedTableColumns with reference table sorted by ordinal position
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsWithReferenceTableByOrdinalPositionIgnoreReference()
        {
            var scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions { Scripting = new ScriptingOptions { OrderColumnAlphabetically = false, IgnoreReferenceTableColumnOrder = true } });
            var exposedScripter = Exposed.From(scripter);

            var refTable = new MicrosoftSqlTable();
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "m", OrdinalPosition = 3 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "z", OrdinalPosition = 2 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 0 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 4 });

            refTable.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Source };

            var table = new MicrosoftSqlTable();
            table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

            table.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Target };
            table.MappedDbObject = refTable;

            var columns = (IEnumerable<ABaseDbColumn>)exposedScripter.GetSortedTableColumns(table);
            columns.Select(x => x.Name).Should().ContainInOrder("b", "e", "a", "d", "c");
        }

        /// <summary>
        /// Test for the GetSortedTableColumns with reference table sorted alphabetically
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTableColumnsWithReferenceTableAlphabeticallyIgnoreReference()
        {
            var scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions { Scripting = new ScriptingOptions { OrderColumnAlphabetically = true, IgnoreReferenceTableColumnOrder = true } });
            var exposedScripter = Exposed.From(scripter);

            var refTable = new MicrosoftSqlTable();
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "m", OrdinalPosition = 3 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "z", OrdinalPosition = 2 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 0 });
            refTable.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 4 });

            refTable.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Source };

            var table = new MicrosoftSqlTable();
            table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
            table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

            table.Database = new MicrosoftSqlDb() { Direction = SQLSchemaCompare.Core.Enums.CompareDirection.Target };
            table.MappedDbObject = refTable;

            var columns = (IEnumerable<ABaseDbColumn>)exposedScripter.GetSortedTableColumns(table);
            columns.Select(x => x.Name).Should().ContainInOrder("a", "b", "c", "d", "e");
        }

        /// <summary>
        /// Test sorting the tables alphabetically
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTablesAlphabetically()
        {
            var scripter = new PostgreSqlScripter(this.Logger, new ProjectOptions());
            var exposedScripter = Exposed.From(scripter);

            var tables = new List<ABaseDbTable>
            {
                new PostgreSqlTable { Name = "e" },
                new PostgreSqlTable { Name = "a" },
                new PostgreSqlTable { Name = "b" },
                new PostgreSqlTable { Name = "d" },
                new PostgreSqlTable { Name = "c" },
            };

            var sortedTables = (List<PostgreSqlTable>)exposedScripter.GetSortedTables(tables, false);
            sortedTables.Select(x => x.Name).Should().ContainInOrder("a", "b", "c", "d", "e");
        }

        /// <summary>
        /// Test sorting the tables with inheritance
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSortedTablesWithInheritance()
        {
            var scripter = new PostgreSqlScripter(this.Logger, new ProjectOptions());
            var exposedScripter = Exposed.From(scripter);

            var tables = new List<ABaseDbTable>
            {
                new PostgreSqlTable { Name = "e" },
                new PostgreSqlTable { Name = "a", InheritedTableName = "c" },
                new PostgreSqlTable { Name = "b", InheritedTableName = "a" },
                new PostgreSqlTable { Name = "d" },
                new PostgreSqlTable { Name = "c" },
            };

            var sortedTables = (List<PostgreSqlTable>)exposedScripter.GetSortedTables(tables, false);
            sortedTables.Select(x => x.Name).Should().ContainInOrder("c", "a", "b", "d", "e");

            // More complicated test (based on https://qt-wiki-uploads.s3.amazonaws.com/images/4/4c/Beginner-Class-Hierarchy.jpg)
            tables = new List<ABaseDbTable>
            {
                new PostgreSqlTable { Name = "Object" },
                new PostgreSqlTable { Name = "Thread", InheritedTableName = "Object" },
                new PostgreSqlTable { Name = "Widget", InheritedTableName = "Object" },
                new PostgreSqlTable { Name = "AbstractButton", InheritedTableName = "Widget" },
                new PostgreSqlTable { Name = "Frame", InheritedTableName = "Widget" },
                new PostgreSqlTable { Name = "ProgressBar", InheritedTableName = "Widget" },
                new PostgreSqlTable { Name = "CheckBox", InheritedTableName = "AbstractButton" },
                new PostgreSqlTable { Name = "PushButton", InheritedTableName = "AbstractButton" },
                new PostgreSqlTable { Name = "RadioButton", InheritedTableName = "AbstractButton" },
                new PostgreSqlTable { Name = "Label", InheritedTableName = "Frame" },
                new PostgreSqlTable { Name = "AbstractScrollArea", InheritedTableName = "Frame" },
                new PostgreSqlTable { Name = "GraphicsView", InheritedTableName = "AbstractScrollArea" },
                new PostgreSqlTable { Name = "TextEdit", InheritedTableName = "AbstractScrollArea" },
            };
            tables = tables.OrderBy(a => Guid.NewGuid()).ToList();

            sortedTables = (List<PostgreSqlTable>)exposedScripter.GetSortedTables(tables, false);
            sortedTables.Select(x => x.Name).Should().ContainInOrder("Object", "Widget", "AbstractButton", "Frame", "AbstractScrollArea", "CheckBox", "GraphicsView", "Label", "ProgressBar", "PushButton", "RadioButton", "TextEdit", "Thread");
        }
    }
}
