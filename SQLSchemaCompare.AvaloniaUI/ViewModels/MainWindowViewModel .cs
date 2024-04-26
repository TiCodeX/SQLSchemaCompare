using Avalonia.Media;

namespace SQLSchemaCompare.AvaloniaUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    public IBrush MenuBackground => new SolidColorBrush(Color.Parse("#292929"));
    public IBrush MenuForeground => Brushes.White;
}
