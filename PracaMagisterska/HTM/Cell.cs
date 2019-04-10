using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    class Cell
    {
        private int segments_per_cell = 5;
        private double synapse_activation_threshold = 1; // thershold for a segment, segment is said to be active if the number of connected active synapses is above this value

        private Column column;
        private int layer;
        public bool active = false;
        private bool was_active = false;
        public bool predicting = false;
        private bool was_predicted = false;
        private bool learning = false;
        private bool was_learning = false;
        private bool predicting_next = false;
        private bool predicted_next = false;
        private Segment[] segments;

        public Cell(Column col, int layer)
        {
            this.column = col;
            this.layer = layer;
            this.segments = new Segment[this.segments_per_cell];
            for (int i=0; i < this.segments_per_cell; i++)
            {
                segments[i] = new Segment();
            }
        }
    }
}
