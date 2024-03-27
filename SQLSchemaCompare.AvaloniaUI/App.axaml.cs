using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using SQLSchemaCompare.AvaloniaUI.ViewModels;
using SQLSchemaCompare.AvaloniaUI.Views;

namespace SQLSchemaCompare.AvaloniaUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Create the splash screen
            var splashScreenVM = new SplashScreenViewModel();
            var splashScreen = new SplashScreen
            {
                DataContext = splashScreenVM
            };

            // Set as the (temporary) main window.
            // By default, the application lifetime will shut down the application when the main
            // window is closed (unless ShutdownMode is set to something else). Later on we will
            // swap out MainWindow for the "true" MainWindow before closing the splash screen.
            // I see that this type of MainWindow swapping is used when switching themes in
            // Avalonias ControlCatalog source, so presumably it's OK to do this.
            desktop.MainWindow = splashScreen;

            splashScreen.Show();

            await Task.Delay(splashScreenVM.Duration);

            // Create the main window, and swap it in for the real main window
            var mainWin = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            desktop.MainWindow = mainWin;
            mainWin.Show();

            // Get rid of the splash screen
            splashScreen.Close();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
