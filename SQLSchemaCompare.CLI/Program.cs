namespace TiCodeX.SQLSchemaCompare.CLI;

using System.CommandLine;
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
            .BuildServiceProvider(true);

        var rootCommand = new RootCommand("The SQL Schema Compare command-line tool.");

        rootCommand.AddCompareCommand(serviceProvider);
        rootCommand.AddInfoCommand(serviceProvider);

        // Default: when no subcommand is specified, behave as 'compare'
        rootCommand.SetDefaultCompareAction();

        int result = rootCommand.Parse(args).Invoke();
        Console.ReadKey();

        return result;
    }
}
