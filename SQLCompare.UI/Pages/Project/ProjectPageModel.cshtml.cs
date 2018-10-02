using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Api;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.Services;
using SQLCompare.UI.Enums;
using SQLCompare.UI.Models;
using SQLCompare.UI.Models.Project;

namespace SQLCompare.UI.Pages.Project
{
    /// <summary>
    /// PageModel of the Project page
    /// </summary>
    public class ProjectPageModel : PageModel
    {
        private readonly ILogger logger;
        private readonly IAppSettingsService appSettingsService;
        private readonly IProjectService projectService;
        private readonly IDatabaseService databaseService;
        private readonly IDatabaseCompareService databaseCompareService;
        private readonly IAppGlobals appGlobals;
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
        /// <returns>TODO: boh</returns>
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
        /// <returns>TODO: boh</returns>
        public ActionResult OnPostCloseProject([FromBody]bool ignoreDirty)
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
        /// <returns>TODO: boh</returns>
        public ActionResult OnPostEditProject([FromBody] CompareProjectOptions options)
        {
            this.projectService.Project.SourceProviderOptions = this.GetDatabaseProviderOptions(options, CompareDirection.Source);
            this.projectService.Project.TargetProviderOptions = this.GetDatabaseProviderOptions(options, CompareDirection.Target);
            this.projectService.Project.Options = options.ProjectOptions;

            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Perform the database comparison
        /// </summary>
        /// <returns>TODO: boh</returns>
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
                return new JsonResult(new ApiResponse<List<string>>
                {
                    Result = this.databaseService.ListDatabases(this.GetDatabaseProviderOptions(options, options.Direction))
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
        /// <returns>TODO: boh</returns>
        public ActionResult OnPostRemoveRecentProject([FromBody] string filename)
        {
            var settings = this.appSettingsService.GetAppSettings();
            settings.RecentProjects.Remove(filename);
            this.appSettingsService.SaveAppSettings();

            return new JsonResult(new ApiResponse());
        }

        // TODO: move somewhere else and add missing parameters
        private ADatabaseProviderOptions GetDatabaseProviderOptions(CompareProjectOptions options, CompareDirection direction)
        {
            var type = options.DatabaseType;
            var hostname = direction == CompareDirection.Source ? options.SourceHostname : options.TargetHostname;
            var username = direction == CompareDirection.Source ? options.SourceUsername : options.TargetUsername;
            var password = this.cipherService.EncryptString(direction == CompareDirection.Source ? options.SourcePassword : options.TargetPassword);
            var savePassword = direction == CompareDirection.Source ? options.SourceSavePassword : options.TargetSavePassword;
            var useWindowsAuthentication = direction == CompareDirection.Source ? options.SourceUseWindowsAuthentication : options.TargetUseWindowsAuthentication;
            var useSSL = direction == CompareDirection.Source ? options.SourceUseSSL : options.TargetUseSSL;
            var database = direction == CompareDirection.Source ? options.SourceDatabase : options.TargetDatabase;

            switch (type)
            {
                case DatabaseType.MicrosoftSql:
                    return new MicrosoftSqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                        SavePassword = savePassword,
                        UseWindowsAuthentication = useWindowsAuthentication,
                        UseSSL = useSSL,
                        Database = database,
                    };
                case DatabaseType.MySql:
                    return new MySqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                        SavePassword = savePassword,
                        UseSSL = useSSL,
                        Database = database,
                    };
                case DatabaseType.PostgreSql:
                    return new PostgreSqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                        SavePassword = savePassword,
                        Database = database,
                    };
                default:
                    this.logger.LogError($"Unknown Database type: {type}");
                    throw new ArgumentException("Unknown Database type");
            }
        }
    }
}