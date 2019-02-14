using ExposedObject;
using FluentAssertions;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql;
using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseUtilities;
using TiCodeX.SQLSchemaCompare.Test;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLSchemaCompare.Test.Infrastructure.DatabaseUtilities
{
    /// <summary>
    /// Test class for the DatabaseFilter
    /// </summary>
    public class DatabaseFilterTests : BaseTests<DatabaseFilterTests>
    {
        private readonly dynamic exposedFilter;
        private readonly ABaseDbIndex testItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseFilterTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public DatabaseFilterTests(ITestOutputHelper output)
            : base(output)
        {
            this.exposedFilter = Exposed.From(typeof(DatabaseFilter));
            this.testItem = new ABaseDbIndex
            {
                Schema = "schema1",
                Name = "item1",
            };
        }

        /// <summary>
        /// Test the filtering of the database with a complex clause
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterDatabaseTest()
        {
            var db = new MicrosoftSqlDb();
            db.Schemas.Add(new ABaseDbSchema { Name = "schema1" });
            db.Schemas.Add(new ABaseDbSchema { Name = "schema2" });
            var table1 = new MicrosoftSqlTable { Schema = "schema1", Name = "table1" };
            table1.Columns.Add(new MicrosoftSqlColumn { Schema = "schema1", TableName = "table1", Name = "column1", });
            table1.Columns.Add(new MicrosoftSqlColumn { Schema = "schema1", TableName = "table1", Name = "column2", });
            table1.Columns.Add(new MicrosoftSqlColumn { Schema = "schema1", TableName = "table1", Name = "column3", });
            db.Tables.Add(table1);
            var table2 = new MicrosoftSqlTable { Schema = "schema1", Name = "table2" };
            table2.Columns.Add(new MicrosoftSqlColumn { Schema = "schema1", TableName = "table2", Name = "column1", });
            table2.Columns.Add(new MicrosoftSqlColumn { Schema = "schema1", TableName = "table2", Name = "column2", });
            table2.Columns.Add(new MicrosoftSqlColumn { Schema = "schema1", TableName = "table2", Name = "column3", });
            db.Tables.Add(table2);
            var table3 = new MicrosoftSqlTable { Schema = "schema1", Name = "table3" };
            table3.Columns.Add(new MicrosoftSqlColumn { Schema = "schema1", TableName = "table3", Name = "column1", });
            table3.Columns.Add(new MicrosoftSqlColumn { Schema = "schema1", TableName = "table3", Name = "column2", });
            table3.Columns.Add(new MicrosoftSqlColumn { Schema = "schema1", TableName = "table3", Name = "column3", });
            db.Tables.Add(table3);
            var table4 = new MicrosoftSqlTable { Schema = "schema2", Name = "table4" };
            table4.Columns.Add(new MicrosoftSqlColumn { Schema = "schema2", TableName = "table4", Name = "column1", });
            table4.Columns.Add(new MicrosoftSqlColumn { Schema = "schema2", TableName = "table4", Name = "column2", });
            table4.Columns.Add(new MicrosoftSqlColumn { Schema = "schema2", TableName = "table4", Name = "column3", });
            db.Tables.Add(table4);

            var filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Equals,
                Value = "schema2",
            });
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 1,
                Field = FilterField.Schema,
                Operator = FilterOperator.Equals,
                Value = "schema1",
            });
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 1,
                ObjectType = DatabaseObjectType.Table,
                Field = FilterField.Name,
                Operator = FilterOperator.Equals,
                Value = "table2",
            });

            var dbFilter = new DatabaseFilter();
            dbFilter.PerformFilter(db, filteringOptions);

            db.Tables.Count.Should().Be(2);
            db.Tables[0].Schema.Should().Be("schema1");
            db.Tables[0].Name.Should().Be("table1");
            db.Tables[1].Schema.Should().Be("schema1");
            db.Tables[1].Name.Should().Be("table3");
        }

        /// <summary>
        /// Test the filter to include item when the schema begins with
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterIncludeSchemaBeginsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.BeginsWith,
                Value = "sch",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.BeginsWith,
                Value = "blabla",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();
        }

        /// <summary>
        /// Test the filter to include item when the schema ends with
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterIncludeSchemaEndsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.EndsWith,
                Value = "ema1",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.EndsWith,
                Value = "ema2",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();
        }

        /// <summary>
        /// Test the filter to include item when the schema is equal
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterIncludeSchemaEqualsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Equals,
                Value = "schema1",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Equals,
                Value = "schema2",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();
        }

        /// <summary>
        /// Test the filter to include item when the schema contains
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterIncludeSchemaContainsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Contains,
                Value = "chem",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Contains,
                Value = "test",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();
        }

        /// <summary>
        /// Test the filter to include item when the schema doesn't begins with
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterIncludeSchemaNotBeginsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotBeginsWith,
                Value = "blabla",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotBeginsWith,
                Value = "sch",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();
        }

        /// <summary>
        /// Test the filter to include item when the schema doesn't ends with
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterIncludeSchemaNotEndsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotEndsWith,
                Value = "ema2",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotEndsWith,
                Value = "ema1",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();
        }

        /// <summary>
        /// Test the filter to include item when the schema is not equal
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterIncludeSchemaNotEqualsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotEquals,
                Value = "schema2",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotEquals,
                Value = "schema1",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();
        }

        /// <summary>
        /// Test the filter to include item when the schema doesn't contains
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterIncludeSchemaNotContainsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotContains,
                Value = "test",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = true
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotContains,
                Value = "chem",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();
        }

        /// <summary>
        /// Test the filter to exclude item when the schema begins with
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterExcludeSchemaBeginsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.BeginsWith,
                Value = "sch",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.BeginsWith,
                Value = "blabla",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();
        }

        /// <summary>
        /// Test the filter to exclude item when the schema ends with
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterExcludeSchemaEndsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.EndsWith,
                Value = "ema1",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.EndsWith,
                Value = "ema2",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();
        }

        /// <summary>
        /// Test the filter to exclude item when the schema is equal
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterExcludeSchemaEqualsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Equals,
                Value = "schema1",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Equals,
                Value = "schema2",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();
        }

        /// <summary>
        /// Test the filter to exclude item when the schema contains
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterExcludeSchemaContainsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Contains,
                Value = "chem",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Contains,
                Value = "test",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();
        }

        /// <summary>
        /// Test the filter to exclude item when the schema doesn't begins with
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterExcludeSchemaNotBeginsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotBeginsWith,
                Value = "blabla",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotBeginsWith,
                Value = "sch",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();
        }

        /// <summary>
        /// Test the filter to exclude item when the schema doesn't ends with
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterExcludeSchemaNotEndsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotEndsWith,
                Value = "ema2",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotEndsWith,
                Value = "ema1",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();
        }

        /// <summary>
        /// Test the filter to exclude item when the schema is not equal
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterExcludeSchemaNotEqualsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotEquals,
                Value = "schema2",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotEquals,
                Value = "schema1",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();
        }

        /// <summary>
        /// Test the filter to exclude item when the schema doesn't contains
        /// </summary>
        [Fact]
        [UnitTest]
        public void FilterExcludeSchemaNotContainsWithTest()
        {
            // Positive match
            var filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotContains,
                Value = "test",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeFalse();

            // Negative match
            filteringOptions = new FilteringOptions
            {
                Include = false
            };
            filteringOptions.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.NotContains,
                Value = "chem",
            });

            ((bool)this.exposedFilter.FilterIncludeObject(this.testItem, filteringOptions)).Should().BeTrue();
        }
    }
}
