using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace GameOfLife.Avalonia;

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

    public void ToggleAt(global::Avalonia.Point p)
    {
        var x = (int)(p.X / cellSize);
        var y = (int)(p.Y / cellSize);
        grid.Toggle(x, y);
        Invalidate();
    }

    public string ExportToJson()
    {
        var total = cols * rows;
        var blocks = (total + 31 - 1) / 31;
        var raw = new int[blocks];
        int i = 0;
        for (int b = 0; b < blocks; b++)
        {
            int acc = 0;
            for (int bit = 0; bit < 31 && i < total; bit++, i++)
            {
                int y = i / cols;
                int x = i % cols;
                if (grid[x, y])
                    acc |= 1 << bit;
            }
            raw[b] = acc;
        }
        List<int> rle = new(raw.Length * 2);
        for (int idx = 0; idx < raw.Length; )
        {
            int v = raw[idx];
            int count = 1;
            while (idx + count < raw.Length && raw[idx + count] == v)
                count++;
            rle.Add(v);
            rle.Add(count);
            idx += count;
        }
        return JsonSerializer.Serialize(
            new LifeConfig
            {
                Columns = cols,
                Rows = rows,
                Data = rle.ToArray(),
            }
        );
    }

    public void ImportFromJson(string json)
    {
        var cfg = JsonSerializer.Deserialize<LifeConfig>(json);
        if (cfg == null || cfg.Rows <= 0 || cfg.Columns <= 0)
            return;
        cols = cfg.Columns;
        rows = cfg.Rows;
        grid.Resize(cols, rows);
        var total = cols * rows;
        var expectedBlocks = (total + 31 - 1) / 31;
        int[] blocks;
        if (cfg.Compressed?.Length >= 2)
        {
            var comp = cfg.Compressed;
            var list = new List<int>();
            for (int i = 0; i + 1 < comp.Length; i += 2)
            {
                int v = comp[i];
                int count = Math.Max(0, comp[i + 1]);
                for (int k = 0; k < count; k++)
                    list.Add(v);
            }
            blocks = [.. list];
        }
        else if (cfg.Data != null)
        {
            if (cfg.Data.Length == expectedBlocks)
            {
                blocks = cfg.Data;
            }
            else if (cfg.Data.Length % 2 == 0)
            {
                int sum = 0;
                for (int i = 1; i < cfg.Data.Length; i += 2)
                    sum += Math.Max(0, cfg.Data[i]);
                if (sum == expectedBlocks)
                {
                    List<int> list = new(expectedBlocks);
                    for (int i = 0; i + 1 < cfg.Data.Length; i += 2)
                    {
                        int v = cfg.Data[i];
                        int count = Math.Max(0, cfg.Data[i + 1]);
                        for (int k = 0; k < count; k++)
                            list.Add(v);
                    }
                    blocks = [.. list];
                }
                else
                {
                    blocks = [];
                }
            }
            else
            {
                blocks = [];
            }
        }
        else
        {
            return;
        }
        int cellIndex = 0;
        for (int bi = 0; bi < blocks.Length && cellIndex < total; bi++)
        {
            int acc = blocks[bi];
            for (int bit = 0; bit < 31 && cellIndex < total; bit++, cellIndex++)
            {
                int y = cellIndex / cols;
                int x = cellIndex % cols;
                grid.Set(x, y, ((acc >> bit) & 1) == 1);
            }
        }
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

public record LifeConfig
{
    public int Columns { get; init; }
    public int Rows { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? Compressed { get; init; }
}
