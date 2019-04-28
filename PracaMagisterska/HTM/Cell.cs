using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class Cell
    {
        private const int segments_per_cell = HTM_parameters.SEGMENTS_PER_CELL;
        private Column column;
        private int layer;
        public bool active = false;
        public bool was_active = false;
        public bool predicting = false;
        public bool was_predicted = false;
        public bool learning = false;
        public bool was_learning = false;
        private bool predicting_next = false;
        private bool was_predicted_next = false;
        public bool demage = false;
        public List<Segment> segments;

        public Cell(Column col, int layer)
        {
            this.column = col;
            this.layer = layer;
            this.segments = new List<Segment>();
            for (int i=0; i < segments_per_cell; i++)
            {
                segments.Add(new Segment());
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
            //this.demage = false;
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

        public Segment get_best_matching_segment(bool next_step)
        {
            // doc Numenta: This routine is aggressive in finding the best match. The permanence value of synapses is allowed to be below connectedPerm. 
            // The number of active synapses is allowed to be below activationThreshold, but must be above minThreshold. 
            // The routine returns the segment index. If no segments are found, then an index of -1 is returned.

            // next step: true if segment should be next step type, otherwise all-time prediiction

            Segment best_segment = null;
            int best_segment_synapse_count = HTM_parameters.MIN_SYNAPSES_PER_SEGMENT_THRESHOLD - 1;

            List<Segment> segments_to_consider = new List<Segment>();
            foreach (Segment segment in this.segments)
            {
                if (segment.next_step == next_step)
                    segments_to_consider.Add(segment);
            }

            foreach (Segment segment in segments_to_consider)
            {
                int synapse_count = segment.old_firing_synapses(false).Count();
                if (synapse_count > best_segment_synapse_count)
                {
                    best_segment = segment;
                    best_segment_synapse_count = synapse_count;
                }
            }

            return best_segment;

        }

        public Segment create_segment(HTM htm, bool next_step)
        {
            const int synapses_per_segment = HTM_parameters.SYNAPSES_PER_SEGMENT;
            Segment segment = new Segment(next_step: next_step);
            int synapse_len = this.create_synapses(segment, htm.get_cells(), synapses_per_segment, true, false);

            if (!next_step && synapse_len < synapses_per_segment)
            {
                int number_of_additnional_synapses = synapses_per_segment - synapse_len;
                this.create_synapses(segment, htm.get_cells(), number_of_additnional_synapses, false, true);
            }

            this.segments.Add(segment);

            return segment;

        }

        public int create_synapses(Segment segment, IEnumerable<Cell> cells, int max_synapses_amount, bool check_was_learning, bool check_was_active)
        {
            List<Cell> matching_cells = new List<Cell>();
            foreach (Cell cell in cells)
            {
                if (this != cell && check_was_learning && cell.was_learning)
                {
                    matching_cells.Add(cell);
                }
                else if (this != cell && check_was_active && cell.was_active)
                {
                    matching_cells.Add(cell);
                }
                else if (this != cell)
                {
                    matching_cells.Add(cell);
                }
            }

            Random rnd = new Random();
            int sample_size = Math.Min(matching_cells.Count(), max_synapses_amount);
            int counter = 0;
            foreach (Cell cell in matching_cells.OrderBy(x => rnd.Next()).Take(sample_size))
            {
                segment.create_synapse(cell);
                counter++;
            }

            return counter;
        }
    }
}
