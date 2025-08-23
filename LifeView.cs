using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace YetAnotherLife;

public class LifeView : Control
{
    public static readonly StyledProperty<MainViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<LifeView, MainViewModel?>(nameof(ViewModel));
    public MainViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    static LifeView()
    {
        ViewModelProperty.Changed.AddClassHandler<LifeView>(
            (o, e) =>
            {
                if (e.OldValue is MainViewModel ov)
                    ov.PropertyChanged -= o.OnVmChanged;
                if (e.NewValue is MainViewModel nv)
                    nv.PropertyChanged += o.OnVmChanged;
                o.InvalidateVisual();
            }
        );
    }

    void OnVmChanged(object? s, PropertyChangedEventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
            Dispatcher.UIThread.Post(InvalidateVisual);
        else
            Dispatcher.UIThread.Post(InvalidateVisual);
    }

    int lastCx = -1,
        lastCy = -1;

    void ToggleCellAt(Point p)
    {
        if (ViewModel == null)
            return;
        var s = ViewModel.CellSize;
        var cx = (int)(p.X / s);
        var cy = (int)(p.Y / s);
        if (cx == lastCx && cy == lastCy)
            return;
        lastCx = cx;
        lastCy = cy;
        ViewModel.ToggleAt(p);
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        ToggleCellAt(e.GetPosition(this));
        e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var props = e.GetCurrentPoint(this).Properties;
        if (props.IsLeftButtonPressed)
            ToggleCellAt(e.GetPosition(this));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        lastCx = lastCy = -1;
        if (ReferenceEquals(e.Pointer.Captured, this))
            e.Pointer.Capture(null);
    }

    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);
        if (ViewModel == null)
            return;
        var s = ViewModel.CellSize;
        var b = Bounds;
        var cols = (int)(b.Width / s);
        var rows = (int)(b.Height / s);
        ViewModel.EnsureSize(cols, rows);
        var alive = Brushes.White;
        var dead = Brushes.Black;
        ctx.FillRectangle(dead, b);
        for (var y = 0; y < rows; y++)
        for (var x = 0; x < cols; x++)
            if (ViewModel[x, y])
                ctx.FillRectangle(alive, new Rect(x * s + 1, y * s + 1, s - 2, s - 2));
    }
}
