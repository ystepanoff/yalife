using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace YetAnotherLife;

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

            var appMenu = new NativeMenu();

            var file = new NativeMenuItem("File");
            var fileMenu = new NativeMenu();
            var load = new NativeMenuItem("Load Configuration");
            load.Click += async (_, __) => await LoadConfigAsync(w);
            var save = new NativeMenuItem("Save Configuration");
            save.Click += async (_, __) => await SaveConfigAsync(w);
            fileMenu.Items.Add(load);
            fileMenu.Items.Add(save);
            file.Menu = fileMenu;
            appMenu.Items.Add(file);

            var help = new NativeMenuItem("Help");
            var helpMenu = new NativeMenu();
            var about = new NativeMenuItem("About");
            about.Click += OnAboutClick;
            helpMenu.Items.Add(about);
            help.Menu = helpMenu;
            appMenu.Items.Add(help);

            NativeMenu.SetMenu(w, appMenu);

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

    private async Task LoadConfigAsync(Window owner)
    {
        if (owner.DataContext is not MainViewModel vm)
            return;
        var files = await owner.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Load Configuration",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON")
                    {
                        Patterns = new[] { "*.json" },
                        MimeTypes = new[] { "application/json" },
                    },
                },
            }
        );
        if (files == null || files.Count == 0)
            return;
        await using var s = await files[0].OpenReadAsync();
        using var sr = new StreamReader(s);
        var json = await sr.ReadToEndAsync();
        vm.ImportFromJson(json);
    }

    private async Task SaveConfigAsync(Window owner)
    {
        if (owner.DataContext is not MainViewModel vm)
            return;
        var file = await owner.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save Configuration",
                SuggestedFileName = "life.json",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("JSON")
                    {
                        Patterns = new[] { "*.json" },
                        MimeTypes = new[] { "application/json" },
                    },
                },
            }
        );
        if (file == null)
            return;
        var json = vm.ExportToJson();
        await using var s = await file.OpenWriteAsync();
        using var sw = new StreamWriter(s);
        await sw.WriteAsync(json);
        await sw.FlushAsync();
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
