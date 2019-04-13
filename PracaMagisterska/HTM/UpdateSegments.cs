using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class UpdateSegments
    {
        public Dictionary<Cell, List<List<SynapseState>>> cell_with_states;

        public UpdateSegments()
        {
            cell_with_states = new Dictionary<Cell, List<List<SynapseState>>>();
        }

        public void add(Cell cell, Segment segment, int time_delta = 0)
        {
            // time_delta: 0 mean current state, -1 mean previous state

            List<SynapseState> states = new List<SynapseState>();
            foreach (Synapse synapse in segment.synapses)
            {
                bool synapse_was_active = synapse.firing_at(time_delta, false);
                SynapseState synapse_state = new SynapseState(synapse, segment, synapse_was_active);
                states.Add(synapse_state);
            }

            if (cell_with_states.ContainsKey(cell))
                cell_with_states[cell].Add(states);
            else
                cell_with_states.Add(cell,  new List<List<SynapseState>>() { states });
        }

        public void reset(Cell cell)
        {
            cell_with_states.Remove(cell);
        }

        public List<List<SynapseState>> get_cell_synapse_states(Cell cell)
        {
            return cell_with_states[cell];
        }

    }
}
