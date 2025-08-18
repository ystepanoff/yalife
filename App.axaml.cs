using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace GameOfLife.AvaloniaApp;

public partial class App : Application
{
    public override void Initialize()
    {
        Name = "Game of Life";
        AvaloniaXamlLoader.Load(this);
    }

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

    private async void OnAboutClick(object? sender, EventArgs e)
    {
        if (
            ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime d
            || d.MainWindow is null
        )
            return;
        var dlg = new Window
        {
            Title = "About",
            Content = new TextBlock
            {
                Text = "Game of Life\nConway’s cellular automaton.\n© 2025 Yegor Stepanov",
                Margin = new Thickness(16),
            },
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        await dlg.ShowDialog(d.MainWindow);
    }
}
