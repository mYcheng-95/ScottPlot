using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScottPlot.Statistics
{
    public struct WindInfo
    {
        /// <summary>
        /// 风速
        /// </summary>
        public double Speed;
        /// <summary>
        /// 方向
        /// </summary>
        public double Direction;

        public WindInfo(double speed, double direction)
        {
            (Speed, Direction) = (speed, direction);
        }
       
    }
}
