using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameOfLife.AvaloniaApp;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    void Notify([CallerMemberName] string? p = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

    int cols,
        rows,
        cellSize = 10,
        intervalMs = 120;
    bool running,
        wrap;
    Grid grid = new();
    public int CellSize
    {
        get => cellSize;
        set
        {
            if (cellSize == value)
                return;
            cellSize = value;
            Notify();
        }
    }
    public int IntervalMs
    {
        get => intervalMs;
        set
        {
            if (intervalMs == value)
                return;
            intervalMs = value;
            Notify();
        }
    }
    public bool Wrap
    {
        get => wrap;
        set
        {
            if (wrap == value)
                return;
            wrap = value;
            Notify();
        }
    }
    public bool this[int x, int y] => grid[x, y];
    public ICommand Run =>
        new Relay(() =>
        {
            running = !running;
            if (running)
                Loop();
            Notify(nameof(RunText));
        });
    public ICommand Step =>
        new Relay(() =>
        {
            grid = grid.Next(wrap);
            Invalidate();
        });
    public ICommand Clear =>
        new Relay(() =>
        {
            grid.Clear();
            Invalidate();
        });
    public ICommand Randomise =>
        new Relay(() =>
        {
            grid.Randomise();
            Invalidate();
        });
    public string RunText => running ? "Stop" : "Run";

    void Invalidate()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    async void Loop()
    {
        while (running)
        {
            await Task.Delay(intervalMs);
            grid = grid.Next(wrap);
            Invalidate();
        }
    }

    public void EnsureSize(int c, int r)
    {
        if (c == cols && r == rows)
            return;
        cols = c;
        rows = r;
        grid.Resize(cols, rows);
    }

    public void ToggleAt(Avalonia.Point p)
    {
        var x = (int)(p.X / cellSize);
        var y = (int)(p.Y / cellSize);
        grid.Toggle(x, y);
        Invalidate();
    }
}

public class Relay : ICommand
{
    readonly Action a;
    readonly Func<bool>? c;

    public Relay(Action a, Func<bool>? canExecute = null)
    {
        this.a = a;
        c = canExecute;
    }

    public bool CanExecute(object? parameter) => c?.Invoke() ?? true;

    public event EventHandler? CanExecuteChanged;

    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public void Execute(object? parameter)
    {
        a();
        NotifyCanExecuteChanged();
    }
}
