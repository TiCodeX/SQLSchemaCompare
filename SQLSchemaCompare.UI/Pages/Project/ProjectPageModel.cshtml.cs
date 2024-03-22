namespace TiCodeX.SQLSchemaCompare.UI.Pages.Project
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Api;
    using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
    using TiCodeX.SQLSchemaCompare.Core.Enums;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
    using TiCodeX.SQLSchemaCompare.Services;
    using TiCodeX.SQLSchemaCompare.UI.Models;
    using TiCodeX.SQLSchemaCompare.UI.Models.Project;

    /// <summary>
    /// PageModel of the Project page
    /// </summary>
    public class ProjectPageModel : PageModel
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The app settings service
        /// </summary>
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// The project service
        /// </summary>
        private readonly IProjectService projectService;

        /// <summary>
        /// The database service
        /// </summary>
        private readonly IDatabaseService databaseService;

        /// <summary>
        /// The database compare service
        /// </summary>
        private readonly IDatabaseCompareService databaseCompareService;

        /// <summary>
        /// The app globals
        /// </summary>
        private readonly IAppGlobals appGlobals;

        /// <summary>
        /// The cipher service
        /// </summary>
        private readonly ICipherService cipherService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPageModel"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="projectService">The injected project service</param>
        /// <param name="databaseService">The injected database service</param>
        /// <param name="databaseCompareService">The injected database compare service</param>
        /// <param name="appGlobals">The injected app globals</param>
        /// <param name="cipherService">The injected cipher service</param>
        public ProjectPageModel(
            ILoggerFactory loggerFactory,
            IAppSettingsService appSettingsService,
            IProjectService projectService,
            IDatabaseService databaseService,
            IDatabaseCompareService databaseCompareService,
            IAppGlobals appGlobals,
            ICipherService cipherService)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger(nameof(ProjectPageModel));
            this.appSettingsService = appSettingsService;
            this.projectService = projectService;
            this.databaseService = databaseService;
            this.databaseCompareService = databaseCompareService;
            this.appGlobals = appGlobals;
            this.cipherService = cipherService;
        }

        /// <summary>
        /// Gets the current project
        /// </summary>
        public CompareProject Project { get; private set; }

        /// <summary>
        /// Get the Project page for the current Project
        /// </summary>
        public void OnGet()
        {
            if (this.projectService.Project == null)
            {
                this.projectService.NewProject(DatabaseType.MicrosoftSql);
            }

            this.Project = this.projectService.Project;
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <param name="req">The new project request</param>
        /// <returns>The response</returns>
        public IActionResult OnPostNewProject([FromBody] NewProjectRequest req)
        {
            try
            {
                if (req == null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                if (this.projectService.NeedSave() && !req.IgnoreDirty)
                {
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorProjectNeedToBeSaved });
                }

                this.projectService.NewProject(req.DatabaseType);
                return new JsonResult(new ApiResponse());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error creating new project");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorUnexpected, ErrorMessage = Localization.ErrorCannotCreateNewProject });
            }
        }

        /// <summary>
        /// Set the project to dirty
        /// </summary>
        /// <returns>The Api result</returns>
        public IActionResult OnPostSetProjectDirtyState()
        {
            this.projectService.SetDirtyState();
            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Get the Project page
        /// </summary>
        /// <param name="req">The load project request</param>
        /// <returns>The API response with result information</returns>
        public IActionResult OnPostLoadProject([FromBody] LoadProjectRequest req)
        {
            try
            {
                if (req == null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                if (this.projectService.NeedSave() && !req.IgnoreDirty)
                {
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorProjectNeedToBeSaved });
                }

                this.projectService.LoadProject(req.Filename);
            }
            catch (InvalidOperationException ex)
            {
                this.logger.LogError(ex, $"Error loading project: {req?.Filename}");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorCannotLoadProject, ErrorMessage = string.Format(CultureInfo.InvariantCulture, Localization.ErrorLoadProjectInvalidProjectFile, this.appGlobals.ProductName) });
            }
            catch (FileNotFoundException ex)
            {
                this.logger.LogError(ex, $"Error loading project: {req?.Filename}");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorCannotLoadProject, ErrorMessage = Localization.ErrorLoadProjectFileNotFound });
            }
            catch (UnauthorizedAccessException ex)
            {
                this.logger.LogError(ex, $"Error loading project: {req?.Filename}");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorCannotLoadProject, ErrorMessage = Localization.ErrorLoadProjectUnauthorizedFileAccess });
            }
            catch (IOException ex)
            {
                this.logger.LogError(ex, $"Error loading project: {req?.Filename}");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorCannotLoadProject, ErrorMessage = string.Format(CultureInfo.InvariantCulture, Localization.ErrorLoadProjectIOError, ex.Message) });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error loading project: {req?.Filename}");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorCannotLoadProject, ErrorMessage = string.Format(CultureInfo.InvariantCulture, Localization.ErrorLoadProject, ex.Message) });
            }

            var appSettings = this.appSettingsService.GetAppSettings();
            appSettings.RecentProjects.Remove(req.Filename);
            appSettings.RecentProjects.Add(req.Filename);

            try
            {
                this.appSettingsService.SaveAppSettings();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error saving app settings");
            }

            this.Project = this.projectService.Project;
            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Save the project
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <returns>The ApiResponse in JSON</returns>
        public ActionResult OnPostSaveProject([FromBody] string filename)
        {
            try
            {
                this.projectService.SaveProject(filename);
            }
            catch (Exception ex)
            {
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorCannotSaveProject, ErrorMessage = string.Format(CultureInfo.InvariantCulture, Localization.ErrorCannotSaveProject, ex.Message) });
            }

            var appSettings = this.appSettingsService.GetAppSettings();
            appSettings.RecentProjects.Remove(filename);
            appSettings.RecentProjects.Add(filename);
            this.appSettingsService.SaveAppSettings();

            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Close the project
        /// </summary>
        /// <param name="ignoreDirty">Close project even if current project is dirty</param>
        /// <returns>The ApiResponse in JSON</returns>
        public ActionResult OnPostCloseProject([FromBody] bool ignoreDirty)
        {
            try
            {
                if (this.projectService.NeedSave() && !ignoreDirty)
                {
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorProjectNeedToBeSaved });
                }

                this.projectService.CloseProject();
                return new JsonResult(new ApiResponse());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error closing project");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorUnexpected, ErrorMessage = Localization.ErrorCannotCloseProject });
            }
        }

        /// <summary>
        /// Edit the project with the new values from the UI
        /// </summary>
        /// <param name="options">The project options</param>
        /// <returns>The ApiResponse in JSON</returns>
        public ActionResult OnPostEditProject([FromBody] CompareProjectOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.projectService.Project.SourceProviderOptions = this.GetDatabaseProviderOptions(options, CompareDirection.Source);
            this.projectService.Project.TargetProviderOptions = this.GetDatabaseProviderOptions(options, CompareDirection.Target);
            this.projectService.Project.Options = options.ProjectOptions;

            // The UI  have the object type filter set only to the first clause of each group,
            // set the same type to the others of the same group
            if (this.projectService.Project.Options.Filtering.Clauses.Any())
            {
                var groupCounter = 0;
                foreach (var filterClauseGroup in this.projectService.Project.Options.Filtering.Clauses.GroupBy(x => x.Group))
                {
                    var clauses = filterClauseGroup.ToList();

                    // Recompute the group number
                    clauses.ForEach(x => x.Group = groupCounter);

                    // If there are more clauses, set the ObjectType of the first to the others
                    if (clauses.Count > 1)
                    {
                        clauses.ForEach(x => x.ObjectType = clauses[0].ObjectType);
                    }

                    groupCounter++;
                }

                // If there is only one clause and it's Value is not set, then discard it
                if (this.projectService.Project.Options.Filtering.Clauses.Count == 1 &&
                    string.IsNullOrWhiteSpace(this.projectService.Project.Options.Filtering.Clauses[0].Value))
                {
                    this.projectService.Project.Options.Filtering.Clauses.Clear();
                }
            }

            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Perform the database comparison
        /// </summary>
        /// <returns>The ApiResponse in JSON</returns>
        public ActionResult OnGetStartCompare()
        {
            this.databaseCompareService.StartCompare();

            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Connect to the server to retrieve the available databases
        /// </summary>
        /// <param name="options">The database provider options</param>
        /// <returns>The list of database names in JSON</returns>
        public ActionResult OnPostLoadDatabaseList([FromBody] CompareProjectOptions options)
        {
            try
            {
                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                return new JsonResult(new ApiResponse<List<string>>
                {
                    Result = this.databaseService.ListDatabases(this.GetDatabaseProviderOptions(options, options.Direction)),
                });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving databases");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorUnexpected, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Remove the filename from the recent projects
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <returns>The ApiResponse in JSON</returns>
        public ActionResult OnPostRemoveRecentProject([FromBody] string filename)
        {
            var settings = this.appSettingsService.GetAppSettings();
            settings.RecentProjects.Remove(filename);
            this.appSettingsService.SaveAppSettings();

            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Get the database provider options
        /// </summary>
        /// <param name="options">The compare project options</param>
        /// <param name="direction">The compare direction</param>
        /// <returns>The database provider options</returns>
        private ADatabaseProviderOptions GetDatabaseProviderOptions(CompareProjectOptions options, CompareDirection direction)
        {
            var type = options.DatabaseType;
            var hostname = direction == CompareDirection.Source ? options.SourceHostname : options.TargetHostname;
            var port = direction == CompareDirection.Source ? options.SourcePort : options.TargetPort;
            var username = direction == CompareDirection.Source ? options.SourceUsername : options.TargetUsername;
            var password = this.cipherService.EncryptString(direction == CompareDirection.Source ? options.SourcePassword : options.TargetPassword);
            var savePassword = direction == CompareDirection.Source ? options.SourceSavePassword : options.TargetSavePassword;
            var useWindowsAuthentication = direction == CompareDirection.Source ? options.SourceUseWindowsAuthentication : options.TargetUseWindowsAuthentication;
            var useAzureAuthentication = direction == CompareDirection.Source ? options.SourceUseAzureAuthentication : options.TargetUseAzureAuthentication;
            var useSsl = direction == CompareDirection.Source ? options.SourceUseSsl : options.TargetUseSsl;
            var ignoreServerCertificate = direction == CompareDirection.Source ? options.SourceIgnoreServerCertificate : options.TargetIgnoreServerCertificate;
            var database = direction == CompareDirection.Source ? options.SourceDatabase : options.TargetDatabase;

            switch (type)
            {
                case DatabaseType.MicrosoftSql:
                    return new MicrosoftSqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Port = port ?? MicrosoftSqlDatabaseProviderOptions.DefaultPort,
                        Username = username,
                        Password = password,
                        SavePassword = savePassword,
                        UseWindowsAuthentication = useWindowsAuthentication,
                        UseAzureAuthentication = useAzureAuthentication,
                        UseSsl = useSsl,
                        IgnoreServerCertificate = ignoreServerCertificate,
                        Database = database,
                    };
                case DatabaseType.MySql:
                    return new MySqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Port = port ?? MySqlDatabaseProviderOptions.DefaultPort,
                        Username = username,
                        Password = password,
                        SavePassword = savePassword,
                        UseSsl = useSsl,
                        IgnoreServerCertificate = ignoreServerCertificate,
                        Database = database,
                    };
                case DatabaseType.PostgreSql:
                    return new PostgreSqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Port = port ?? PostgreSqlDatabaseProviderOptions.DefaultPort,
                        Username = username,
                        Password = password,
                        SavePassword = savePassword,
                        UseSsl = useSsl,
                        IgnoreServerCertificate = ignoreServerCertificate,
                        Database = database,
                    };
                case DatabaseType.MariaDb:
                    return new MariaDbDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Port = port ?? MariaDbDatabaseProviderOptions.DefaultPort,
                        Username = username,
                        Password = password,
                        SavePassword = savePassword,
                        UseSsl = useSsl,
                        IgnoreServerCertificate = ignoreServerCertificate,
                        Database = database,
                    };
                default:
                    this.logger.LogError($"Unknown Database type: {type}");
                    throw new ArgumentException("Unknown Database type");
            }
        }
    }
}
