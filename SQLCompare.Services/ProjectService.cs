using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Interfaces.Repository;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Services
{
    /// <summary>
    /// Implementation that provides the mechanisms to handle the comparison project.
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository projectRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectService"/> class.
        /// </summary>
        /// <param name="projectRepository">The injected project repository</param>
        public ProjectService(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository;
        }

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
                },
                Options = new ProjectOptions()
            };
            return true;
        }

        /// <inheritdoc/>
        public void SaveProject(string filename)
        {
            if (this.Project != null)
            {
                this.projectRepository.Write(this.Project, filename);
            }
        }

        /// <inheritdoc/>
        public void CloseProject()
        {
            this.Project = null;
        }

        /// <inheritdoc/>
        public void LoadProject(string filename)
        {
            this.Project = this.projectRepository.Read(filename);
        }
    }
}
