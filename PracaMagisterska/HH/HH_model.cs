using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenterSpace.NMath.Analysis;
using CenterSpace.NMath.Core;
using System.Windows.Forms;
using OxyPlot;

namespace PracaMagisterska
{
    class HH_model
    {
        private HH_neuron hh_neuron;
        public List<DataPoint> potential_points { get; private set; }
        public List<DataPoint> current_points { get; private set; }

        public HH_model()
        {

        }


        public Tuple<List<DataPoint>, List<DataPoint>> start_action()
        {
            hh_neuron = new HH_neuron();
            this.potential_points = new List<DataPoint> { new DataPoint(0.0, 0.0) };
            this.current_points = new List<DataPoint> { new DataPoint(0.0, 0.0) };
            this.euler(this.Rigid, new double[] { 0, 0.31767691, 0.05293249, 0.59612075 }, 0.0, 50.0, 0.001);
            Tuple<List<DataPoint>, List<DataPoint>> res = Tuple.Create(this.potential_points, this.current_points);
            return res;
        }


        private List<double> euler(Func<double, double[], double[]> hh, double[] start_parameters, double start_t, double end_t, double time_step)
        {
            List<double> V_values = new List<double>() {start_parameters[0] };
            double t = start_t;
            double[] y = start_parameters;

            while (t < end_t)
            {
                t += time_step;
                double[] params_increment = this.Rigid(t, y);
                for (int i = 0; i < params_increment.Length; i++)
                {
                    y[i] += time_step * params_increment[i];
                }

                this.potential_points.Add(new DataPoint(t, y[0]));
                this.current_points.Add(new DataPoint(t, I_inj(t)));
                V_values.Add(y[0]);
            }

            return V_values;

        }

        public double[] Rigid(double t, double[] y)
        {
            double V = y[0];
            double m = y[1];
            double h = y[2];
            double n = y[3];

            double dvdt = (I_inj(t) - this.hh_neuron.I_Na(V, m, h) - this.hh_neuron.I_K(V, n) - this.hh_neuron.I_Cl(V)) / this.hh_neuron.cm;
            double dmdt = this.alpha_m(1 - m) - this.beta_m(V) * m;
            double dhdt = this.alpha_h(1 - h) - this.beta_h(V) * h;
            double dndt = this.alpha_n(1 - n) - this.beta_n(V) * n;
            var res = new double[] { dvdt, dmdt, dhdt, dndt };
            return res;
        }

        public static double I_inj(double t)
        {
            if (5 < t && t < 6)
            {
                return 150;
            }
            else if (15 < t && t < 16)
            {
                return 50.0;
            }
            return 0;
        }

        private double alpha_n(double V)
        {
            return 0.01 * (10.0 - V) / (Math.Exp(1 - (0.1 * V)) - 1.0);
        }

        private double beta_n(double V)
        {
            return 0.125 * Math.Exp(-V / 80.0);
        }

        private double alpha_m(double V)
        {
            return 0.1 * (25.0 - V) / (Math.Exp(2.5 - (0.1 * V)) - 1.0);
        }

        private double beta_m(double V)
        {
            return 4.0 * Math.Exp(-V / 18.0);
        }

        private double alpha_h(double V)
        {
            return 0.07 * Math.Exp( - V / 20.0);
        }

        private double beta_h(double V)
        {
            return 1.0 / (Math.Exp(3.0 - (0.1 * V)) + 1.0);
        }
    }
}
