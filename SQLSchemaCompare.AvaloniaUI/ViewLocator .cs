using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SQLSchemaCompare.AvaloniaUI.ViewModels;

namespace SQLSchemaCompare.AvaloniaUI;

public class ViewLocator : IDataTemplate
{
    Control? ITemplate<object?, Control?>.Build(object? param)
    {
        if (param is null)
        {
            throw new ArgumentNullException(nameof(param));
        }

        var name = param.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        else
        {
            return new TextBlock { Text = "Not Found: " + name };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
