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
            var compareProject = new CompareProject
            {
                SourceProviderOptions = new MicrosoftSqlDatabaseProviderOptions
                {
                    UseConnectionString = true,
                    ConnectionString = Guid.NewGuid().ToString(),
                    Hostname = Guid.NewGuid().ToString(),
                    Database = Guid.NewGuid().ToString(),
                    Username = Guid.NewGuid().ToString(),
                    Password = Guid.NewGuid().ToString(),
                    SavePassword = true,
                    UseWindowsAuthentication = true,
                    UseAzureAuthentication = true,
                    UseSsl = true,
                    IgnoreServerCertificate = true,
                },
                TargetProviderOptions = new PostgreSqlDatabaseProviderOptions
                {
                    Hostname = Guid.NewGuid().ToString(),
                    Database = Guid.NewGuid().ToString(),
                    Username = Guid.NewGuid().ToString(),
                    Password = Guid.NewGuid().ToString(),
                    UseSsl = true,
                    IgnoreServerCertificate = false,
                },
                Options = new ProjectOptions
                {
                    Scripting = new ScriptingOptions
                    {
                        IgnoreCollate = true,
                        OrderColumnAlphabetically = true,
                        IgnoreReferenceTableColumnOrder = true,
                        GenerateUpdateScriptForNewNotNullColumns = true,
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

                var xmlFileExpected = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<CompareProject xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <SourceProviderOptions xsi:type=""{nameof(MicrosoftSqlDatabaseProviderOptions)}"">
    <UseConnectionString>{XmlConvert.ToString(compareProject.SourceProviderOptions.UseConnectionString)}</UseConnectionString>
    <ConnectionString>{compareProject.SourceProviderOptions.ConnectionString}</ConnectionString>
    <Hostname>{compareProject.SourceProviderOptions.Hostname}</Hostname>
    <Port>{compareProject.SourceProviderOptions.Port}</Port>
    <Database>{compareProject.SourceProviderOptions.Database}</Database>
    <Username>{compareProject.SourceProviderOptions.Username}</Username>
    <Password>{compareProject.SourceProviderOptions.Password}</Password>
    <SavePassword>{XmlConvert.ToString(compareProject.SourceProviderOptions.SavePassword)}</SavePassword>
    <UseSSL>{XmlConvert.ToString(compareProject.SourceProviderOptions.UseSsl)}</UseSSL>
    <IgnoreServerCertificate>{XmlConvert.ToString(compareProject.SourceProviderOptions.IgnoreServerCertificate)}</IgnoreServerCertificate>
    <UseWindowsAuthentication>{XmlConvert.ToString(((MicrosoftSqlDatabaseProviderOptions)compareProject.SourceProviderOptions).UseWindowsAuthentication)}</UseWindowsAuthentication>
    <UseAzureAuthentication>{XmlConvert.ToString(((MicrosoftSqlDatabaseProviderOptions)compareProject.SourceProviderOptions).UseAzureAuthentication)}</UseAzureAuthentication>
  </SourceProviderOptions>
  <TargetProviderOptions xsi:type=""{nameof(PostgreSqlDatabaseProviderOptions)}"">
    <UseConnectionString>{XmlConvert.ToString(compareProject.TargetProviderOptions.UseConnectionString)}</UseConnectionString>
    <Hostname>{compareProject.TargetProviderOptions.Hostname}</Hostname>
    <Port>5432</Port>
    <Database>{compareProject.TargetProviderOptions.Database}</Database>
    <Username>{compareProject.TargetProviderOptions.Username}</Username>
    <SavePassword>{XmlConvert.ToString(compareProject.TargetProviderOptions.SavePassword)}</SavePassword>
    <UseSSL>{XmlConvert.ToString(compareProject.TargetProviderOptions.UseSsl)}</UseSSL>
    <IgnoreServerCertificate>{XmlConvert.ToString(compareProject.TargetProviderOptions.IgnoreServerCertificate)}</IgnoreServerCertificate>
  </TargetProviderOptions>
  <Options>
    <Scripting>
      <IgnoreCollate>{XmlConvert.ToString(compareProject.Options.Scripting.IgnoreCollate)}</IgnoreCollate>
      <OrderColumnAlphabetically>{XmlConvert.ToString(compareProject.Options.Scripting.OrderColumnAlphabetically)}</OrderColumnAlphabetically>
      <IgnoreReferenceTableColumnOrder>{XmlConvert.ToString(compareProject.Options.Scripting.IgnoreReferenceTableColumnOrder)}</IgnoreReferenceTableColumnOrder>
      <GenerateUpdateScriptForNewNotNullColumns>{XmlConvert.ToString(compareProject.Options.Scripting.GenerateUpdateScriptForNewNotNullColumns)}</GenerateUpdateScriptForNewNotNullColumns>
    </Scripting>
    <Filtering>
      <Include>{XmlConvert.ToString(compareProject.Options.Filtering.Include)}</Include>
      <Clauses>
        <FilterClause>
          <Group>{XmlConvert.ToString(compareProject.Options.Filtering.Clauses[0].Group)}</Group>
          <ObjectType xsi:nil=""true"" />
          <Field>{compareProject.Options.Filtering.Clauses[0].Field}</Field>
          <Operator>{compareProject.Options.Filtering.Clauses[0].Operator}</Operator>
          <Value>{compareProject.Options.Filtering.Clauses[0].Value}</Value>
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
