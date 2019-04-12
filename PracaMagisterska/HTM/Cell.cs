using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class Cell
    {
        private int segments_per_cell = 5;
        private double synapse_activation_threshold = 1; // thershold for a segment, segment is said to be active if the number of connected active synapses is above this value

        private Column column;
        private int layer;
        public bool active = false;
        private bool was_active = false;
        public bool predicting = false;
        public bool was_predicted = false;
        public bool learning = false;
        private bool was_learning = false;
        private bool predicting_next = false;
        private bool was_predicted_next = false;
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

        public void clock_tick()
        {
            this.was_active = this.active;
            this.was_learning = this.learning;
            this.was_predicted = this.predicting;
            this.was_predicted_next = this.predicting_next;

            this.active = false;
            this.learning = false;
            this.predicting = false;
            this.predicting_next = false;
        }

        public Segment get_active_segment()
        {
            List<Segment> segments = this.near_segments();
            foreach (Segment segment in segments)
            {
                if (segment.was_active())
                    return segment;
            }
            return null;


        }

        public List<Segment> near_segments()
        {
            List<Segment> segments_list = new List<Segment>();
            foreach (Segment segment in this.segments)
            {
                if (segment.next_step)
                {
                    segments_list.Add(segment);
                }
            }

            return segments_list;
        }
    }
}
