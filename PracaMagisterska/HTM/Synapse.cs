using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    class Synapse
    {
        private const double connected_permanence = HTM_parameters.CONNECTED_PERMANENCE ;      // if permanence for a synapse is above this value, it is said to be connected
        private const double permanence_increment = HTM_parameters.PERMANENCE_INCREMENT;
        private const double permanence_decrement = HTM_parameters.PERMANENCE_DECREMENT;

        private double permanence;
        private InputCell input_cell;

        public Synapse(InputCell input_cell, double permanence=(connected_permanence-0.001))
        {
            this.permanence = permanence;
            this.input_cell = input_cell;
        }
    }
}
