﻿namespace ScottPlot.Panels;

public class BoxPanel : IPanel
{
    public bool IsVisible { get; set; } = true;
    public Edge Edge { get; set; }
    public float Size { get; set; }
    public bool ShowDebugInformation { get; set; } = false;
    public float MinimumSize { get; set; } = 0;
    public float MaximumSize { get; set; } = float.MaxValue;

    public float Measure() => IsVisible ? Size : 0;

    public BoxPanel(Edge edge, float size)
    {
        Edge = edge;
        Size = size;
    }

    public PixelRect GetPanelRect(PixelRect rect, float size, float offset)
    {
        if (!IsVisible)
            return PixelRect.Zero;

        return Edge switch
        {
            Edge.Left => new SKRect(rect.Left - Size, rect.Top, Size, rect.Height).ToPixelRect(),
            Edge.Right => new SKRect(rect.Right, rect.Top, Size, rect.Height).ToPixelRect(),
            Edge.Bottom => new SKRect(rect.Left, rect.Bottom, rect.Width, Size).ToPixelRect(),
            Edge.Top => new SKRect(rect.Left, rect.Top - Size, rect.Width, Size).ToPixelRect(),
            _ => throw new NotImplementedException()
        };
    }

    public void Render(RenderPack rp, float size, float offset)
    {
        if (!IsVisible)
            return;

        PixelRect panelRect = GetPanelRect(rp.DataRect, size, offset);
        Drawing.DrawRectangle(rp.Canvas, panelRect, Colors.LightGray);
    }
}
