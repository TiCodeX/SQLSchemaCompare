using System;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;
        private readonly IProjectRepository projectRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectService"/> class.
        /// </summary>
        /// <param name="projectRepository">The injected project repository</param>
        /// <param name="loggerFactory">The injected logger factory</param>
        public ProjectService(IProjectRepository projectRepository, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(nameof(ProjectService));
            this.projectRepository = projectRepository;
        }

        /// <inheritdoc/>
        public CompareProject Project { get; private set; }

        /// <inheritdoc/>
        public bool NewProject()
        {
            // TODO: return false if project is still open
            this.Project = new CompareProject
            {
                SourceProviderOptions = new MicrosoftSqlDatabaseProviderOptions(),
                TargetProviderOptions = new MicrosoftSqlDatabaseProviderOptions(),
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
            try
            {
                this.Project = this.projectRepository.Read(filename);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Load project error: {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public bool NeedSave()
        {
            bool isDirty = false;

            // Todo: implement a way to detect if is dirty
            if (this.Project != null && isDirty)
            {
                return true;
            }

            return false;
        }
    }
}
