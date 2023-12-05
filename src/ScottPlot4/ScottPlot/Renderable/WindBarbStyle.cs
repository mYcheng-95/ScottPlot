using ScottPlot.Drawing;
using ScottPlot.Plottable.AxisManagers;
using ScottPlot.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScottPlot.Renderable
{
    public class WindBarbStyle
    {
        /// <summary>
        /// Describes which part of the vector line will be placed at the data coordinates.
        /// </summary>
        public ArrowAnchor Anchor = ArrowAnchor.Center;

        /// <summary>
        /// If enabled arrowheads will be drawn as lines scaled to each vector's magnitude.
        /// </summary>
        public bool ScaledArrowheads;

        /// <summary>
        /// When using scaled arrowheads this defines the width of the arrow relative to the vector line's length.
        /// </summary>
        public double ScaledArrowheadWidth = 0.15;

        /// <summary>
        /// When using scaled arrowheads this defines length of the arrowhead relative to the vector line's length.
        /// </summary>
        public double ScaledArrowheadLength = 0.5;

        /// <summary>
        /// Length of the scaled arrowhead
        /// </summary>
        private double ScaledTipLength => Math.Sqrt(ScaledArrowheadLength * ScaledArrowheadLength + ScaledArrowheadWidth * ScaledArrowheadWidth);

        /// <summary>
        /// Width of the scaled arrowhead
        /// </summary>
        private double ScaledHeadAngle => Math.Atan2(ScaledArrowheadWidth, ScaledArrowheadLength);

        /// <summary>
        /// Size of the arrowhead if custom/scaled arrowheads are not in use
        /// </summary>
        public float NonScaledArrowheadWidth = 2;

        /// <summary>
        /// Size of the arrowhead if custom/scaled arrowheads are not in use
        /// </summary>
        public float NonScaledArrowheadLength = 2;

        /// <summary>
        /// Marker drawn at each coordinate
        /// </summary>
        public MarkerShape MarkerShape = MarkerShape.filledCircle;

        /// <summary>
        /// Size of markers to be drawn at each coordinate
        /// </summary>
        public float MarkerSize = 0;

        /// <summary>
        /// Thickness of the arrow lines
        /// </summary>
        public float LineWidth = 1;


        /// <summary>
        /// Colormap used to translate heatmap values to colors
        /// </summary>
        public Colormap Colormap { get; private set; } = Colormap.Viridis;
        /// <summary>
        /// If defined, colors will be "clipped" to this value such that lower values (lower colors) will not be shown
        /// </summary>
        public double? ScaleMin { get; set; }

        /// <summary>
        /// If defined, colors will be "clipped" to this value such that greater values (higher colors) will not be shown
        /// </summary>
        public double? ScaleMax { get; set; }
        /// <summary>
        /// Value of the the lower edge of the colormap
        /// </summary>
        public double ColormapMin => ScaleMin ?? Min;

        /// <summary>
        /// Value of the the upper edge of the colormap
        /// </summary>
        public double ColormapMax => ScaleMax ?? Max;

        /// <summary>
        /// Minimum heatmap value
        /// </summary>
        private double Min;

        /// <summary>
        /// Maximum heatmap value
        /// </summary>
        private double Max;

        private double dataScaleX = 1;
        private double dataScaleY = 1;
        /// <summary>
        /// Render an evenly-spaced 2D vector field.
        /// </summary>
        public void Render(PlotDimensions dims, Graphics gfx, double[] xs, double[] ys, WindInfo[,] windInfos, Color[] colors)
        {
            // 算比率
            int scale = (int)Math.Ceiling((double)xs.Length / (double)ys.Length);
            if (scale > 1) {
                //   x 比 y多
                // 放大y
                dataScaleY = (double)scale;
                dataScaleX = 1;

            }
            else
            {
                scale = (int)Math.Ceiling((double)ys.Length / (double)xs.Length);
                if(scale > 1) {
                    // 放大x
                    dataScaleX = (double)scale;
                    dataScaleY = 1;
                }
                else
                {
                    dataScaleX = 1;
                    dataScaleY = 1;
                }
            }

            //Colormap = colormap ?? Colormap;
            //ScaleMin = min;
            //ScaleMax = max;
            for (int i = 0; i < xs.Length; i++)
            {
                for (int j = 0; j < ys.Length; j++)
                {
                    Coordinate coordinate = new(xs[i], ys[j]);
                    CoordinateVector vector = new(windInfos[i, j].Speed, windInfos[i, j].Direction);
                    Color color = colors[i * ys.Length + j];
                    RenderWindBarb(dims, gfx, coordinate, vector, color);
                   
                }
            
            }
        }

        /// <summary>
        /// Render a single arrow placed anywhere in coordinace space
        /// </summary>
        public void RenderWindBarb(PlotDimensions dims, Graphics gfx, Coordinate pt, CoordinateVector vec, Color color)
        {
            double x = pt.X; // 中心点横坐标
            double y = pt.Y; // 中心点纵坐标
            double xVector = vec.X; // 风速值
            double yVector = vec.Y; // 风向

            float tailX, tailY, endX, endY;
            switch (Anchor)
            {
                case ArrowAnchor.Base:
                    tailX = dims.GetPixelX(x);
                    tailY = dims.GetPixelY(y);
                    endX = dims.GetPixelX(x + xVector);
                    endY = dims.GetPixelY(y + yVector);
                    break;
                case ArrowAnchor.Center:
                    tailX = dims.GetPixelX(x - xVector / 2);
                    tailY = dims.GetPixelY(y - yVector / 2);
                    endX = dims.GetPixelX(x + xVector / 2);
                    endY = dims.GetPixelY(y + yVector / 2);
                    break;
                case ArrowAnchor.Tip:
                    tailX = dims.GetPixelX(x - xVector);
                    tailY = dims.GetPixelY(y - yVector);
                    endX = dims.GetPixelX(x);
                    endY = dims.GetPixelY(y);
                    break;
                default:
                    throw new NotImplementedException("unsupported anchor type");
            }

            using Pen pen = Drawing.GDI.Pen(color, LineWidth, rounded: true);

            if (false)
            {
                DrawFancyArrow(gfx, pen, tailX, tailY, endX, endY);
            }
            else
            {
                DrawStandardArrow(dims,gfx, pen, x, y, (float)xVector, (float)yVector);
            }

            DrawMarker(dims, gfx, pen, x, y);
        }

        private void DrawMarker(PlotDimensions dims, Graphics gfx, Pen pen, double x, double y)
        {
            if (MarkerShape != MarkerShape.none && MarkerSize > 0)
            {
                PointF markerPoint = new(dims.GetPixelX(x), dims.GetPixelY(y));
                MarkerTools.DrawMarker(gfx, markerPoint, MarkerShape, MarkerSize, pen.Color);
            }
        }
        private static readonly double _default_width = 10;
        private static readonly double _default_height = 25;
        //private static double width = _default_width;
        //private static double height = _default_height;
        //private static  double _width = width / 3;
        //private static  double _height = height / 2;
        private void DrawStandardArrow(PlotDimensions dims, Graphics gfx, Pen pen, double x, double y, float speed, float direction)
        {

            /***
             * x = tailX h横坐标
             * y = tailY 纵坐标
             * speed = endX 风速值
             * direction = endY 风向值   
             */
            // 第一件事情，画竖直杆子
            //    这里的竖杆应当是固定值
            // 二： 长横代表 4m/s 短横 2m/s 三角形代表 20m/s

            //  pen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(NonScaledArrowheadWidth, NonScaledArrowheadLength);
            //gfx.DrawLine(pen, x, y, speed, direction);
            //gfx.RotateTransform(0); // 旋转

            // Create pen.
            // Pen pen = new Pen(Color.Black, 3);

            // Create array of points that define lines to draw.
            //pen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(NonScaledArrowheadWidth, NonScaledArrowheadLength);
            var dx = speed - x;
            var dy = direction - y;
            pen.Width = 0.5f;

            //if (dataScaleX > 1)
            //{
            //    x = (x) * dataScaleX + dataScaleX / 2.0;

            //}
            //if (dataScaleY > 1)
            //{
            //    y = (y) * dataScaleY + dataScaleY / 2.0;

            //}
            //y = (y ) * 2 + 1;
            PointF defaultPoint = new(dims.GetPixelX(x), dims.GetPixelY((y)));
            gfx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            System.Drawing.Drawing2D.GraphicsState graphicsState = gfx.Save();
            gfx.TranslateTransform(defaultPoint.X, defaultPoint.Y);
            // 应用角度旋转
            gfx.RotateTransform(direction);
          //  Trace.WriteLine(direction);
            //  Bitmap bitmap = GenerateWindBarb(10);
            Bitmap bitmap = DrawBarb(dims);
            double X = bitmap.Width;
            double Y = bitmap.Height;
            //float width, height;
            //width = dims.GetPixelX(X) - defaultPoint.X; ;
            //height = dims.GetPixelY(Y) - defaultPoint.Y;
            // 长横定义为 0.4 短横定义为 0.2 宽度 整个标记定义宽高为 0.4 0.8
            // 需要保持旋转中心的位置位于整个标记的中心
            //      中心点位于  (x,y) ，该点为坐标点
            // 第一个点 （x-0.2,y-0.4)
            // 第二个点 （x-0.2,y+0.4)
            // 根据风速大小，计算需要多少长横多少短横
            // 第一个横  起点 （x-0.2,y+0.4)  终点（x+0.2,y+0.4）
            // 第二个横  起点 （x-0.2,y+0.3)  终点（x0.2,y+0.3）
            float dataWidth = dims.DataWidth;

            double unitsPerPxX = dims.PxPerUnitX*0.5;
            // 取2/3 的宽度
           // width = unitsPerPxX * 1.0 / 2.0;

            // 第一，风羽的宽度不可以超过风向杆的长度，且一般不会超过风向杆一半
            double unitsPerPxY = dims.PxPerUnitY * 0.7;
            double width = _default_width;
            double height = _default_height;
            double _height = _default_height/2.0;
            //if(unitsPerPxX < width)
            //{
            //  //  width = unitsPerPxX;

            //}
            //else
            //{
            //    width = unitsPerPxX>;
                
            //}
            if(unitsPerPxY < height)
            {
                height = _default_height;
              
            }
            else
            {
                height = unitsPerPxY;
                //_height = height / 2.0;
            }
            _height = height / 2.0;
            // 将风向杆分成2等分（一般风速羽位置不会超过风向杆一半的高度）
            //_height = unitsPerPxY / 2;
            // 取前一半后将该高度分成12等分，20m/s 占2格，其余占一个刻度 长杆4 短杆2 20+4*10 = 60，最大支持60m/s
            //_height = unitsPerPxY / 2.0;
            double unitsBarbY = _height / 12.0;
            // 如果 宽度的3分之2大于 y的高度
            if (width < _height*0.7)
            {
                width = _height * 0.7;
            }

            double _width = width / 2.0;
            PointF[] points =
                    {
                 new PointF((float)dims.GetPixelX(x) - defaultPoint.X,  (float)(dims.GetPixelY((y)) +(-_height))- defaultPoint.Y),
                 new PointF((float)dims.GetPixelX(x) - defaultPoint.X,  (float)(dims.GetPixelY((y))+(_height)) - defaultPoint.Y)

             };
            gfx.DrawLines(pen, points);
           
            float dataHeight = dims.DataHeight;
            // 画一个实心三角
            // 起点 起点 （x-0.2,y+0.4)  中间点（x+0.2,y+0.4） 终点  （x-0.2,y+0.2)
            PointF[] points1 =
                    {
                 new PointF((float)dims.GetPixelX((x)) - defaultPoint.X,  (float)(dims.GetPixelY((y)) -_height)- defaultPoint.Y),
                 new PointF((float)dims.GetPixelX((x)) - defaultPoint.X+(float)(+width),  (float)(dims.GetPixelY((y))-(_height)) - defaultPoint.Y),
                 new PointF((float)dims.GetPixelX((x)) - defaultPoint.X,  (float)(dims.GetPixelY((y))-(_height-(unitsBarbY*2))) - defaultPoint.Y )

             };
            Brush brush = new SolidBrush(pen.Color);
            gfx.FillPolygon(brush, points1);
            //gfx.DrawLines(pen, points);
            // 第二个横  起点 （x-0.3,y+0.3)  终点（x0.3,y+0.3）
            //PointF[] points2 =
            //        {
                 
            //     new PointF((float)dims.GetPixelX((x-_width)),  (float)dims.GetPixelY((y+_height))),

            //     new PointF((float)dims.GetPixelX((x+_width)), (float)dims.GetPixelY((y+_height)))
            // };
            PointF[] points3 =
                    {

                 new PointF((float)dims.GetPixelX((x)) - defaultPoint.X,  (float)(dims.GetPixelY((y))-(+_height-(unitsBarbY*3))) - defaultPoint.Y),

                 new PointF((float)dims.GetPixelX((x)) - defaultPoint.X+(float)(+_width), (float)(dims.GetPixelY((y))-(+_height-(unitsBarbY*3))) - defaultPoint.Y)
             };
            //gfx.DrawLines(pen, points);
            // gfx.DrawLines(pen, points2);
             gfx.DrawLines(pen, points3);

            // RectangleF rect = new(ImageLocationOffset(width, height), new SizeF(width, height));
            // // 恢复坐标系到初始状态

            //// gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            //// gfx.SmoothingMode = SmoothingMode.None;
            // gfx.DrawImage(bitmap, rect);
            // 保存当前坐标系状态

          
            // 将坐标系平移到风羽图的中心
            gfx.Restore(graphicsState);

        }
        /// <summary>
        /// Indicates which corner of the Bitmap is described by X and Y.
        /// This corner will be the axis of Rotation, and the center of Scale.
        /// </summary>
        public Alignment Alignment { get; set; } = ScottPlot.Alignment.MiddleCenter;
        private PointF ImageLocationOffset(float width, float height)
        {
            return Alignment switch
            {
                Alignment.LowerCenter => new PointF(-width / 2, -height),
                Alignment.LowerLeft => new PointF(0, -height),
                Alignment.LowerRight => new PointF(-width, -height),
                Alignment.MiddleLeft => new PointF(0, -height / 2),
                Alignment.MiddleRight => new PointF(-width, -height / 2),
                Alignment.UpperCenter => new PointF(-width / 2, 0),
                Alignment.UpperLeft => new PointF(0, 0),
                Alignment.UpperRight => new PointF(-width, 0),
                Alignment.MiddleCenter => new PointF(-width / 2, -height / 2),
                _ => throw new InvalidEnumArgumentException(),
            };
        }

        private static Bitmap DrawBarb(PlotDimensions dims)
        {
            Bitmap windBarb = new Bitmap(1, 1);
            using Pen pen = Drawing.GDI.Pen(Color.Black, 1, rounded: true);
            using (Graphics g = Graphics.FromImage(windBarb)) {
                PointF[] points =
                     {
                 new PointF(0,  0f),
                 new PointF(0,  0.9f)

                 };
                g.DrawLines(pen, points);

                // 画一个实心三角
                // 起点 起点 （x-0.2,y+0.4)  中间点（x+0.2,y+0.4） 终点  （x-0.2,y+0.2)
                PointF[] points1 =
                        {
                     new PointF(0,  0.9f),
                     new PointF(1f,  1f),
                     new PointF(0,  0.7f)

                };
                Brush brush = new SolidBrush(Color.Black);
                g.FillPolygon(brush, points1);

                PointF[] points3 =
                  {

                 new PointF(0,  0.6f),

                 new PointF(0.5f,0.55f)
             };
                //gfx.DrawLines(pen, points);
                // gfx.DrawLines(pen, points2);
                g.DrawLines(pen, points3);

                // 使用高质量的缩放模式

            }
            // 创建目标大小的 Bitmap
            Bitmap scaledBitmap = new Bitmap(1, 1);
            using (Graphics g = Graphics.FromImage(scaledBitmap))
            {
               
                //g.InterpolationMode = InterpolationMode.NearestNeighbor;
               // g.SmoothingMode = SmoothingMode.None;
                // 设置旋转中心为图像中心
                //g.TranslateTransform(windBarb.Width / 2, windBarb.Height / 2);
                //g.RotateTransform(0);
                // 应用旋转

                // 将旋转后的图像平移到正确的位置
                //g.TranslateTransform(-windBarb.Width / 2, -windBarb.Height / 2);
                g.DrawImage(windBarb, new RectangleF(0, 0,1, 1), new Rectangle(0, 0, windBarb.Width, windBarb.Height), GraphicsUnit.Pixel);
               
            }
                
            //gfx.DrawLines(pen, points);
            // 第二个横  起点 （x-0.3,y+0.3)  终点（x0.3,y+0.3）
            //PointF[] points2 =
            //        {

            //     new PointF((float)dims.GetPixelX((x-_width)),  (float)dims.GetPixelY((y+_height))),

            //     new PointF((float)dims.GetPixelX((x+_width)), (float)dims.GetPixelY((y+_height)))
            // };

            return windBarb;
        }
        private static Bitmap GenerateWindBarb(int windSpeed)
        {
            int barbLength = 8;
            int halfBarbLength = barbLength / 2;
            int barbSpacing = 2; // 适应新的宽度和高度

            Bitmap windBarb = new Bitmap(3, 8);

            using (Graphics g = Graphics.FromImage(windBarb))
            {
                // 计算需要绘制的羽毛数量
                int fullBarbs = windSpeed / 10;
                int halfBarbs = (windSpeed % 10) / 5;

                // 绘制整羽毛
                for (int i = 0; i < fullBarbs; i++)
                {
                    g.DrawLine(Pens.Black, 1, 8 - i * barbSpacing, 1, 8 - i * barbSpacing - barbLength);
                }

                // 绘制半羽毛
                for (int i = 0; i < halfBarbs; i++)
                {
                    g.DrawLine(Pens.Black, 1, 8 - (fullBarbs + i) * barbSpacing, 1, 8 - (fullBarbs + i) * barbSpacing - halfBarbLength);
                }

                // 绘制整体的风速表示
                g.DrawLine(Pens.Black, 1, 8 - (fullBarbs + halfBarbs) * barbSpacing, 1, 8 - (fullBarbs + halfBarbs) * barbSpacing - barbLength);

                // 在 (1, 8 - (fullBarbs + halfBarbs) * barbSpacing) 处绘制一个三角形，表示整体的风速
                Point[] trianglePoints = new Point[]
                {
            new Point(0, 8 - (fullBarbs + halfBarbs) * barbSpacing),
            new Point(2, 8 - (fullBarbs + halfBarbs) * barbSpacing),
            new Point(1, 8 - (fullBarbs + halfBarbs) * barbSpacing - barbLength)
                };
                g.FillPolygon(Brushes.Black, trianglePoints);
            }

            return windBarb;
        }
        private void DrawFancyArrow(Graphics gfx, Pen pen, float x1, float y1, float x2, float y2)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            var arrowAngle = (float)Math.Atan2(dy, dx);
            var sinA1 = (float)Math.Sin(ScaledHeadAngle - arrowAngle);
            var cosA1 = (float)Math.Cos(ScaledHeadAngle - arrowAngle);
            var sinA2 = (float)Math.Sin(ScaledHeadAngle + arrowAngle);
            var cosA2 = (float)Math.Cos(ScaledHeadAngle + arrowAngle);
            var len = (float)Math.Sqrt(dx * dx + dy * dy);
            var hypLen = len * ScaledTipLength;

            var corner1X = x2 - hypLen * cosA1;
            var corner1Y = y2 + hypLen * sinA1;
            var corner2X = x2 - hypLen * cosA2;
            var corner2Y = y2 - hypLen * sinA2;

            PointF[] arrowPoints =
            {
                new PointF(x1, y1), // 起点
                new PointF(x2, y2), // 终点
                new PointF((float)corner1X, (float)corner1Y),
                new PointF(x2, y2),
                new PointF((float)corner2X, (float)corner2Y),
            };
            Trace.WriteLine(string.Format("x:{0},y:{1} speed:{2},direction:{3} corner1X:{4},corner1Y:{5} corner2X:{6},corner2Y:{7}", x1,y1,x2,y2, corner1X, corner1Y, corner2X, corner2Y));
            //PointF[] points =
            //        {
            //     new PointF((float)(10*ScaledHeadAngle),  (float)(10*ScaledHeadAngle)),
            //     new PointF((float)(10*ScaledHeadAngle), (float)(100*ScaledHeadAngle)),
            //     new PointF((float)(200*ScaledHeadAngle),  (float)(50*ScaledHeadAngle)),
            //     new PointF((float)(250*ScaledHeadAngle), (float)(300*ScaledHeadAngle))
            // };
            gfx.DrawLines(pen, arrowPoints);
        }
    }
}
