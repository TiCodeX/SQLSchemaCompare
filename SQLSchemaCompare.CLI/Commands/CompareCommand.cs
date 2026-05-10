namespace TiCodeX.SQLSchemaCompare.CLI.Commands;

/// <summary>
/// The compare command
/// </summary>
internal class CompareCommand(IProjectService projectService, ITaskService taskService, IDatabaseCompareService databaseCompareService)
    : Command<CompareCommand.Options>
{
    /// <inheritdoc/>
    protected override int Execute(CommandContext context, Options options, CancellationToken cancellationToken)
    {
        projectService.LoadProject(options.ProjectFile);

        databaseCompareService.StartCompare();

        while (!taskService.CurrentTaskInfos.All(x => x.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted or TaskStatus.Canceled))
        {
            Thread.Sleep(200);
        }

        if (taskService.CurrentTaskInfos.Any(x => x.Status is TaskStatus.Faulted or TaskStatus.Canceled))
        {
            var exception = taskService.CurrentTaskInfos.FirstOrDefault(x => x.Status is TaskStatus.Faulted or TaskStatus.Canceled)?.Exception;
            if (exception != null)
            {
                throw exception;
            }

            throw new InvalidOperationException("Unknown error during compare task");
        }

        File.WriteAllText(options.OutputFile, projectService.Project.Result.FullAlterScript);

        return 0;
    }

    /// <summary>
    /// The options for the compare command
    /// </summary>
    public class Options : CommandSettings
    {
        /// <summary>
        /// Gets the project file.
        /// </summary>
        [OptionGroup("Project file options")]
        [CommandOption("-p|--project <FILEPATH>")]
        [Description("The project file (.tcxsc)")]
        public string ProjectFile { get; init; }

        /// <summary>
        /// Gets the type of the database.
        /// </summary>
        [OptionGroup("Inline options")]
        [CommandOption("--type")]
        [Description("The database type")]
        public DatabaseType? DatabaseType { get; init; }

        /// <summary>
        /// Gets the source connection string.
        /// </summary>
        [OptionGroup("Inline options")]
        [CommandOption("--source <CONNECTION STRING>")]
        [Description("The source connection string")]
        public string SourceConnectionString { get; init; }

        /// <summary>
        /// Gets the target connection string.
        /// </summary>
        [OptionGroup("Inline options")]
        [CommandOption("--target <CONNECTION STRING>")]
        [Description("The target connection string")]
        public string TargetConnectionString { get; init; }

        /// <summary>
        /// Gets the output file.
        /// </summary>
        [OptionGroup("Common options")]
        [CommandOption("-o|--output <FILEPATH>")]
        [Description("The output file")]
        public string OutputFile { get; init; }

        /// <inheritdoc/>
        public override ValidationResult Validate()
        {
            bool hasProject = !string.IsNullOrWhiteSpace(this.ProjectFile);
            bool hasInline = this.DatabaseType != null ||
                             !string.IsNullOrWhiteSpace(this.SourceConnectionString) ||
                             !string.IsNullOrWhiteSpace(this.TargetConnectionString);

            if (!hasProject && !hasInline)
            {
                throw new ShowHelpException();
            }

            if (hasProject && hasInline)
            {
                return ValidationResult.Error("Specify either the project file options or the inline options, not both.");
            }

            return ValidationResult.Success();
        }
    }
}
