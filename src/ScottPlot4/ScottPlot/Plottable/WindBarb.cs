using ScottPlot.Drawing;
using ScottPlot.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ScottPlot.Plottable
{
    public class WindBarb : IPlottable
    {
        private readonly double[] Xs;
        private readonly double[] Ys;

        /// <summary>
        /// Horizontal location of the lower-left cell
        /// </summary>
        public double OffsetX { get; set; } = 0;

        /// <summary>
        /// Vertical location of the lower-left cell
        /// </summary>
        public double OffsetY { get; set; } = 0;

        /// <summary>
        /// Width of each cell composing the heatmap
        /// </summary>
        public double CellWidth { get; set; } = 1;

        /// <summary>
        /// Height of each cell composing the heatmap
        /// </summary>
        public double CellHeight { get; set; } = 1;

        private readonly WindInfo[,] WindInfos;
        private readonly Color[] VectorColors;
        public string Label { get; set; } = null;
        public bool IsVisible { get; set; } = true;
        public int XAxisIndex { get; set; } = 0;
        public int YAxisIndex { get; set; } = 0;

        private readonly Renderable.WindBarbStyle WindBarbStyle = new();
        private readonly Renderable.ArrowStyle ArrowStyle = new();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vectors">包含两个内容，x，代表风速；y，代表方向</param>
        /// <param name="xs">x坐标</param>
        /// <param name="ys">y坐标</param>
        /// <param name="colormap"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="defaultColor"></param>
        public WindBarb(WindInfo[,] vectors, double[] xs, double[] ys, Colormap colormap, double scaleFactor, Color defaultColor)
        {


            double minMagnitudeSquared = vectors[0, 0].Speed;
            double maxMagnitudeSquared = vectors[0, 0].Speed;
            for (int i = 0; i < xs.Length; i++)
            {
                for (int j = 0; j < ys.Length; j++)
                {
                    if (vectors[i, j].Speed > maxMagnitudeSquared)
                        maxMagnitudeSquared = vectors[i, j].Speed;
                    else if (vectors[i, j].Speed < minMagnitudeSquared)
                        minMagnitudeSquared = vectors[i, j].Speed;
                }
            }
           // double minMagnitude = Math.Sqrt(minMagnitudeSquared);
            //double maxMagnitude = Math.Sqrt(maxMagnitudeSquared);

            double[,] intensities = new double[xs.Length, ys.Length];
            for (int i = 0; i < xs.Length; i++)
            {
                for (int j = 0; j < ys.Length; j++)
                {
                    if (colormap != null)
                            intensities[i, j] = (vectors[i, j].Speed - minMagnitudeSquared) / (maxMagnitudeSquared - minMagnitudeSquared);
                    //vectors[i, j] = Vector2.Multiply(vectors[i, j], (float)(scaleFactor / (maxMagnitude * 1.2)));
                }
            }

            double[] flattenedIntensities = intensities.Cast<double>().ToArray();
            VectorColors = colormap is null ?
                Enumerable.Range(0, flattenedIntensities.Length).Select(x => defaultColor).ToArray() :
                Colormap.GetColors(flattenedIntensities, colormap);

            this.WindInfos = vectors;
            this.Xs = xs;
            this.Ys = ys;
        }

        public AxisLimits GetAxisLimits()
        {
            return new AxisLimits(Xs.Min() - 1, Xs.Max() + 1, Ys.Min() - 1, Ys.Max() + 1);
        }

        public LegendItem[] GetLegendItems()
        {
            var singleItem = new LegendItem(this)
            {
                label = Label,
                color = VectorColors[0],
                lineWidth = 10,
                markerShape = MarkerShape.none
            };
            return LegendItem.Single(singleItem);
        }
        public int PointCount { get => WindInfos.Length; }

        public void Render(PlotDimensions dims, Bitmap bmp, bool lowQuality = false)
        {
            if (IsVisible == false)
                return;

            using Graphics gfx = GDI.Graphics(bmp, dims, lowQuality);
            // 绘制
            WindBarbStyle.Render(dims, gfx, Xs, Ys, WindInfos, VectorColors);

            //ArrowStyle.Render(dims, gfx, Xs, Ys, WindInfos, VectorColors);
        }

        public void ValidateData(bool deep = false)
        {
            
        }
    }
}
