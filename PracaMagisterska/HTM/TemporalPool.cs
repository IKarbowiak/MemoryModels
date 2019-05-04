using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class TemporalPool
    {
        private PracaMagisterska.HTM.HTM htm;
        private bool learning;
        private UpdateSegments update_segments;

        public TemporalPool(PracaMagisterska.HTM.HTM htm, bool learning, UpdateSegments update_segments)
        {
            this.htm = htm;
            this.learning = learning;
            this.update_segments = update_segments; // hash from cell to a list of segments to update when cell becomes active
        }

        public UpdateSegments perform()
        {
            this.phase1();
            this.phase2();
            if (learning)
                this.phase3();

            return this.update_segments;
        }

        public void phase1()
        {
            foreach (Column column in this.htm.get_active_columns())
            {
                bool bu_predicted = false;
                bool is_cell_chosen = false;
                foreach (Cell cell in column.get_cells())
                {
                    if (cell.was_predicted && !cell.damage)
                    {
                        Segment segment = cell.get_active_segment();
                        if (segment != null && segment.distal)
                        {
                            bu_predicted = true;
                            cell.active = true;

                            if (this.learning && segment.was_active_from_learning_cells())
                            {
                                is_cell_chosen = true;
                                cell.learning = true;
                                this.update_segments.add(cell, segment, 0); // TODO: Check this. I added this
                            }

                            break;
                        }
                    }
                }

                // active all cells if any of them was predicted
                if (!bu_predicted)
                {
                    foreach (Cell cell in column.get_cells())
                        cell.active = true;
                }

                // learning part of phase 1
                if (learning && !is_cell_chosen)
                {
                    Tuple<Cell, Segment> best_cell_and_segment = column.get_best_matching_cell(next_step: true);
                    Cell cell = best_cell_and_segment.Item1;
                    Segment segment = best_cell_and_segment.Item2;
                    cell.learning = true;

                    if (segment == null)
                        segment = cell.create_segment(htm: htm, next_step: true);

                    this.update_segments.add(cell, segment, -1);

                }
            }
        }

        public void phase2()
        {
            // doc numenta: A cell will turn on its predictive state output if one of its segments becomes active
            foreach (Cell cell in htm.get_cells())
            {
                foreach (Segment segment in cell.segments.ToList())
                {
                    if (segment.is_active())
                    {
                        cell.predicting = true;
                        if (learning)
                        {
                            this.update_segments.add(cell, segment, 0);
                            Segment pred_segment = cell.get_best_matching_segment(false);
                            if (pred_segment == null)
                                pred_segment = cell.create_segment(htm, false);

                            pred_segment.adjust_synapses_amount(this.htm);
                            this.update_segments.add(cell, pred_segment, -1);

                        }
                    }
                }
            }

        }

        public void phase3()
        {
            foreach (Cell cell in htm.get_cells())
            {
                if (cell.learning)
                {
                    foreach (List<SynapseState> states in this.update_segments.get_cell_synapse_states(cell))
                    {
                        this.adapt_true(states);
                    }
                    if (!cell.predicting)
                        this.update_segments.reset(cell);
                }

                else if (!cell.predicting && cell.was_predicted)
                {
                    foreach (List<SynapseState> states in this.update_segments.get_cell_synapse_states(cell))
                    {
                        this.adapt_false(states);
                    }
                    this.update_segments.reset(cell);
                }
            }
        }

        public void adapt_true(List<SynapseState> synapse_states)
        {
            // doc numenta: synapses on the active list get their permanence counts incremented by permanenceInc. 
            // All other synapses get their permanence counts decremented by permanenceDec
            foreach (SynapseState state in synapse_states)
            {
                if (state.input_was_active)
                    state.synapse.increment_permamence();
                else
                    state.synapse.decrement_permamence();
            }
        }

        public void adapt_false(List<SynapseState> synapse_states)
        {
            // doc Numenta: synapses on the active list get their permanence counts decremented by permanenceDec
            foreach (SynapseState state in synapse_states)
            {
                if (state.input_was_active)
                    state.synapse.decrement_permamence();
            }
        }

    }
}
