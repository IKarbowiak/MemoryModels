using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PracaMagisterska
{
    public abstract class Model
    {
        public double length;
        public double speed;
        public double input_surface;

        public abstract double[] Flow(double time);
    }
    public class Model1 : Model
    {
        public Model1(double lgth, double diam, double sp)
        {
            this.length = lgth;
            double diameter = diam;
            this.speed = sp;
            this.input_surface = Math.Pow((diameter / 2), 2) * Math.PI;
            double volume = this.input_surface * length;
        }

        public override double[] Flow(double time)
        {
            // Time after which liquid will begin to flow out
            double outFlowTime = this.length / this.speed;

            if (outFlowTime > time)
            {
                double[] res0 = { 0, 0 };
                return res0;
            }

            //var aTimer = new System.Timers.Timer(1000);

            //aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            //aTimer.Interval = 1000;
            //aTimer.Enabled = true;

            // The volume of liquid which flowed up
            double flowedVolume = this.input_surface * this.speed * (time - outFlowTime);
            double[] res = { outFlowTime, flowedVolume };
            return res;
        }

        //public void OnTimedEvent(object source, ElapsedEventArgs e)
        //{
        //    Console.WriteLine("Ha");
        //}
    }

    public class Model2 : Model
    {
        public double dendrite_length, axon_length, soma_diameter;
        public double dendrite_volume, soma_volume, axon_volume;
        public double soma_treshold;

        public Model2(double lgth, double diam, double sp)
        {
            this.length = lgth;
            double diameter = diam;
            this.speed = sp;

            calculate_parameters(diameter);

            //Console.WriteLine(this.dendrite_length);
            //Console.WriteLine(this.axon_length);
            //Console.WriteLine(this.soma_diameter);
            //Console.WriteLine(this.soma_treshold);
            //Console.WriteLine(this.soma_volume);
  
        }

        public void calculate_parameters(double diameter)
        {
            this.dendrite_length = this.length / 26;
            this.soma_diameter = this.length * 10 / 26;
            this.axon_length = this.length * 20 / 26;
            this.input_surface = Math.Pow((diameter / 2), 2) * Math.PI;
            this.dendrite_volume = this.dendrite_length * this.input_surface;
            this.axon_volume = this.axon_length * this.input_surface;
            this.soma_volume = Math.Pow(this.soma_diameter / 2, 2) * Math.PI * this.soma_diameter;
            this.soma_treshold = (this.soma_diameter / 2 - diameter / 2) * this.soma_volume / this.soma_diameter;
        }

        public override double[] Flow(double time)
        {

            double time_for_fill_soma = this.soma_treshold / (this.input_surface * this.speed);
            Console.WriteLine("Time For Fill some");
            Console.WriteLine(time_for_fill_soma);

            // Time after which liquid will begin to flow out
            double outFlowTime = this.length / this.speed + time_for_fill_soma;

            if (outFlowTime > time)
            {
                double[] res0 = { 0, 0 };
                return res0;
            }

            // The volume of liquid which flowed up
            double flowedVolume = this.input_surface * this.speed * (time - outFlowTime);
            double[] res = { outFlowTime, flowedVolume };
            return res;
        }
    }

    public class Model3 : Model2
    {
        public Model3(double lgth, double diam, double sp) : base(lgth, diam, sp) { }

        public override double[] Flow(double time)
        {
            double time_for_fill_soma = this.soma_treshold / (this.input_surface * this.speed * 2);

            double outFlowTime = (this.input_surface * (this.axon_length + this.soma_diameter)) / (this.input_surface * this.speed);

            if (outFlowTime > time)
            {
                double[] res0 = { 0, 0 };
                return res0;
            }

            double flowedVolume = this.input_surface * this.speed * (time - outFlowTime) * 2;
            double[] res = { outFlowTime, flowedVolume};
            return res;
        }
    }
}
