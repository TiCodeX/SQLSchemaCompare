namespace TiCodeX.SQLSchemaCompare.CLI.Commands;

using System.Reflection;

/// <summary>
/// The compare command
/// </summary>
internal class CompareCommand(IProjectService projectService, ITaskService taskService, IDatabaseCompareService databaseCompareService)
    : Command<CompareCommand.Options>
{
    /// <inheritdoc/>
    protected override int Execute(CommandContext context, Options options, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.ProjectFile))
        {
            projectService.LoadProject(options.ProjectFile);
        }
        else
        {
            projectService.NewProject(options.DatabaseType);
            projectService.Project.SourceProviderOptions.UseConnectionString = true;
            projectService.Project.SourceProviderOptions.ConnectionString = options.SourceConnectionString;
            projectService.Project.TargetProviderOptions.UseConnectionString = true;
            projectService.Project.TargetProviderOptions.ConnectionString = options.TargetConnectionString;
        }

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
    internal sealed class Options : CommandSettings
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
        [CommandOption("--type <TYPE>")]
        [Description("The database type")]
        public DatabaseType DatabaseType { get; init; } = (DatabaseType)(-1);

        /// <summary>
        /// Gets the source connection string.
        /// </summary>
        [OptionGroup("Inline options")]
        [CommandOption("--source <CONNECTION_STRING>")]
        [Description("The source connection string")]
        public string SourceConnectionString { get; init; }

        /// <summary>
        /// Gets the target connection string.
        /// </summary>
        [OptionGroup("Inline options")]
        [CommandOption("--target <CONNECTION_STRING>")]
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
            var hasProject = !string.IsNullOrWhiteSpace(this.ProjectFile);
            var hasDatabaseType = this.DatabaseType != (DatabaseType)(-1);
            var hasSourceConnectionString = !string.IsNullOrWhiteSpace(this.SourceConnectionString);
            var hasTargetConnectionString = !string.IsNullOrWhiteSpace(this.TargetConnectionString);
            var hasInlineOptions = hasDatabaseType || hasSourceConnectionString || hasTargetConnectionString;
            var hasOutputFile = !string.IsNullOrWhiteSpace(this.OutputFile);

            if (!hasProject && !hasInlineOptions)
            {
                throw new ShowHelpException();
            }

            if (hasProject && hasInlineOptions)
            {
                return ValidationResult.Error("Specify either the project file options or the inline options, not both.");
            }

            var missingOptions = new List<string>();

            if (hasInlineOptions)
            {
                if (!hasDatabaseType)
                {
                    missingOptions.Add(GetLongName(nameof(this.DatabaseType)));
                }

                if (!hasSourceConnectionString)
                {
                    missingOptions.Add(GetLongName(nameof(this.SourceConnectionString)));
                }

                if (!hasTargetConnectionString)
                {
                    missingOptions.Add(GetLongName(nameof(this.TargetConnectionString)));
                }
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
