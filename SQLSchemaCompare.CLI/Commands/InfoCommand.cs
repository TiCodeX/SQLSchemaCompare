namespace TiCodeX.SQLSchemaCompare.CLI.Commands;

using System.CommandLine;
using System.Runtime.InteropServices;
using Spectre.Console;

/// <summary>
/// The info command
/// </summary>
public static class InfoCommand
{
    /// <summary>
    /// Adds the information command.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public static void AddInfoCommand(this RootCommand parent, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(parent);

        var infoCmd = new Command("info", "Display information about SQL Schema Compare and the current environment.");
        parent.Subcommands.Add(infoCmd);
        infoCmd.SetAction(_ => Execute(serviceProvider));
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The exit code</returns>
    private static int Execute(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        AnsiConsole.MarkupLine("[bold][violet]General[/][/]");
        Print("Version", /*AppIdentifier.PRODUCT_VERSION*/"TODO");
        Print("Runtime version", RuntimeInformation.FrameworkDescription);
        Print("Runtime identifier", RuntimeInformation.RuntimeIdentifier);
        Print("OS", $"{RuntimeInformation.OSDescription} ({Environment.OSVersion.VersionString})");
        Print("Processor", RuntimeInformation.ProcessArchitecture.ToString());

        // ENV
        AnsiConsole.MarkupLine("\n[bold][violet]Environment[/][/]");
        string[] variables =
        [
            "NETPAD_CACHE_DIR",

            "DOTNET_ROOT",
            "DOTNET_ROOT_X64",
            "DOTNET_ROOT_ARM64",
            "DOTNET_HOST_PATH",
            "DOTNET_ENVIRONMENT",
            "ASPNETCORE_ENVIRONMENT",
            "DOTNET_ROLL_FORWARD",
            "NUGET_PACKAGES",
        ];

        foreach (var variable in variables)
        {
            Print(variable, Helper.ShortenHomePath(Environment.GetEnvironmentVariable(variable)));
        }

        return 0;
    }

    /// <summary>
    /// Prints the specified header.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="text">The text.</param>
    private static void Print(string header, string text)
    {
        AnsiConsole.MarkupLineInterpolated($"  [b]{header,-27}[/] {text}");
    }
}
