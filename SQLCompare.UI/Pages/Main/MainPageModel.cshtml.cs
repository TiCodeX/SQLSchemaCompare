using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Api;
using SQLCompare.Core.Entities.Compare;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.UI.Models.Main;

namespace SQLCompare.UI.Pages.Main
{
    /// <summary>
    /// PageModel of the Main page
    /// </summary>
    public class MainPageModel : PageModel
    {
        private readonly ILogger logger;
        private readonly IProjectService projectService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageModel"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="projectService">The injected project service</param>
        public MainPageModel(ILoggerFactory loggerFactory, IProjectService projectService)
        {
            this.logger = loggerFactory.CreateLogger(nameof(MainPageModel));
            this.projectService = projectService;
        }

        /// <summary>
        /// Gets the list of different items
        /// </summary>
        public List<ABaseCompareResultItem> DifferentItems { get; } = new List<ABaseCompareResultItem>();

        /// <summary>
        /// Gets the list of items only on the source
        /// </summary>
        public List<ABaseCompareResultItem> OnlySourceItems { get; } = new List<ABaseCompareResultItem>();

        /// <summary>
        /// Gets the list of items only on the target
        /// </summary>
        public List<ABaseCompareResultItem> OnlyTargetItems { get; } = new List<ABaseCompareResultItem>();

        /// <summary>
        /// Gets the list of items which are the same in both
        /// </summary>
        public List<ABaseCompareResultItem> SameItems { get; } = new List<ABaseCompareResultItem>();

        /// <summary>
        /// Get the Main page
        /// </summary>
        public void OnGet()
        {
            var result = this.projectService.Project.Result;

            this.DifferentItems.AddRange(result.Tables.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            this.DifferentItems.AddRange(result.Views.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            this.DifferentItems.AddRange(result.Functions.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            this.DifferentItems.AddRange(result.StoredProcedures.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            this.DifferentItems.AddRange(result.Sequences.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            this.DifferentItems.AddRange(result.DataTypes.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Different items => {this.DifferentItems.Count}");

            this.OnlySourceItems.AddRange(result.Tables.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            this.OnlySourceItems.AddRange(result.Views.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            this.OnlySourceItems.AddRange(result.Functions.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            this.OnlySourceItems.AddRange(result.StoredProcedures.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            this.OnlySourceItems.AddRange(result.Sequences.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            this.OnlySourceItems.AddRange(result.DataTypes.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Only Source items => {this.OnlySourceItems.Count}");

            this.OnlyTargetItems.AddRange(result.Tables.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.SourceItemName));
            this.OnlyTargetItems.AddRange(result.Views.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.SourceItemName));
            this.OnlyTargetItems.AddRange(result.Functions.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.SourceItemName));
            this.OnlyTargetItems.AddRange(result.StoredProcedures.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.SourceItemName));
            this.OnlyTargetItems.AddRange(result.Sequences.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.SourceItemName));
            this.OnlyTargetItems.AddRange(result.DataTypes.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Only Target items => {this.OnlyTargetItems.Count}");

            this.SameItems.AddRange(result.Tables.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            this.SameItems.AddRange(result.Views.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            this.SameItems.AddRange(result.Functions.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            this.SameItems.AddRange(result.StoredProcedures.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            this.SameItems.AddRange(result.Sequences.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            this.SameItems.AddRange(result.DataTypes.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Same items => {this.SameItems.Count}");
        }

        /// <summary>
        /// Get the create script of two items
        /// </summary>
        /// <param name="id">The requested compare result item</param>
        /// <returns>A JSON object with the source and target create scripts</returns>
        public ActionResult OnGetCreateScript(Guid id)
        {
            var resultItem = (ABaseCompareResultItem)this.projectService.Project.Result.Tables.FirstOrDefault(x => x.Id == id) ??
                             (ABaseCompareResultItem)this.projectService.Project.Result.Views.FirstOrDefault(x => x.Id == id) ??
                             (ABaseCompareResultItem)this.projectService.Project.Result.Functions.FirstOrDefault(x => x.Id == id) ??
                             (ABaseCompareResultItem)this.projectService.Project.Result.StoredProcedures.FirstOrDefault(x => x.Id == id) ??
                             (ABaseCompareResultItem)this.projectService.Project.Result.Sequences.FirstOrDefault(x => x.Id == id) ??
                             (ABaseCompareResultItem)this.projectService.Project.Result.DataTypes.FirstOrDefault(x => x.Id == id);

            if (resultItem == null)
            {
                this.logger.LogError("Unable to find the item specified");
                throw new NotImplementedException("Unable to find the item specified");
            }

            return new JsonResult(new ApiResponse<CreateScriptResult>
            {
                Result = new CreateScriptResult
                {
                    SourceSql = resultItem.SourceCreateScript,
                    TargetSql = resultItem.TargetCreateScript,
                }
            });
        }
    }
}