using System;

namespace GameOfLife.AvaloniaApp;

public class Grid
{
    bool[] cells = Array.Empty<bool>();
    int cols,
        rows;
    public bool this[int x, int y] =>
        x >= 0 && y >= 0 && x < cols && y < rows && cells[y * cols + x];

    public void Resize(int c, int r)
    {
        var newCols = Math.Max(1, c);
        var newRows = Math.Max(1, r);
        var n = new bool[newCols * newRows];
        for (var y = 0; y < Math.Min(rows, newRows); y++)
        for (var x = 0; x < Math.Min(cols, newCols); x++)
            n[y * newCols + x] = cells[y * cols + x];
        cols = newCols;
        rows = newRows;
        cells = n;
    }

    public void Toggle(int x, int y)
    {
        if (x < 0 || y < 0 || x >= cols || y >= rows)
            return;
        cells[y * cols + x] = !cells[y * cols + x];
    }

    public void Clear() => Array.Fill(cells, false);

    public void Randomise()
    {
        var r = Random.Shared;
        for (var i = 0; i < cells.Length; i++)
            cells[i] = r.NextDouble() < 0.2;
    }

    int CountNeighbours(int x, int y, bool wrap)
    {
        int n = 0;
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
        {
            if (dx == 0 && dy == 0)
                continue;
            int nx = x + dx,
                ny = y + dy;
            if (wrap)
            {
                if (nx < 0)
                    nx = cols - 1;
                if (ny < 0)
                    ny = rows - 1;
                if (nx >= cols)
                    nx = 0;
                if (ny >= rows)
                    ny = 0;
                if (this[nx, ny])
                    n++;
            }
            else if (this[nx, ny])
                n++;
        }
        return n;
    }

    public Grid Next(bool wrap)
    {
        var g = new Grid
        {
            cols = cols,
            rows = rows,
            cells = new bool[cells.Length],
        };
        for (var y = 0; y < rows; y++)
        for (var x = 0; x < cols; x++)
        {
            var n = CountNeighbours(x, y, wrap);
            var alive = this[x, y];
            g.cells[y * cols + x] = n == 3 || (alive && n == 2);
        }
        return g;
    }
}
