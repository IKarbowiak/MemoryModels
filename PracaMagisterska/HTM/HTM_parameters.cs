using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public static class HTM_parameters
    {
        // segments parameters
        public const int SYNAPSES_PER_SEGMENT = 45;
        public const int SEGMENTS_PER_CELL = 5;
        public const int MIN_SYNAPSES_PER_SEGMENT_THRESHOLD = 1; // how much synapses must fire to be even consider as for learning

        // synapses parameters
        public const int SYNAPSE_ACTIVATION_THRESHOLD = 1;
        public const double CONNECTED_PERMANENCE = 0.2;
        public const double PERMANENCE_INCREMENT = 0.04;
        public const double PERMANENCE_DECREMENT = 0.04;
        public const int MAX_NEW_SYNAPSES = 3;

        public const int INPUT_BIAS_PEAK = 2;
        public const double INPUT_BIAS_STD_DEV = 0.25;

        public const int CELLS_PER_COLUMN = 4;
        public const int INHIBITION_RADIUS = 4;
        public const int DESIRED_LOCAL_ACTIVITY = 3;
        
        public const int MIN_OVERLAP = 5;
        public const double AVG_SCALE = 0.995;
        public const int THRESHOLD_SYNAPSES_PER_SEGMENT = 20;
  
    }
}
