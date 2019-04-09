using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    class Segment
    {
        private List<Synapse> synapses;
        private bool distal;
        private bool next_step;

        public Segment(bool distal=true, bool next_step=true)
        {
            // next_step: whether this segment indicates predicted firing in the very next time step 
            this.synapses = new List<Synapse>();
            this.distal = distal;
            this.next_step = next_step;
        }

    }
}
