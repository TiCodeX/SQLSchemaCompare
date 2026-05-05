namespace TiCodeX.SQLSchemaCompare.CLI.Commands;

using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The compare command
/// </summary>
public static class CompareCommand
{
    /// <summary>
    /// Adds the compare command.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public static void AddCompareCommand(this RootCommand parent, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(parent);

        var compareCmd = new Command("compare", "Compare two databases.");
        parent.Subcommands.Add(compareCmd);

        var projectFileOption = new Option<string>("--project", "-p")
        {
            Description = "The project file",
            Arity = ArgumentArity.ExactlyOne,
        };

        var outputFileOption = new Option<string>("--output", "-o")
        {
            Description = "The output file",
            Arity = ArgumentArity.ExactlyOne,
        };

        compareCmd.Options.Add(projectFileOption);
        compareCmd.Options.Add(outputFileOption);

        compareCmd.SetAction(p =>
        {
            var options = new Options(
                p.GetValue(projectFileOption),
                p.GetValue(outputFileOption));

            return Execute(options, serviceProvider);
        });
    }

    /// <summary>
    /// Sets the default compare action.
    /// </summary>
    /// <param name="rootCommand">The root command.</param>
    public static void SetDefaultCompareAction(this RootCommand rootCommand)
    {
        rootCommand.SetAction(p =>
        {
            // With no args and no piped input, show help instead of invoking 'compare'
            if (p.UnmatchedTokens.Count == 0 && !Console.IsInputRedirected)
            {
                return rootCommand.Parse("--help").Invoke();
            }

            // Re-invoke with 'compare' prepended so the compare subcommand handles parsing
            var runArgs = new[] { "compare" }.Concat(p.UnmatchedTokens).ToArray();
            return rootCommand.Parse(runArgs).Invoke();
        });
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The exit code</returns>
    private static int Execute(Options options, IServiceProvider serviceProvider)
    {
        var projectService = serviceProvider.GetRequiredService<IProjectService>();
        projectService.LoadProject(options.ProjectFile);

        var taskService = serviceProvider.GetRequiredService<ITaskService>();

        var databaseCompareService = serviceProvider.GetRequiredService<IDatabaseCompareService>();
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
    private sealed record Options(
        string ProjectFile,
        string OutputFile);
}
