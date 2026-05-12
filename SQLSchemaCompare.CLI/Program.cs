namespace TiCodeX.SQLSchemaCompare.CLI;

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
        var services = new ServiceCollection()
            .RegisterServices()
            .AddLogging();

        using var registrar = new DependencyInjectionRegistrar(services);

        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.UseStrictParsing();
            config.SetHelpProvider(new CustomHelpProvider(config.Settings));

            config.AddCommand<CompareCommand>("compare").WithDescription("Compare two databases.");
        });

        // When options are provided without a command name, default to 'compare'
        if (args?.Length > 0 &&
            args[0].StartsWith('-') &&
            args[0] != "-?" &&
            args[0] != "-h" &&
            args[0] != "--help")
        {
            args = ["compare", .. args];
        }

        try
        {
            return app.Run(args);
        }
        catch (ShowHelpException)
        {
            return app.Run([.. args, "--help"]);
        }
        catch (CommandAppException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return 1;
        }
    }
}
