namespace TiCodeX.SQLSchemaCompare.UI.Pages.Main
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Api;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Compare;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

    /// <summary>
    /// PageModel of the Main page
    /// </summary>
    public class MainPageModel : PageModel
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The project service
        /// </summary>
        private readonly IProjectService projectService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageModel"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="projectService">The injected project service</param>
        public MainPageModel(ILoggerFactory loggerFactory, IProjectService projectService)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger(nameof(MainPageModel));
            this.projectService = projectService;
        }

        /// <summary>
        /// Gets or sets the list of different items
        /// </summary>
        public IReadOnlyCollection<ABaseCompareResultItem> DifferentItems { get; set; }

        /// <summary>
        /// Gets or sets the list of items only on the source
        /// </summary>
        public IReadOnlyCollection<ABaseCompareResultItem> OnlySourceItems { get; set; }

        /// <summary>
        /// Gets or sets the list of items only on the target
        /// </summary>
        public IReadOnlyCollection<ABaseCompareResultItem> OnlyTargetItems { get; set; }

        /// <summary>
        /// Gets or sets the list of items which are the same in both
        /// </summary>
        public IReadOnlyCollection<ABaseCompareResultItem> SameItems { get; set; }

        /// <summary>
        /// Get the Main page
        /// </summary>
        public void OnGet()
        {
            this.DifferentItems = this.projectService.Project.Result.DifferentItems;
            this.OnlySourceItems = this.projectService.Project.Result.OnlySourceItems;
            this.OnlyTargetItems = this.projectService.Project.Result.OnlyTargetItems;
            this.SameItems = this.projectService.Project.Result.SameItems;
        }

        /// <summary>
        /// Get the result item scripts
        /// </summary>
        /// <param name="id">The requested compare result item</param>
        /// <returns>A JSON object with the scripts</returns>
        public ActionResult OnGetResultItemScripts(Guid id)
        {
            var resultItem = this.projectService.Project.Result.DifferentItems.FirstOrDefault(x => x.Id == id) ??
                             this.projectService.Project.Result.OnlySourceItems.FirstOrDefault(x => x.Id == id) ??
                             this.projectService.Project.Result.OnlyTargetItems.FirstOrDefault(x => x.Id == id) ??
                             this.projectService.Project.Result.SameItems.FirstOrDefault(x => x.Id == id);

            if (resultItem == null)
            {
                this.logger.LogError("Unable to find the item specified");
                throw new NotImplementedException("Unable to find the item specified");
            }

            return new JsonResult(new ApiResponse<CompareResultItemScripts>
            {
                Result = resultItem.Scripts,
            });
        }
    }
}
