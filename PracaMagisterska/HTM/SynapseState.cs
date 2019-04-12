using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class SynapseState
    {
        public Synapse synapse;
        public Segment segment;
        public bool input_was_active;

        public SynapseState(Synapse syn, Segment seg, bool was_active)
        {
            this.synapse = syn;
            this.segment = seg;
            this.input_was_active = was_active;
        }

        public void capture_segment_state()
        {

        }
    }
}
