using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Services
{
    /// <summary>
    /// Implementation that provides the mechanisms to handle the comparison project.
    /// </summary>
    public class ProjectService : IProjectService
    {
        /// <inheritdoc/>
        public CompareProject Project { get; private set; }

        /// <inheritdoc/>
        public bool NewProject()
        {
            // TODO: return false if project is still open
            this.Project = new CompareProject()
            {
                SourceProviderOptions = new MicrosoftSqlDatabaseProviderOptions
                {
                    Hostname = @"localhost\SQLEXPRESS",
                    Username = "brokerpro",
                    Password = "brokerpro05",
                    Database = "brokerpro",
                    UseWindowsAuthentication = true,
                },
                TargetProviderOptions = new PostgreSqlDatabaseProviderOptions
                {
                    Hostname = "localhost",
                    Username = "postgres",
                    Password = "test1234",
                    Database = "world",
                }
            };
            return true;
        }

        /// <inheritdoc/>
        public void SaveProject()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void CloseProject()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void LoadProject(string filename)
        {
        }
    }
}
