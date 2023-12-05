﻿namespace ScottPlot.LayoutEngines;

/// <summary>
/// Generate layouts where the data area has a fixed padding from the edge of the figure
/// </summary>
public class FixedPadding : LayoutEngineBase, ILayoutEngine
{
    private PixelPadding Padding { get; }

    public FixedPadding(PixelPadding padding)
    {
        Padding = padding;
    }

    public Layout GetLayout(PixelRect figureRect, IEnumerable<IPanel> panels)
    {
        Dictionary<IPanel, float> panelSizes = LayoutEngineBase.MeasurePanels(panels);
        Dictionary<IPanel, float> panelOffsets = GetPanelOffsets(panels, panelSizes);

        PixelRect dataRect = new(
            left: figureRect.Left + Padding.Left,
            right: figureRect.Left + figureRect.Width - Padding.Right,
            bottom: figureRect.Top + figureRect.Height - Padding.Bottom,
            top: figureRect.Top + Padding.Top);

        return new Layout(figureRect, dataRect, panelSizes, panelOffsets);
    }
}
