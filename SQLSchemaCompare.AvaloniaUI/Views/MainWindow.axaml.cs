using Avalonia.Controls;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

namespace SQLSchemaCompare.AvaloniaUI.Views;

public partial class MainWindow : Window
{
    private readonly ILocalizationService localizationService;

    public MainWindow(ILocalizationService localizationService)
    {
        this.localizationService = localizationService;
        InitializeComponent();
    }
}
