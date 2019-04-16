using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska
{
    class HH_neuron
    {
        // conductances
        private double g_Na = 120;          // S/m^2
        private double g_K = 36;            // S/m^2 
        private double g_Cl = 0.3;              // S/m^2
        // reversal potenntial
        private double V_Na = 115;          // V
        private double V_K = -12;         //V
        private double V_Cl = 10.613;       //V
        // capacitance
        public double cm = 1;           // F/m^2
        // radius
        private double rad_Na = 0.102 * Math.Pow(10, -9);      // m
        private double rad_K = 0.138 * Math.Pow(10, -9);      // m
        private double rad_Cl = 0.181 * Math.Pow(10, -9);      // m

        private double radius;              // m
        private double length;              // m
        private double Ra;                  // om/m^2
        private double delta_x;             // m

        public HH_neuron(double r = 0.000238, double len = 1, int discretization = 10000)
        {
            this.radius = r;
            this.length = len;
            this.Ra = 0.354 / Math.PI *  Math.Pow(this.radius, 2);
            this.delta_x = len / discretization;
        }

        public double count_applied_current(double Is)
        {
            return Is / (2 * Math.PI * this.radius * this.delta_x);
        }

        public double I_Na(double V, double m, double h)
        {
            return g_Na * Math.Pow(m, 3) * h * (V - V_Na);
        }

        public double I_K(double V, double n)
        {
            return g_K * Math.Pow(n, 4) * (V - V_K);
        }

        public double I_Cl(double V)
        {
            return g_Cl * (V - V_Cl);
        }
    }
}
