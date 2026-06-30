namespace TiCodeX.SQLSchemaCompare.CLI.Commands;

using System.Reflection;

/// <summary>
/// The monitor command
/// </summary>
internal class MonitorCommand(IProjectService projectService, ITaskService taskService, IDatabaseCompareService databaseCompareService, ILogger<MonitorCommand> logger)
    : Command<MonitorCommand.Options>
{
    /// <inheritdoc/>
    protected override int Execute(CommandContext context, Options options, CancellationToken cancellationToken)
    {
        projectService.NewProject(options.DatabaseType);
        projectService.Project.SourceProviderOptions.UseConnectionString = true;
        projectService.Project.SourceProviderOptions.ConnectionString = options.ConnectionString;
        projectService.Project.TargetProviderOptions.UseConnectionString = true;
        projectService.Project.TargetProviderOptions.ConnectionString = options.ConnectionString;

        databaseCompareService.StartCompare(true);

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
        logger.LogInformation("Compare completed successfully. Output written to {OutputFile}", options.OutputFile);

        return 0;
    }

    /// <summary>
    /// The options for the compare command
    /// </summary>
    internal sealed class Options : CommandSettings
    {
        /// <summary>
        /// Gets the type of the database.
        /// </summary>
        [OptionGroup("Inline options")]
        [CommandOption("--type <TYPE>")]
        [Description("The database type")]
        public DatabaseType DatabaseType { get; init; } = (DatabaseType)(-1);

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        [OptionGroup("Inline options")]
        [CommandOption("--connection <CONNECTION_STRING>")]
        [Description("The connection string")]
        public string ConnectionString { get; init; }

        /// <summary>
        /// Gets the output file.
        /// </summary>
        [OptionGroup("Common options")]
        [CommandOption("-o|--output <FILE_PATH>")]
        [Description("The output file")]
        public string OutputFile { get; init; }

        /// <inheritdoc/>
        public override ValidationResult Validate()
        {
            var hasDatabaseType = this.DatabaseType != (DatabaseType)(-1);
            var hasConnectionString = !string.IsNullOrWhiteSpace(this.ConnectionString);
            var hasOutputFile = !string.IsNullOrWhiteSpace(this.OutputFile);

            if (!hasDatabaseType && !hasConnectionString && !hasOutputFile)
            {
                throw new ShowHelpException();
            }

            var missingOptions = new List<string>();

            if (!hasDatabaseType)
            {
                missingOptions.Add(GetLongName(nameof(this.DatabaseType)));
            }

            if (!hasConnectionString)
            {
                missingOptions.Add(GetLongName(nameof(this.ConnectionString)));
            }

            if (!hasOutputFile)
            {
                missingOptions.Add(GetLongName(nameof(this.OutputFile)));
            }

            if (missingOptions.Count > 0)
            {
                return ValidationResult.Error($"Missing required options: {string.Join(", ", missingOptions)}");
            }

            return ValidationResult.Success();

            static string GetLongName(string propertyName)
            {
                var propertyInfo = typeof(Options).GetProperty(propertyName);
                var commandOption = propertyInfo.GetCustomAttribute<CommandOptionAttribute>();
                return $"--{commandOption.LongNames[0]}";
            }
        }
    }
}
