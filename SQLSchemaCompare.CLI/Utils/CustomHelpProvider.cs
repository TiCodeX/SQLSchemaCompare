namespace TiCodeX.SQLSchemaCompare.CLI.Utils;

using System.Reflection;

/// <summary>
/// The custom help provider
/// </summary>
internal class CustomHelpProvider(ICommandAppSettings settings) : HelpProvider(settings)
{
    /// <inheritdoc/>
    public override IEnumerable<IRenderable> GetHeader(ICommandModel model, ICommandInfo command)
    {
        var result = new List<IRenderable>();

        var assembly = typeof(CustomHelpProvider).Assembly;
        var product = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        var link = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        var version = assembly.GetName().Version;
        var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

        var content = new Markup(
            $"[bold dodgerblue1 link={link}]{product}[/]\n" +
            $"[cyan]v{version.ToString(3)}[/]\n" +
            $"[grey]Copyright (c) {DateTime.Now.Year} {company}[/]");

        result.Add(new Panel(content)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.DodgerBlue1),
            Expand = false,
            Padding = new Padding(2, 0, 2, 0),
        });
        result.Add(new Text(Environment.NewLine));

        return result;
    }

    /// <inheritdoc/>
    public override IEnumerable<IRenderable> GetOptions(ICommandModel model, ICommandInfo command)
    {
        if (command == null)
        {
            return base.GetOptions(model, command);
        }

        var result = new List<IRenderable>();

        var settingsType = command.GetType().GetProperty("SettingsType")?.GetValue(command) as Type;

        var groups = command.Parameters
            .Select(x => new
            {
                PropertyInfo = settingsType.GetProperty((x as ICommandParameterInfo).PropertyName),
                CommandOption = x as ICommandOption,
            })
            .GroupBy(x => x.PropertyInfo.GetCustomAttribute<OptionGroupAttribute>()?.Name ?? "Options");

        foreach (var group in groups)
        {
            result.Add(new Text(Environment.NewLine));
            result.Add(new Text($"{group.Key.ToUpperInvariant()}:", new Style(Color.Yellow, decoration: Decoration.Bold)));
            result.Add(new Text(Environment.NewLine));

            var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadLeft(4))
                .AddColumn(new GridColumn().NoWrap().PadLeft(4));

            foreach (var param in group)
            {
                var shortNames = param.CommandOption.ShortNames.Select(s => $"-{s}");
                var longNames = param.CommandOption.LongNames.Select(l => $"--{l}");
                var aliases = string.Join(", ", shortNames.Concat(longNames));

                var propertyType = Nullable.GetUnderlyingType(param.PropertyInfo.PropertyType) ?? param.PropertyInfo.PropertyType;
                if (!string.IsNullOrWhiteSpace(param.CommandOption.ValueName))
                {
                    aliases += $" <{param.CommandOption.ValueName}>";
                }
                else if (propertyType.IsEnum)
                {
                    aliases += $" <{string.Join("|", Enum.GetNames(propertyType))}>";
                }
                else
                {
                    // Do nothing
                }

                grid.AddRow($"[Gray62]{aliases}[/]", param.CommandOption.Description ?? string.Empty);
            }

            result.Add(grid);
        }

        return result;
    }
}
