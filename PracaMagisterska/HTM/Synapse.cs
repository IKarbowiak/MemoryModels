using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class Synapse
    {
        private const double connected_permanence = HTM_parameters.CONNECTED_PERMANENCE ;      // if permanence for a synapse is above this value, it is said to be connected
        private const double permanence_increment = HTM_parameters.PERMANENCE_INCREMENT;
        private const double permanence_decrement = HTM_parameters.PERMANENCE_DECREMENT;

        private double permanence;
        public InputCell input_cell;

        public Synapse(InputCell input_cell, double permanence=(connected_permanence-0.001))
        {
            this.permanence = permanence;
            this.input_cell = input_cell;
        }

        public bool was_firing()
        {
            return this.input_cell.was_active() && this.connected();
        }

        public bool is_firing()
        {
            return this.input_cell.is_active() && this.connected();
        }

        public bool connected()
        {
            return this.permanence >= connected_permanence;
        }

        public void increment_permamence(double factor = permanence_increment)
        {
            this.permanence = Math.Min(1.0, this.permanence + factor);
        }

        public void decrement_permamence()
        {
            this.permanence = Math.Max(0.0, this.permanence - permanence_decrement);
        }

        public bool was_input_learning()
        {
            return this.input_cell.learning;
        }
    }
}
