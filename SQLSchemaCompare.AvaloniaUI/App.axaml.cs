using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using SQLSchemaCompare.AvaloniaUI.ViewModels;
using SQLSchemaCompare.AvaloniaUI.Views;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
using TiCodeX.SQLSchemaCompare.Services;

namespace SQLSchemaCompare.AvaloniaUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        Locator.CurrentMutable.Register<ILocalizationService>(() => new LocalizationService());

        var collection = new ServiceCollection();
        collection.AddTransient<SplashScreenViewModel>();
        collection.AddTransient<MainWindowViewModel>();
        var services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Create the splash screen
            var splashScreenVM = services.GetRequiredService<SplashScreenViewModel>();
            var splashScreen = new SplashScreen
            {
                DataContext = splashScreenVM,
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
            var mainWindowVM = services.GetRequiredService<MainWindowViewModel>();
            var mainWindow = new MainWindow
            {
                DataContext = mainWindowVM,
            };
            desktop.MainWindow = mainWindow;
            mainWindow.Show();

            // Get rid of the splash screen
            splashScreen.Close();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
