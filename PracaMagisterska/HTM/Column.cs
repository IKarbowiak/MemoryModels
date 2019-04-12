using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class Column
    {
        private int min_overlap = 5;

        public int x;
        public int y;
        public bool active;
        public double overlap;
        private HTM htm;
        public Segment segment { get; set; }
        public Cell[] cells { get; set; }
        public double boost;
        public double min_duty_cycle; // doc numenta: A variable representing the minimum desired firing rate for a cell. If a cell's firing rate falls below this value, it will be boosted.
        public double active_duty_cycle; // doc numenta: A sliding average representing how often column c has been active after inhibition
        public double overlap_duty_cycle; // doc numenta: A sliding average representing how often column c has had significant overlap (i.e. greater than minOverlap) with its inputs
        private const double avg_scale = HTM_parameters.AVG_SCALE;

        public Column(HTM htm_obj, int x, int y, int cells_in_column)
        {
            // column localization
            this.x = x;
            this.y = y;
            this.htm = htm_obj;
            this.active = false;
            this.segment = new Segment(distal: false);
            this.overlap = 0;
            this.boost = 1.0;
            this.min_duty_cycle = 0;
            this.active_duty_cycle = 0;
            this.overlap_duty_cycle = 0;


            this.cells = new Cell[cells_in_column];
            for (int i=0; i < cells_in_column; i++)
            {
                this.cells[i] = new Cell(this, i);
            }
        }

        public double calculate_distance(int inputx, int inputy, double input_compression)
        {
            double x_range = (double)this.x * input_compression;
            double y_range = (double)this.y * input_compression;
            // calculate distance between cell in localization x, y and this column
            return Math.Sqrt( Math.Pow((double)inputx - x_range, 2) + Math.Pow((double)inputy - y_range, 2));
        }

        public double old_firing_synapses()
        {
            return this.segment.old_firing_synapses().Count();
        }

        // Numenta docs: Given the list of columns, return the k'th highest overlap value.
        public double kth_score(int k)
        {
            List <Column> neighbors = htm.neighbors(this);
            neighbors.OrderByDescending(col => col.overlap);
            int kth_element_index = Math.Min(k - 1, neighbors.Count() - 1);
            return neighbors[kth_element_index].overlap;
        }

        public List<Synapse> get_synapses()
        {
            return this.segment.synapses;
        }

        public double neighbor_max_duty_cycle()
        {
            // doc Numenta: Returns the maximum active duty cycle of the columns in the given list of columns.
            List<Column> neighbors = htm.neighbors(this);
            double max_duty_cycle = 0;
            foreach (Column column in neighbors)
            {
                max_duty_cycle = Math.Max(max_duty_cycle, column.active_duty_cycle);
            }

            return max_duty_cycle;
        }

        public double update_active_duty_cycle()
        {
            // doc Numenta: Computes a moving average of how often column c has been active after inhibition.
            // using an exponential average, so AS*old+(1-AS*current) => new
            double new_duty_cycle = avg_scale * this.active_duty_cycle;
            if (this.active)
                new_duty_cycle += (1 - avg_scale);
            return new_duty_cycle;
        }

        public double boost_function()
        {
            // doc Numenta: Returns the boost value of a column. The boost value is a scalar >= 1. If activeDutyCyle(c) is above minDutyCycle(c), the boost value is 1. 
            // The boost increases linearly once the column's activeDutyCyle starts falling below its minDutyCycle.
            if (this.active_duty_cycle >= this.min_duty_cycle)
                return 1.0;
            else if (this.active_duty_cycle == 0)
                return this.boost * 1.05;
            else
                return this.min_duty_cycle / this.active_duty_cycle;
        }

        public double update_overlap_duty_cycle()
        {
            double new_duty_cycle = avg_scale * this.overlap_duty_cycle;
            if (this.overlap > this.min_overlap)
            {
                new_duty_cycle += (1 - avg_scale);
            }
            return new_duty_cycle;
        }

        public void increase_permanences(double factor)
        {
            this.segment.increase_permanences(factor);
        }

        public List<Synapse> get_connected_synapses()
        {
            return this.segment.get_connected_synapses();
        }

        public Tuple<Cell, Segment> get_best_matching_cell(bool next_step = true)
        {
            // doc Numenta: find the segment with the largest number of active synapses.
            // If no segments are found, then an index of -1 is returned
            Cell best_cell = null;
            int best_cell_firing_synapse_count = 0;
            Segment best_segment = null;

            foreach (Cell cell in this.cells)
            {
                Segment segment = cell.get_best_matching_segment(next_step);
                int synapses_count = segment != null ? segment.old_firing_synapses(connection_required: false).Count() : 0;
                if (synapses_count > best_cell_firing_synapse_count)
                {
                    best_cell_firing_synapse_count = synapses_count; ;
                    best_segment = segment;
                    best_cell = cell;
                }
            }

            // if best_cell is none, choose cell with the fewest segments
            if (best_cell == null)
            {
                int fewest_segment = this.cells[0].segments.Count();
                best_cell = this.cells[0];
                foreach (Cell cell in this.cells)
                {
                    if (best_cell == cell)
                        continue;
                    if (cell.segments.Count() < fewest_segment)
                    {
                        best_cell = cell;
                        fewest_segment = cell.segments.Count();
                    }
                }
                best_segment = best_cell.get_best_matching_segment(next_step);
            }

            return Tuple.Create(best_cell, best_segment);
        }
    }
}
