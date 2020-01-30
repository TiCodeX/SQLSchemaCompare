using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using FluentAssertions;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Infrastructure.Repository;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace TiCodeX.SQLSchemaCompare.Test.Infrastructure.Repository
{
    /// <summary>
    /// Test class for the ProjectRepository
    /// </summary>
    public class ProjectRepositoryTests : BaseTests<ProjectRepositoryTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRepositoryTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public ProjectRepositoryTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test the read functionality
        /// </summary>
        [Fact]
        [UnitTest]
        public void Read()
        {
            var projectRepository = new ProjectRepository(this.LoggerFactory);

            const string sourceHostname = "localhost";
            const string sourceUsername = "admin";
            const string sourcePassword = "test";
            const string sourceDatabase = "database1";
            const string targetHostname = "192.168.1.1";
            const string targetUsername = "pippo";
            const string targetPassword = "pluto";
            const string targetDatabase = "database2";

            var xmlFile = $@"<?xml version=""1.0""?>
<CompareProject xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <SourceProviderOptions xsi:type=""MicrosoftSqlDatabaseProviderOptions"">
    <Hostname>{sourceHostname}</Hostname>
    <Username>{sourceUsername}</Username>
    <Password>{sourcePassword}</Password>
    <Database>{sourceDatabase}</Database>
    <UseWindowsAuthentication>true</UseWindowsAuthentication>
  </SourceProviderOptions>
  <TargetProviderOptions xsi:type=""PostgreSqlDatabaseProviderOptions"">
    <Hostname>{targetHostname}</Hostname>
    <Username>{targetUsername}</Username>
    <Password>{targetPassword}</Password>
    <Database>{targetDatabase}</Database>
  </TargetProviderOptions>
</CompareProject>
";

            var filename = Path.GetTempFileName();
            try
            {
                File.WriteAllText(filename, xmlFile);

                var project = projectRepository.Read(filename);

                project.Should().NotBeNull();

                project.SourceProviderOptions.Should().BeOfType<MicrosoftSqlDatabaseProviderOptions>();
                project.SourceProviderOptions.Hostname.Should().Be(sourceHostname);
                project.SourceProviderOptions.Username.Should().Be(sourceUsername);
                project.SourceProviderOptions.Password.Should().Be(sourcePassword);
                project.SourceProviderOptions.Database.Should().Be(sourceDatabase);

                project.TargetProviderOptions.Should().BeOfType<PostgreSqlDatabaseProviderOptions>();
                project.TargetProviderOptions.Hostname.Should().Be(targetHostname);
                project.TargetProviderOptions.Username.Should().Be(targetUsername);
                project.TargetProviderOptions.Password.Should().Be(targetPassword);
                project.TargetProviderOptions.Database.Should().Be(targetDatabase);
            }
            finally
            {
                try
                {
                    File.Delete(filename);
                }
                catch
                {
                    // Do nothing
                }
            }
        }

        /// <summary>
        /// The the write functionality
        /// </summary>
        [Fact]
        [UnitTest]
        public void Write()
        {
            const string sourceHostname = "localhost";
            const string sourceUsername = "admin";
            const string sourcePassword = "test";
            const bool sourceSavePassword = true;
            const string sourceDatabase = "database1";
            const bool sourceUseWindowsAuthentication = true;
            const bool sourceUseSSL = false;
            const string targetHostname = "192.168.1.1";
            const string targetUsername = "pippo";
            const string targetPassword = "pluto";
            const string targetDatabase = "database2";
            const bool targetSavePassword = false;
            const bool targetUseSSL = true;

            var compareProject = new CompareProject
            {
                SourceProviderOptions = new MicrosoftSqlDatabaseProviderOptions
                {
                    Hostname = sourceHostname,
                    Database = sourceDatabase,
                    Username = sourceUsername,
                    Password = sourcePassword,
                    SavePassword = sourceSavePassword,
                    UseWindowsAuthentication = sourceUseWindowsAuthentication,
                    UseSSL = sourceUseSSL,
                },
                TargetProviderOptions = new PostgreSqlDatabaseProviderOptions
                {
                    Hostname = targetHostname,
                    Database = targetDatabase,
                    Username = targetUsername,
                    Password = targetPassword,
                    UseSSL = targetUseSSL,
                },
                Options = new ProjectOptions
                {
                    Scripting = new ScriptingOptions
                    {
                        IgnoreCollate = true,
                        OrderColumnAlphabetically = true,
                        IgnoreReferenceTableColumnOrder = true,
                    },
                    Filtering = new FilteringOptions
                    {
                        Include = false,
                    },
                },
            };
            compareProject.Options.Filtering.Clauses.Add(new FilterClause
            {
                Group = 0,
                Field = FilterField.Schema,
                Operator = FilterOperator.Equals,
                Value = "customer_data",
            });

            var filename = Path.GetTempFileName();
            try
            {
                new ProjectRepository(this.LoggerFactory).Write(compareProject, filename);

                var xmlFile = File.ReadAllText(filename);

                var xmlFileExpected = $@"<?xml version=""1.0""?>
<CompareProject xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <SourceProviderOptions xsi:type=""MicrosoftSqlDatabaseProviderOptions"">
    <Hostname>{sourceHostname}</Hostname>
    <Port>1433</Port>
    <Database>{sourceDatabase}</Database>
    <Username>{sourceUsername}</Username>
    <Password>{sourcePassword}</Password>
    <SavePassword>{XmlConvert.ToString(sourceSavePassword)}</SavePassword>
    <UseSSL>{XmlConvert.ToString(sourceUseSSL)}</UseSSL>
    <UseWindowsAuthentication>{XmlConvert.ToString(sourceUseWindowsAuthentication)}</UseWindowsAuthentication>
  </SourceProviderOptions>
  <TargetProviderOptions xsi:type=""PostgreSqlDatabaseProviderOptions"">
    <Hostname>{targetHostname}</Hostname>
    <Port>5432</Port>
    <Database>{targetDatabase}</Database>
    <Username>{targetUsername}</Username>
    <SavePassword>{XmlConvert.ToString(targetSavePassword)}</SavePassword>
    <UseSSL>{XmlConvert.ToString(targetUseSSL)}</UseSSL>
  </TargetProviderOptions>
  <Options>
    <Scripting>
      <IgnoreCollate>true</IgnoreCollate>
      <OrderColumnAlphabetically>true</OrderColumnAlphabetically>
      <IgnoreReferenceTableColumnOrder>true</IgnoreReferenceTableColumnOrder>
    </Scripting>
    <Filtering>
      <Include>false</Include>
      <Clauses>
        <FilterClause>
          <Group>0</Group>
          <ObjectType xsi:nil=""true"" />
          <Field>Schema</Field>
          <Operator>Equals</Operator>
          <Value>customer_data</Value>
        </FilterClause>
      </Clauses>
    </Filtering>
  </Options>
</CompareProject>";

                // Remove line-endings for comparison
                xmlFile = Regex.Replace(xmlFile, "\\r|\\n", string.Empty);
                xmlFileExpected = Regex.Replace(xmlFileExpected, "\\r|\\n", string.Empty);

                xmlFile.Should().Be(xmlFileExpected);
            }
            finally
            {
                try
                {
                    File.Delete(filename);
                }
                catch
                {
                    // Do nothing
                }
            }
        }
    }
}
