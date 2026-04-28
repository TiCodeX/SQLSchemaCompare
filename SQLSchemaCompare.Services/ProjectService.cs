namespace TiCodeX.SQLSchemaCompare.Services
{
    /// <summary>
    /// Implementation that provides the mechanisms to handle the comparison project.
    /// </summary>
    public class ProjectService : IProjectService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The project repository
        /// </summary>
        private readonly IProjectRepository projectRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectService"/> class.
        /// </summary>
        /// <param name="projectRepository">The injected project repository</param>
        /// <param name="loggerFactory">The injected logger factory</param>
        public ProjectService(IProjectRepository projectRepository, ILoggerFactory loggerFactory)
        {
            ArgumentNullException.ThrowIfNull(loggerFactory);

            this.logger = loggerFactory.CreateLogger(nameof(ProjectService));
            this.projectRepository = projectRepository;
        }

        /// <inheritdoc/>
        public CompareProject Project { get; private set; }

        /// <inheritdoc/>
        public bool NewProject(DatabaseType? databaseType)
        {
            this.Project = databaseType.HasValue ? databaseType.Value switch
            {
                DatabaseType.MicrosoftSql => new CompareProject
                {
                    SourceProviderOptions = new MicrosoftSqlDatabaseProviderOptions(),
                    TargetProviderOptions = new MicrosoftSqlDatabaseProviderOptions(),
                    Options = new ProjectOptions(),
                    State = ProjectState.New,
                },
                DatabaseType.MySql => new CompareProject
                {
                    SourceProviderOptions = new MySqlDatabaseProviderOptions(),
                    TargetProviderOptions = new MySqlDatabaseProviderOptions(),
                    Options = new ProjectOptions(),
                    State = ProjectState.New,
                },
                DatabaseType.PostgreSql => new CompareProject
                {
                    SourceProviderOptions = new PostgreSqlDatabaseProviderOptions(),
                    TargetProviderOptions = new PostgreSqlDatabaseProviderOptions(),
                    Options = new ProjectOptions(),
                    State = ProjectState.New,
                },
                DatabaseType.MariaDb => new CompareProject
                {
                    SourceProviderOptions = new MariaDbDatabaseProviderOptions(),
                    TargetProviderOptions = new MariaDbDatabaseProviderOptions(),
                    Options = new ProjectOptions(),
                    State = ProjectState.New,
                },
                _ => throw new NotSupportedException("Unknown Database Type"),
            } : new CompareProject
            {
                SourceProviderOptions = new MicrosoftSqlDatabaseProviderOptions(),
                TargetProviderOptions = new MicrosoftSqlDatabaseProviderOptions(),
                Options = new ProjectOptions(),
                State = ProjectState.New,
                EditableDatabaseType = true,
            };

            return true;
        }

        /// <inheritdoc/>
        public void SaveProject(string filename)
        {
            if (this.Project == null)
            {
                return;
            }

            this.projectRepository.Write(this.Project, filename);
            this.Project.State = ProjectState.Saved;
        }

        /// <inheritdoc/>
        public void CloseProject()
        {
            this.Project = null;
        }

        /// <inheritdoc/>
        public void LoadProject(string filename)
        {
            try
            {
                this.Project = this.projectRepository.Read(filename);
                this.Project.State = ProjectState.Saved;
                this.Project.EditableDatabaseType = false;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Load project error: {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public void SetDirtyState()
        {
            this.Project?.State = ProjectState.Dirty;
        }

        /// <inheritdoc/>
        public bool NeedSave()
        {
            return this.Project != null && this.Project.State == ProjectState.Dirty;
        }
    }
}
