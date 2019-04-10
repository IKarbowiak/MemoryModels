using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public static class HTM_parameters
    {
        public const int SYNAPSES_PER_SEGMENT = 45;
        public const double CONNECTED_PERMANENCE = 0.2;
        public const double PERMANENCE_INCREMENT = 0.04;
        public const double PERMANENCE_DECREMENT = 0.04;
        public const int INPUT_BIAS_PEAK = 2;
        public const double INPUT_BIAS_STD_DEV = 0.25;
        public const int CELLS_PER_COLUMN = 4;
    }
}
