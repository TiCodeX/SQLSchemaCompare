namespace TiCodeX.SQLSchemaCompare.CLI;

using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Reflection;
using Spectre.Console;

/// <summary>
/// The custom help action
/// </summary>
internal class CustomHelpAction(HelpAction action) : SynchronousCommandLineAction
{
    /// <inheritdoc/>
    public override int Invoke(ParseResult parseResult)
    {
        var assembly = typeof(CustomHelpAction).Assembly;
        var product = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        var link = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        var version = assembly.GetName().Version;
        var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

        var content = new Markup(
            $"[bold dodgerblue1 link={link}]{product}[/]\n" +
            $"[cyan]v{version.ToString(3)}[/]\n" +
            $"[grey]Copyright (c) {DateTime.Now.Year} {company}[/]");

        AnsiConsole.Write(new Panel(content)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.DodgerBlue1),
            Expand = false,
            Padding = new Padding(2, 0, 2, 0),
        });
        AnsiConsole.WriteLine();

        return action.Invoke(parseResult);
    }
}
