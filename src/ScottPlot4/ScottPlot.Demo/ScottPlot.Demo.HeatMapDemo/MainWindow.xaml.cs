using ScottPlot.Cookbook.Categories.PlotTypes;
using ScottPlot.Drawing;
using ScottPlot.Plottable;
using ScottPlot.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScottPlot.Demo.HeatMapDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 水平风向热力图谱对象
        /// </summary>
        ScottPlot.Plot W_H_D_heat_Plot;

        private Plottable.Crosshair _Crosshair;
        /// <summary>
        /// 水平风速热力图
        /// </summary>
        Plottable.Heatmap W_H_Heat_Plot_heatmap = null;
        public MainWindow()
        {
            InitializeComponent();
            PloatInit();
            double[] xPositions = DataGen.Range(0, 150);
            double[] yPositions = DataGen.Range(0, 150);
            WindInfo[,] vectors = new WindInfo[xPositions.Length, yPositions.Length];
            double count = 0;
            for (int x = 0; x < xPositions.Length; x++)
                for (int y = 0; y < yPositions.Length; y++)
                    vectors[x, y] = new WindInfo(
           speed: Math.Sin(xPositions[x]/10*Math.PI+ yPositions [y]/100* Math.PI),
            direction: (count+=0.5)*Math.PI);
            Color[] colors = { Color.Indigo, Color.Blue, Color.Green, Color.Yellow, Color.Orange, Color.Red, };

            // display the colormap on the plot as a colorbar
            ScottPlot.Drawing.Colormap cmap = new Colormap(colors);
            this._wind.Plot.AddWindBarb(vectors, xPositions, yPositions,colormap: cmap);
            this.Dispatcher.Invoke(() =>
            {
                this._wind.Refresh();

            });
        }
        private void PloatInit()
        {
           // W_H_D_heat_Plot = this._heatmap.Plot;
            //_heatmap.MouseMove += plot_MouseMove;
           // _heatmap.MouseDoubleClick += plot_MouseMove;
            //_heatmap.MouseLeave += plot_MouseLeave;
        }

        private void plot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (W_H_Heat_Plot_heatmap != null)
            {
                _Crosshair.IsVisible = false;
                //this.Dispatcher.BeginInvoke(new Action(() => { _heatmap.Refresh(); }));
            }
        }

        private void plot_MouseMove(object sender, MouseEventArgs e)
        {
            if (W_H_Heat_Plot_heatmap != null)
            {
                //(double coordinateX, double coordinateY) = this._heatmap.GetMouseCoordinates();

                //_Crosshair.X = coordinateX;
                //_Crosshair.Y = coordinateY;
                //_Crosshair.IsVisible = true;
                //this.Dispatcher.BeginInvoke(new Action(() => { _heatmap.Refresh(); }));
            }
        }

        private void PlotClear()
        {
            if (W_H_Heat_Plot_heatmap != null)
            {
                W_H_D_heat_Plot.Remove(W_H_Heat_Plot_heatmap);
                W_H_D_heat_Plot.Remove(_Crosshair);

            }
            _Crosshair = null;
            W_H_Heat_Plot_heatmap = null;
        }
        int count =0;
        bool isStop = true;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(isStop)
            {
                Render();
                isStop = false;
            }
            else
            {
                isStop = true;
            }
            
        }

        
        private void Render()
        {
            //Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        if (isStop)
            //        {
            //            return;
            //        }
            //        count++;
            //        PlotClear();
            //        double[,] data = Generate.Sin2D(width: 1_0000, height: 1_000, xPeriod: 0.2 * count, yPeriod: 0.4 * count);
            //        if (W_H_Heat_Plot_heatmap == null)
            //        {

            //            W_H_Heat_Plot_heatmap = W_H_D_heat_Plot.AddHeatmap(data, lockScales: false);

            //            // opt into parallel processing
            //            W_H_Heat_Plot_heatmap.UseParallel = true;
            //            if (_Crosshair == null)
            //            {
            //                _Crosshair = W_H_D_heat_Plot.AddCrosshair(0, 0);
            //                // use the custom formatter for X and Y crosshair labels
            //                _Crosshair.HorizontalLine.PositionFormatter = FormatterYHair;
            //                _Crosshair.VerticalLine.PositionFormatter = FormatterXHair;
            //            }
            //        }
            //        Dispatcher.Invoke(new Action(() => {
            //           // _heatmap.Refresh();
            //        }));
            //        Thread.Sleep(1000);
            //    }
            //});
          
           
        }
        int x = 0;
        int y = 0;
        private string FormatterXHair(double arg)
        {
            int index = (int)Math.Floor(arg);
            x = index;
            return "" + arg;
        }
        private string FormatterYHair(double arg)
        {
            int index = (int)Math.Floor(arg);
            y = index;
            return ""+ arg;
        }
    }
}
