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
        private double fraction_segment_activation_threshold = (double)HTM_parameters.THRESHOLD_SYNAPSES_PER_SEGMENT / 100;

        public List<Synapse> synapses;
        public bool distal;
        public bool next_step;
        //public bool was_active;

        public Segment(bool distal=true, bool next_step=false)
        {
            // next_step: whether this segment indicates predicted firing in the very next time step 
            this.synapses = new List<Synapse>();
            this.distal = distal;
            this.next_step = next_step;
            //this.was_active = false;
        }

        //public void clock_tick()
        //{
        //    was_active = this.is_active();
        //    foreach (Synapse synapse in this.synapses)
        //        synapse.clock_tick();
        //}

        public void add_synapse(Synapse synapse)
        {
            this.synapses.Add(synapse);
        }

        public List<Synapse> old_firing_synapses(bool connection_required = true)
        {
            List<Synapse> firing_synapses = new List<Synapse>();
            foreach (Synapse synapse in this.synapses)
            {
                if (synapse.was_firing(connection_required))
                    firing_synapses.Add(synapse);
            }
            return firing_synapses;
        }

        public void increase_permanences(double factor)
        {
            foreach (Synapse synapse in this.synapses)
                synapse.increment_permamence(factor);
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

        public void create_synapse(Cell input)
        {
            // input: coming data or the previous cell in network
            this.add_synapse(new Synapse(cell: input));
        }

        public bool is_active()
        {
            int total = this.synapses.Count();
            if (total == 0)
                return false;
            List<Synapse> firing_synapses = new List<Synapse>();
            foreach (Synapse synapse in this.synapses)
            {
                if (synapse.is_firing(true))
                    firing_synapses.Add(synapse);
            }

            return ((double)firing_synapses.Count() / (double)total) >= fraction_segment_activation_threshold;
        }

        public void adjust_synapses_amount(HTM htm)
        {
            // add synapses if not enough is active
            List<Synapse> synapses = this.old_firing_synapses(false);
            int amount_of_missing_synapses = HTM_parameters.MAX_NEW_SYNAPSES - synapses.Count();

            if (amount_of_missing_synapses > 0)
            {
                List<Cell> last_learning_cells = new List<Cell>();
                foreach (Cell cell in htm.get_cells())
                {
                    if (cell.was_learning)
                        last_learning_cells.Add(cell);
                }
                if (last_learning_cells.Count() > 0)
                {
                    Random rnd = new Random();
                    for (int i = 0; i < amount_of_missing_synapses; i++)
                    {
                        int index = rnd.Next(last_learning_cells.Count() - 1);
                        Cell chosen_cell = last_learning_cells[index];
                        this.create_synapse(chosen_cell);
                    }
                }
            }
        }

    }
}
