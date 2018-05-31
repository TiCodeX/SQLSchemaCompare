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
            if (this.Project != null)
            {
                return false;
            }

            this.Project = new CompareProject()
            {
                SourceProviderOptions = new MicrosoftSqlDatabaseProviderOptions()
                {
                    Hostname = @"localhost\SQLEXPRESS",
                    Password = "brokerpro05",
                    Username = "brokerpro",
                    UseWindowsAuthentication = true
                },
                TargetProviderOptions = new MicrosoftSqlDatabaseProviderOptions()
                {
                    Hostname = @"localhost\SQLEXPRESS",
                    Password = "brokerpro05",
                    Username = "brokerpro",
                    UseWindowsAuthentication = true
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
            this.Project = new CompareProject();
        }
    }
}
