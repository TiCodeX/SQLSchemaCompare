namespace TiCodeX.SQLSchemaCompare.CLI;

using System.CommandLine;
using System.CommandLine.Help;
using Microsoft.Extensions.DependencyInjection;
using TiCodeX.SQLSchemaCompare.CLI.Commands;

/// <summary>
/// SQLSchemaCompare CLI application
/// </summary>
public static class Program
{
    /// <summary>
    /// Entry point of the SQLSchemaCompare UI application
    /// </summary>
    /// <param name="args">The command line arguments</param>
    /// <returns>The exit code</returns>
    public static int Main(string[] args)
    {
        using var serviceProvider = new ServiceCollection()
            .RegisterServices()
            .AddLogging()
            .BuildServiceProvider(true);

        var rootCommand = new RootCommand("The SQL Schema Compare command-line tool.")
        {
            TreatUnmatchedTokensAsErrors = false,
        };

        rootCommand.AddCompareCommand(serviceProvider);

        // Default: when no subcommand is specified, behave as 'compare'
        rootCommand.SetDefaultCompareAction();

        rootCommand.Options.OfType<HelpOption>().ToList().ForEach(option =>
        {
            option.Action = new CustomHelpAction((HelpAction)option.Action);
        });

        return rootCommand.Parse(args).Invoke();
    }
}
