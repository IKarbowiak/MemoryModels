using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska
{
    public class Model1 
    {
        public int length;
        public int speed;
        public double surface;

        public Model1(int lgth, int diam, int sp)
        {
            this.length = lgth;
            int diameter = diam;
            this.speed = sp;
            this.surface = Math.Pow((diameter / 2), 2) * Math.PI;
            double volume = this.surface * length;
        }

        public double[] Flow(int time)
        {
            // Time after which liquid will begin to flow out
            double outFlowTime = this.length / this.speed;

            // The volume of liquid which flowed up
            double flowedVolume = this.surface * this.speed * (time - outFlowTime);
            double[] res = { outFlowTime, flowedVolume };
            return res;
        }
    }
}
