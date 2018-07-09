using System.IO;
using System.Xml;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Infrastructure.Repository;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Infrastructure.Repository
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
        public static void Read()
        {
            var projectRepository = new ProjectRepository();

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
            File.WriteAllText(filename, xmlFile);

            var project = projectRepository.Read(filename);

            Assert.NotNull(project);

            Assert.IsType<MicrosoftSqlDatabaseProviderOptions>(project.SourceProviderOptions);
            Assert.Equal(sourceHostname, project.SourceProviderOptions.Hostname);
            Assert.Equal(sourceUsername, project.SourceProviderOptions.Username);
            Assert.Equal(sourcePassword, project.SourceProviderOptions.Password);
            Assert.Equal(sourceDatabase, project.SourceProviderOptions.Database);

            Assert.IsType<PostgreSqlDatabaseProviderOptions>(project.TargetProviderOptions);
            Assert.Equal(targetHostname, project.TargetProviderOptions.Hostname);
            Assert.Equal(targetUsername, project.TargetProviderOptions.Username);
            Assert.Equal(targetPassword, project.TargetProviderOptions.Password);
            Assert.Equal(targetDatabase, project.TargetProviderOptions.Database);
        }

        /// <summary>
        /// The the write functionality
        /// </summary>
        [Fact]
        [UnitTest]
        public static void Write()
        {
            const string sourceHostname = "localhost";
            const string sourceUsername = "admin";
            const string sourcePassword = "test";
            const string sourceDatabase = "database1";
            const bool sourceUseWindowsAuthentication = true;
            const bool sourceUseSSL = false;
            const string targetHostname = "192.168.1.1";
            const string targetUsername = "pippo";
            const string targetPassword = "pluto";
            const string targetDatabase = "database2";
            const bool targetUseSSL = true;

            var compareProject = new CompareProject
            {
                SourceProviderOptions = new MicrosoftSqlDatabaseProviderOptions
                {
                    Hostname = sourceHostname,
                    Database = sourceDatabase,
                    Username = sourceUsername,
                    Password = sourcePassword,
                    UseWindowsAuthentication = sourceUseWindowsAuthentication,
                    UseSSL = sourceUseSSL
                },
                TargetProviderOptions = new PostgreSqlDatabaseProviderOptions
                {
                    Hostname = targetHostname,
                    Database = targetDatabase,
                    Username = targetUsername,
                    Password = targetPassword,
                    UseSSL = targetUseSSL
                },
                Options = new ProjectOptions
                {
                    Scripting = new ScriptingOptions
                    {
                        IgnoreCollate = true,
                        OrderColumnAlphabetically = true,
                        UseSchemaName = false,
                    }
                }
            };

            var filename = Path.GetTempFileName();
            new ProjectRepository().Write(compareProject, filename);

            var xmlFile = File.ReadAllText(filename);

            var xmlFileExpected = $@"<?xml version=""1.0""?>
<CompareProject xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <SourceProviderOptions xsi:type=""MicrosoftSqlDatabaseProviderOptions"">
    <Hostname>{sourceHostname}</Hostname>
    <Database>{sourceDatabase}</Database>
    <Username>{sourceUsername}</Username>
    <Password>{sourcePassword}</Password>
    <UseSSL>{XmlConvert.ToString(sourceUseSSL)}</UseSSL>
    <UseWindowsAuthentication>{XmlConvert.ToString(sourceUseWindowsAuthentication)}</UseWindowsAuthentication>
  </SourceProviderOptions>
  <TargetProviderOptions xsi:type=""PostgreSqlDatabaseProviderOptions"">
    <Hostname>{targetHostname}</Hostname>
    <Database>{targetDatabase}</Database>
    <Username>{targetUsername}</Username>
    <Password>{targetPassword}</Password>
    <UseSSL>{XmlConvert.ToString(targetUseSSL)}</UseSSL>
  </TargetProviderOptions>
  <Options>
    <Scripting>
      <IgnoreCollate>true</IgnoreCollate>
      <OrderColumnAlphabetically>true</OrderColumnAlphabetically>
      <UseSchemaName>false</UseSchemaName>
    </Scripting>
  </Options>
</CompareProject>";

            Assert.Equal(xmlFileExpected, xmlFile);
        }
    }
}
