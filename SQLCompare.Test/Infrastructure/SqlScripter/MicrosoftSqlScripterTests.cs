using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Infrastructure.SqlScripters;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Infrastructure.SqlScripter
{
    /// <summary>
    /// Test class for the MicrosoftSqlScripter
    /// </summary>
    public class MicrosoftSqlScripterTests : BaseTests<MicrosoftSqlScripterTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlScripterTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public MicrosoftSqlScripterTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test datasource
        /// </summary>
        /// <returns>The data</returns>
        public static IEnumerable<object[]> ScriptColumnTestData()
        {
            // ColumnName, ColumnDefault, Isnullable, DataType, CharacterMaxLenght, CharacterOctetLenght, NumericPrecision, NumericPrecisionRadix, NumericScale, DateTimePrecision, CharachterSetCatalog, CharacterSetSchema, CharacterSetName, CollationCatalog, CollationSchema, CollationName, DomainCatalog, DomainSchema, DomainName, IsIdentity, IdentitySeed, IdentityIncrement
            yield return new object[] { true, "TestColumn", null, false, "bigint", null, null, (byte?)19, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [bigint] NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "bigint", null, null, (byte?)19, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [bigint] NULL" };

            yield return new object[] { true, "TestColumn", null, true, "binary", 50, 50, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [binary](50) NULL" };

            yield return new object[] { true, "TestColumn", null, false, "bit", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [bit] NOT NULL" };
            yield return new object[] { true, "TestColumn", "((0))", false, "bit", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [bit] DEFAULT ((0)) NOT NULL" };
            yield return new object[] { true, "TestColumn", "('TRUE')", false, "bit", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [bit] DEFAULT ('TRUE') NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "bit", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [bit] NULL" };

            yield return new object[] { true, "TestColumn", null, false, "datetime", null, null, null, null, null, (short?)3, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [datetime] NOT NULL" };
            yield return new object[] { true, "TestColumn", "(getdate())", false, "datetime", null, null, null, null, null, (short?)3, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [datetime] DEFAULT (getdate()) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "datetime", null, null, null, null, null, (short?)3, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [datetime] NULL" };
            yield return new object[] { true, "TestColumn", "(getdate())", true, "datetime", null, null, null, null, null, (short?)3, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [datetime] DEFAULT (getdate()) NULL" };

            yield return new object[] { true, "TestColumn", null, false, "datetime2", null, null, null, null, null, (short?)7, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [datetime2](7) NOT NULL" };

            yield return new object[] { true, "TestColumn", null, false, "decimal", null, null, (byte?)18, (short?)10, 2, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [decimal](18, 2) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "decimal", null, null, (byte?)4, (short?)10, 1, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [decimal](4, 1) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "decimal", null, null, (byte?)5, (short?)10, 2, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [decimal](5, 2) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "decimal", null, null, (byte?)6, (short?)10, 3, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [decimal](6, 3) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "decimal", null, null, (byte?)16, (short?)10, 2, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [decimal](16, 2) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "decimal", null, null, (byte?)18, (short?)10, 2, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [decimal](18, 2) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "decimal", null, null, (byte?)18, (short?)10, 4, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [decimal](18, 4) NULL" };

            yield return new object[] { true, "TestColumn", null, false, "float", null, null, (byte?)53, (short?)2, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [float] NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "float", null, null, (byte?)53, (short?)2, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [float] NULL" };

            yield return new object[] { true, "TestColumn", null, false, "hierarchyid", 892, 892, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [hierarchyid] NOT NULL" };

            yield return new object[] { true, "TestColumn", null, true, "image", 2147483647, 2147483647, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [image] NULL" };

            yield return new object[] { true, "TestColumn", null, false, "int", null, null, (byte?)10, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [int] NOT NULL" };
            yield return new object[] { true, "TestColumn", null, false, "int", null, null, (byte?)10, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, true, 0, 1, "[TestColumn] [int] IDENTITY(0,1) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, false, "int", null, null, (byte?)10, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, true, 1, 1, "[TestColumn] [int] IDENTITY(1,1) NOT NULL" };
            yield return new object[] { true, "TestColumn", "((0))", false, "int", null, null, (byte?)10, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [int] DEFAULT ((0)) NOT NULL" };
            yield return new object[] { true, "TestColumn", "((1))", false, "int", null, null, (byte?)10, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [int] DEFAULT ((1)) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "int", null, null, (byte?)10, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [int] NULL" };
            yield return new object[] { true, "TestColumn", "((0))", true, "int", null, null, (byte?)10, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [int] DEFAULT ((0)) NULL" };
            yield return new object[] { true, "TestColumn", "((1))", true, "int", null, null, (byte?)10, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [int] DEFAULT ((1)) NULL" };

            yield return new object[] { true, "TestColumn", null, false, "nvarchar", -1, -1, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](max) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, false, "nvarchar", 3, 6, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](3) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, false, "nvarchar", 5, 10, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](5) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, false, "nvarchar", 300, 600, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](300) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "nvarchar", -1, -1, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](max) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "nvarchar", 2, 4, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](2) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "nvarchar", 5, 10, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](5) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "nvarchar", 10, 20, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](10) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "nvarchar", 500, 1000, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](500) NULL" };

            yield return new object[] { true, "TestColumn", null, true, "text", 2147483647, 2147483647, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [text] NULL" };

            yield return new object[] { true, "TestColumn", "((0))", false, "tinyint", null, null, (byte?)3, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [tinyint] DEFAULT ((0)) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "tinyint", null, null, (byte?)3, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [tinyint] NULL" };
            yield return new object[] { true, "TestColumn", "((0))", true, "tinyint", null, null, (byte?)3, (short?)10, 0, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [tinyint] DEFAULT ((0)) NULL" };

            yield return new object[] { true, "TestColumn", null, false, "uniqueidentifier", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [uniqueidentifier] NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "uniqueidentifier", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [uniqueidentifier] NULL" };

            yield return new object[] { true, "TestColumn", null, false, "varbinary", -1, -1, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [varbinary](max) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "varbinary", -1, -1, null, null, null, null, null, null, null, null, null, null, null, null, null, false, 0, 0, "[TestColumn] [varbinary](max) NULL" };

            yield return new object[] { true, "TestColumn", null, false, "varchar", -1, -1, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](max) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, false, "varchar", 10, 10, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](10) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, false, "varchar", 20, 20, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](20) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, false, "varchar", 1000, 1000, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](1000) NOT NULL" };
            yield return new object[] { true, "TestColumn", null, true, "varchar", -1, -1, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](max) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "varchar", 20, 20, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](20) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "varchar", 30, 30, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](30) NULL" };
            yield return new object[] { true, "TestColumn", null, true, "varchar", 4000, 4000, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](4000) NULL" };

            yield return new object[] { false, "TestColumn", null, false, "nvarchar", -1, -1, null, null, null, null, null, null, "UNICODE", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AI NOT NULL" };
            yield return new object[] { false, "TestColumn", null, false, "varchar", 1000, 1000, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AI NOT NULL" };
            yield return new object[] { false, "TestColumn", "('test')", false, "varchar", 20, 20, null, null, null, null, null, null, "iso_1", null, null, "SQL_Latin1_General_CP1_CI_AI", null, null, null, false, 0, 0, "[TestColumn] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AI DEFAULT ('test') NOT NULL" };
        }

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
#pragma warning disable SA1611 // Element parameters must be documented
        /// <summary>
        /// Test the script column functionality
        /// </summary>
        [Theory]
        [UnitTest]
        [MemberData(nameof(ScriptColumnTestData))]
        public void ScriptColumn(
            bool ignoreCollate,
            string columnName,
            string columnDefault,
            bool isNullable,
            string dataType,
            int? characterMaxLenght,
            int? characterOctetLenght,
            byte? numericPrecision,
            short? numericPrecisionRadix,
            int? numericScale,
            short? dateTimePrecision,
            string charachterSetCatalog,
            string characterSetSchema,
            string characterSetName,
            string collationCatalog,
            string collationSchema,
            string collationName,
            string domainCatalog,
            string domainSchema,
            string domainName,
            bool isIdentity,
            long identitySeed,
            long identityIncrement,
            string expecteResult)
        {
            MicrosoftSqlColumn col = new MicrosoftSqlColumn()
            {
                Name = columnName,
                ColumnDefault = columnDefault,
                IsNullable = isNullable,
                DataType = dataType,
                DateTimePrecision = dateTimePrecision,
                CharacterMaxLenght = characterMaxLenght,
                CharacterOctetLenght = characterOctetLenght,
                NumericPrecision = numericPrecision,
                NumericPrecisionRadix = numericPrecisionRadix,
                NumericScale = numericScale,
                CollationSchema = collationSchema,
                CharachterSetCatalog = charachterSetCatalog,
                CharacterSetSchema = characterSetSchema,
                CharacterSetName = characterSetName,
                CollationCatalog = collationCatalog,
                CollationName = collationName,
                DomainCatalog = domainCatalog,
                DomainSchema = domainSchema,
                DomainName = domainName,
                IsIdentity = isIdentity,
                IdentitySeed = identitySeed,
                IdentityIncrement = identityIncrement
            };

            ProjectOptions options = new ProjectOptions();
            options.Scripting.IgnoreCollate = ignoreCollate;

            MicrosoftSqlScripter scripter = new MicrosoftSqlScripter(this.Logger, options);
            Assert.Equal(scripter.ScriptColumn(col), expecteResult);
        }
#pragma warning restore SA1611 // Element parameters must be documented
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

    }
}
