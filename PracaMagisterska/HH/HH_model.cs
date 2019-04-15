using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenterSpace.NMath.Analysis;
using CenterSpace.NMath.Core;
using System.Windows.Forms;
using OxyPlot;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Threading;

namespace PracaMagisterska
{
    class HH_model
    {
        private HH_neuron hh_neuron;
        public IList<DataPoint> Points { get; private set; }
        public string Title { get; private set; }
        public RelayCommand start { get; set; }

        public HH_model()
        {
            start = new RelayCommand(start_action);

            //foreach (DataPoint point in this.Points)
            //{
            //    Console.WriteLine(point);
            //}

        }



        public void start_action()
        {
            hh_neuron = new HH_neuron();
            this.Solution();
            Console.WriteLine("Finish");
        }

        private void Solution()
        {
            var solver = new RungeKutta45OdeSolver();
            var timeSpan = new DoubleVector(0.0, 50.0);
            var initial_params = new DoubleVector(-65, 0.05, 0.6, 0.32);
            this.Points = new List<DataPoint> { new DataPoint( 0.0, -65) };

            // Construct the delegate representing our system of differential equations...
            var odeFunction = new Func<double, DoubleVector, DoubleVector>(this.Rigid);

            RungeKutta45OdeSolver.Options solverOptions = new RungeKutta45OdeSolver.Options
            {
                Refine = 1
            };

            RungeKutta45OdeSolver.Solution<DoubleMatrix> soln = solver.Solve(odeFunction, timeSpan, initial_params, solverOptions);
            //var V_elements = soln.Y.Col(1);
            //var T_elements = soln.T;
            //Console.WriteLine(T_elements);
            //Console.WriteLine(V_elements);
            //foreach (int index in Enumerable.Range(0, V_elements.Count()))
            //{
            //    this.Points.Add(new DataPoint(T_elements[index], V_elements[index]));
            //};
           
            //Console.WriteLine("T = " + soln.T.ToString("G5"));
            //Console.WriteLine("Y = ");
            //Console.WriteLine(soln.Y.ToTabDelimited("G5"));


            this.Title = "Test";

        }

        public DoubleVector Rigid(double t, DoubleVector y)
        {
            double V = y[0];
            double m = y[1];
            double h = y[2];
            double n = y[3];

            //Console.Write(t);
            //Console.Write('\t');
            //Console.Write(V);
            //Console.WriteLine();
            //this.Points.Add(new DataPoint(t, V));

            double dvdt = (I_inj(t) - this.hh_neuron.I_Na(V, m, h) - this.hh_neuron.I_K(V, n) - this.hh_neuron.I_Cl(V)) / this.hh_neuron.cm;
            double dmdt = this.alpha_m(1 - m) - this.beta_m(V) * m;
            double dhdt = this.alpha_h(1 - h) - this.beta_h(V) * h;
            double dndt = this.alpha_n(1 - n) - this.beta_n(V) * n;
            this.Points.Add(new DataPoint(t, V));
            var res = new DoubleVector(dvdt, dmdt, dhdt, dndt);
            return res;
        }

        public static double I_inj(double t)
        {
            if (10 < t && t < 200)
            {
                return 10;
            }
            return 0;
        }

        private double alpha_n(double V)
        {
            return 0.01 * (V + 55) / (1.0 - Math.Exp(-(V + 55) / 10) );
        }

        private double beta_n(double V)
        {
            return 0.125 * Math.Exp(-(V + 65) / 80);
        }

        private double alpha_m(double V)
        {
            return 0.1 * (V + 40) / (1 - Math.Exp(-(V + 40) / 10));
        }

        private double beta_m(double V)
        {
            return 4 * Math.Exp(-(V + 65) / 18);
        }

        private double alpha_h(double V)
        {
            return 0.07 * Math.Exp( - (V + 65) / 20);
        }

        private double beta_h(double V)
        {
            return 1 / (Math.Exp(-(V + 35) / 10) + 1);
        }
    }
}
