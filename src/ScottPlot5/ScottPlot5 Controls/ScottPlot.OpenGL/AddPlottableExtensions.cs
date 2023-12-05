﻿using ScottPlot.DataSources;

namespace ScottPlot;

/// <summary>
/// This class extends Plot.Add.* to add additional plottables provided by this NuGet package
/// </summary>
public static class AddPlottableExtensions
{
    /// <summary>
    /// Add an OpenGL-accelerated scatter plot
    /// </summary>
    public static Plottables.ScatterGL ScatterGL(this PlottableAdder add, IPlotControl control, double[] xs, double[] ys)
    {
        ScatterSourceXsYs source = new(xs, ys);
        IScatterSource sourceWithCaching = new CacheScatterLimitsDecorator(source);
        Plottables.ScatterGL sp = new(sourceWithCaching, control);
        Color nextColor = add.GetNextColor();
        sp.LineStyle.Color = nextColor;
        sp.MarkerStyle.Fill.Color = nextColor;
        add.Plottable(sp);
        return sp;
    }

    /// <summary>
    /// Add an OpenGL-accelerated scatter plot with customizable line width
    /// </summary>
    public static Plottables.ScatterGLCustom ScatterGLCustom(this PlottableAdder add, IPlotControl control, double[] xs, double[] ys)
    {
        DataSources.ScatterSourceXsYs data = new(xs, ys);
        Plottables.ScatterGLCustom sp = new(data, control);
        Color nextColor = add.GetNextColor();
        sp.LineStyle.Color = nextColor;
        sp.MarkerStyle.Fill.Color = nextColor;
        add.Plottable(sp);
        return sp;
    }
}
