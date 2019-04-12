using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class Segment
    {
        private int synapses_per_segment = HTM_parameters.SYNAPSES_PER_SEGMENT;
        private double fraction_segment_activation_threshold = HTM_parameters.THRESHOLD_SYNAPSES_PER_SEGMENT / 100;

        public List<Synapse> synapses;
        public bool distal;
        public bool next_step;

        public Segment(bool distal=true, bool next_step=false)
        {
            // next_step: whether this segment indicates predicted firing in the very next time step 
            this.synapses = new List<Synapse>();
            this.distal = distal;
            this.next_step = next_step;
        }

        public void add_synapse(Synapse synapse)
        {
            this.synapses.Add(synapse);
        }

        public List<Synapse> old_firing_synapses()
        {
            List<Synapse> firing_synapses = new List<Synapse>();
            foreach (Synapse synapse in this.synapses)
            {
                if (synapse.was_firing())
                    firing_synapses.Add(synapse);
            }
            return firing_synapses;
        }

        public void increase_permanences(double factor)
        {
            foreach (Synapse synapse in this.synapses)
                synapse.increment_permamence();
        }

        public List<Synapse> get_connected_synapses()
        {
            List<Synapse> connected_synapses = new List<Synapse>();
            foreach (Synapse synapse in this.synapses)
            {
                if (synapse.connected())
                    connected_synapses.Add(synapse);
            }
            return connected_synapses;
        }

        public bool was_active()
        {
            // True if segment fire - enough synapses fire to reach the activation threshold
            // synapse is said to be active if the cell it came from was active in the previous step
            int total = this.synapses.Count();
            if (total == 0)
                return false;

            List<Synapse> firing_synapses = new List<Synapse>();
            foreach (Synapse synapse in this.synapses)
            {
                if (synapse.was_firing())
                    firing_synapses.Add(synapse);
            }

            return (firing_synapses.Count() / total) >= fraction_segment_activation_threshold;
        }

        public bool was_active_from_learning_cells()
        {
            int total = this.synapses.Count();
            if (total == 0)
                return false;
            List<Synapse> firing_synapses = new List<Synapse>();
            foreach (Synapse synapse in this.synapses)
            {
                if (synapse.was_firing() && synapse.was_input_learning())
                    firing_synapses.Add(synapse);
            }

            return (firing_synapses.Count() / total) >= fraction_segment_activation_threshold;
        }

    }
}
