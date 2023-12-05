﻿namespace ScottPlot.Panels;

public class ColorBar : IPanel
{
    public bool IsVisible { get; set; } = true;

    public IHasColorAxis Source { get; set; }

    public Edge Edge { get; set; }
    public float Width { get; set; } = 50;
    public float Margin { get; set; } = 15;
    public bool ShowDebugInformation { get; set; } = false;
    public float MinimumSize { get; set; } = 0;
    public float MaximumSize { get; set; } = float.MaxValue;

    public ColorBar(IHasColorAxis source, Edge edge = Edge.Right)
    {
        Source = source;
        Edge = edge;
    }

    // Unfortunately the size of the axis depends on the size of the plotting window, so we just have to guess here. 2000 should be larger than most
    public float Measure() => IsVisible ? Margin + GetAxis(2000).Measure() + Width : 0;

    public PixelRect GetPanelRect(PixelRect dataRect, float size, float offset)
    {
        if (!IsVisible)
            return PixelRect.Zero;

        return Edge switch
        {
            Edge.Left => new SKRect(dataRect.Left - Width, dataRect.Top, dataRect.Left, dataRect.Top + dataRect.Height).ToPixelRect(),
            Edge.Right => new SKRect(dataRect.Right, dataRect.Top, dataRect.Right + Width, dataRect.Top + dataRect.Height).ToPixelRect(),
            Edge.Bottom => new SKRect(dataRect.Left, dataRect.Bottom, dataRect.Left + dataRect.Width, dataRect.Bottom + Width).ToPixelRect(),
            Edge.Top => new SKRect(dataRect.Left, dataRect.Top - Width, dataRect.Left + dataRect.Width, dataRect.Top).ToPixelRect(),
            _ => throw new NotImplementedException()
        };
    }

    public void Render(RenderPack rp, float size, float offset)
    {
        if (!IsVisible)
            return;

        using var _ = new SKAutoCanvasRestore(rp.Canvas);

        PixelRect panelRect = GetPanelRect(rp.DataRect, size, offset);

        SKPoint marginTranslation = GetTranslation(Margin);
        SKPoint axisTranslation = GetTranslation(Width);

        using var bmp = GetBitmap();

        rp.Canvas.Translate(marginTranslation);
        rp.Canvas.DrawBitmap(bmp, panelRect.ToSKRect());

        var colorbarLength = Edge.IsVertical() ? rp.DataRect.Height : rp.DataRect.Width;
        var axis = GetAxis(colorbarLength);

        rp.Canvas.Translate(axisTranslation);
        axis.Render(rp, size, offset);
    }

    private SKPoint GetTranslation(float magnitude) => Edge switch
    {
        Edge.Left => new(-magnitude, 0),
        Edge.Right => new(magnitude, 0),
        Edge.Bottom => new(0, magnitude),
        Edge.Top => new(0, -magnitude),
        _ => throw new ArgumentOutOfRangeException(nameof(Edge))
    };

    private SKBitmap GetBitmap()
    {
        uint[] argbs = Enumerable.Range(0, 256).Select(i => Source.Colormap.GetColor((Edge.IsVertical() ? 255 - i : i) / 255f).ARGB).ToArray();

        int bmpWidth = Edge.IsVertical() ? 1 : 256;
        int bmpHeight = !Edge.IsVertical() ? 1 : 256;

        return Drawing.BitmapFromArgbs(argbs, bmpWidth, bmpHeight);
    }

    private IAxis GetAxis(float length)
    {
        IAxis axis = Edge switch
        {
            Edge.Left => new AxisPanels.LeftAxis(),
            Edge.Right => new AxisPanels.RightAxis(),
            Edge.Bottom => new AxisPanels.BottomAxis(),
            Edge.Top => new AxisPanels.TopAxis(),
            _ => throw new ArgumentOutOfRangeException(nameof(Edge))
        };

        axis.Label.Text = "";

        var range = Source.GetRange();
        axis.Min = range.Min;
        axis.Max = range.Max;

        axis.TickGenerator.Regenerate(axis.Range, axis.Edge, length);

        return axis;
    }
}
