namespace TiCodeX.SQLSchemaCompare.Test.Infrastructure.SqlScripter;

using System.Runtime.CompilerServices;

/// <summary>
/// Test class for the abstract class ADatabaseScripter
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ADatabaseScripterTests"/> class.
/// </remarks>
/// <param name="output">The test output helper</param>
public class ADatabaseScripterTests(ITestOutputHelper output) : BaseTests<ADatabaseScripterTests>(output)
{
    /// <summary>
    /// Test for the GetSortedTableColumns when options specify for alphabetical order
    /// </summary>
    [Fact]
    [UnitTest]
    public void GetSortedTableColumnsAlphabetically()
    {
        var scripter = new MicrosoftSqlScripter(this.Logger, new ProjectOptions { Scripting = new ScriptingOptions { OrderColumnAlphabetically = true } });

        var table = new MicrosoftSqlTable();
        table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

        table.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Source };

        var columns = ScripterAccessorHelper<MicrosoftSqlScriptHelper>.CallGetSortedTableColumns(scripter, table);
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

        var table = new MicrosoftSqlTable();
        table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

        table.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Source };

        var columns = ScripterAccessorHelper<MicrosoftSqlScriptHelper>.CallGetSortedTableColumns(scripter, table);
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

        var refTable = new MicrosoftSqlTable();
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "m", OrdinalPosition = 3 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "z", OrdinalPosition = 2 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 0 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 4 });

        refTable.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Source };

        var table = new MicrosoftSqlTable();
        table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

        table.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Target };
        table.MappedDbObject = refTable;

        var columns = ScripterAccessorHelper<MicrosoftSqlScriptHelper>.CallGetSortedTableColumns(scripter, table);
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

        var refTable = new MicrosoftSqlTable();
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "m", OrdinalPosition = 3 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "z", OrdinalPosition = 2 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 0 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 4 });

        refTable.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Source };

        var table = new MicrosoftSqlTable();
        table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

        table.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Target };
        table.MappedDbObject = refTable;

        var columns = ScripterAccessorHelper<MicrosoftSqlScriptHelper>.CallGetSortedTableColumns(scripter, table);
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

        var refTable = new MicrosoftSqlTable();
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "m", OrdinalPosition = 3 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "z", OrdinalPosition = 2 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 0 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 4 });

        refTable.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Source };

        var table = new MicrosoftSqlTable();
        table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

        table.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Target };
        table.MappedDbObject = refTable;

        var columns = ScripterAccessorHelper<MicrosoftSqlScriptHelper>.CallGetSortedTableColumns(scripter, table);
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

        var refTable = new MicrosoftSqlTable();
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "m", OrdinalPosition = 3 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "z", OrdinalPosition = 2 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 0 });
        refTable.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 4 });

        refTable.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Source };

        var table = new MicrosoftSqlTable();
        table.Columns.Add(new MicrosoftSqlColumn { Name = "e", OrdinalPosition = 1 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "a", OrdinalPosition = 2 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "b", OrdinalPosition = 0 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "c", OrdinalPosition = 4 });
        table.Columns.Add(new MicrosoftSqlColumn { Name = "d", OrdinalPosition = 3 });

        table.Database = new MicrosoftSqlDb() { Direction = CompareDirection.Target };
        table.MappedDbObject = refTable;

        var columns = ScripterAccessorHelper<MicrosoftSqlScriptHelper>.CallGetSortedTableColumns(scripter, table);
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

        var tables = new List<ABaseDbTable>
        {
            new PostgreSqlTable { Name = "e" },
            new PostgreSqlTable { Name = "a" },
            new PostgreSqlTable { Name = "b" },
            new PostgreSqlTable { Name = "d" },
            new PostgreSqlTable { Name = "c" },
        };

        var sortedTables = CallGetSortedTables(scripter, tables, false);
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

        var tables = new List<ABaseDbTable>
        {
            new PostgreSqlTable { Name = "e" },
            new PostgreSqlTable { Name = "a", InheritedTableName = "c" },
            new PostgreSqlTable { Name = "b", InheritedTableName = "a" },
            new PostgreSqlTable { Name = "d" },
            new PostgreSqlTable { Name = "c" },
        };

        var sortedTables = CallGetSortedTables(scripter, tables, false);
        sortedTables.Select(x => x.Name).Should().ContainInOrder("c", "a", "b", "d", "e");

        // More complicated test (based on https://qt-wiki-uploads.s3.amazonaws.com/images/4/4c/Beginner-Class-Hierarchy.jpg)
        tables =
        [
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
        ];
        tables = [.. tables.OrderBy(a => Guid.NewGuid())];

        sortedTables = CallGetSortedTables(scripter, tables, false);
        sortedTables.Select(x => x.Name).Should().ContainInOrder("Object", "Widget", "AbstractButton", "Frame", "AbstractScrollArea", "CheckBox", "GraphicsView", "Label", "ProgressBar", "PushButton", "RadioButton", "TextEdit", "Thread");
    }

    /// <summary>
    /// Calls the get sorted tables.
    /// </summary>
    /// <param name="scripter">The scripter.</param>
    /// <param name="tables">The tables</param>
    /// <param name="dropOrder">Whether to sort the tables for dropping them</param>
    /// <returns>The sorted tables</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetSortedTables")]
    private static extern IEnumerable<ABaseDbTable> CallGetSortedTables(PostgreSqlScripter scripter, List<ABaseDbTable> tables, bool dropOrder);

    /// <summary>
    /// The ScripterAccessorHelper class is a helper class that provides access to the private methods of the ADatabaseScripter class.
    /// </summary>
    /// <typeparam name="TScriptHelper">The type of the script helper.</typeparam>
    private static class ScripterAccessorHelper<TScriptHelper>
        where TScriptHelper : AScriptHelper
    {
        /// <summary>
        /// Calls the get sorted tables.
        /// </summary>
        /// <param name="scripter">The scripter.</param>
        /// <param name="table">The table with columns to script</param>
        /// <returns>The sorted columns</returns>
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetSortedTableColumns")]
        public static extern IEnumerable<ABaseDbColumn> CallGetSortedTableColumns(ADatabaseScripter<TScriptHelper> scripter, ABaseDbTable table);
    }
}
