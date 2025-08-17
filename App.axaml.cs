using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace GameOfLife.AvaloniaApp;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
        {
            var w = new MainWindow
            {
                DataContext = new MainViewModel(),
                WindowState = WindowState.Normal,
            };
            d.MainWindow = w;
            w.Show();
            w.Activate();
            Dispatcher.UIThread.Post(() =>
            {
                w.WindowState = WindowState.Normal;
                w.Activate();
            });
        }
        base.OnFrameworkInitializationCompleted();
    }
}
