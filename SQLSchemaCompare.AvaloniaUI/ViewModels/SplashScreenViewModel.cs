using System;

namespace SQLSchemaCompare.AvaloniaUI.ViewModels;

public class SplashScreenViewModel : ViewModelBase
{
    public TimeSpan Duration { get; } = TimeSpan.FromSeconds(5);
}
