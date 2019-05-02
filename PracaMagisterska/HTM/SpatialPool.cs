using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class SpatialPool
    {
        private const int desired_local_activity = HTM_parameters.DESIRED_LOCAL_ACTIVITY;

        public SpatialPool()
        {

        }

        public void perform(PracaMagisterska.HTM.HTM htm)
        {
            this.overlap(htm);
            List<Column> active_columns = this.inhibition(htm);
            //htm.inhibition_radius = (int)this.learning(active_columns, htm);
        }

        private void overlap(HTM htm)
        {
            foreach (Column col in htm.get_columns())
            {
                col.overlap = col.old_firing_synapses();

                if (col.overlap < HTM_parameters.MIN_OVERLAP)
                    col.overlap = 0;
                else
                    col.overlap *= col.boost;
            }
        }

        private List<Column> inhibition(HTM htm)
        {
            List<Column> active_columns = new List<Column>();
            foreach (Column column in htm.get_columns())
            {
                double minimum_local_activity = column.kth_score(desired_local_activity);

                if (column.overlap > 0 && column.overlap >= minimum_local_activity)
                {
                    active_columns.Add(column);
                    column.active = true;
                }
                else
                    column.active = false;
            }

            return active_columns;
        }

        private double learning(List<Column> active_columns, HTM htm)
        {
            foreach (Column col in active_columns)
            {
                foreach (Synapse synapse in col.get_synapses())
                {
                    if (synapse.was_firing())
                        synapse.increment_permamence();
                    else
                        synapse.decrement_permamence();
                }
            }

            // boost columns which does not win often
            // help column learn connections
            foreach (Column col in htm.get_columns())
            {
                col.min_duty_cycle = 0.01 * col.neighbor_max_duty_cycle();
                col.active_duty_cycle = col.update_active_duty_cycle();
                col.boost = col.boost_function();

                col.overlap_duty_cycle = col.update_overlap_duty_cycle();
                if (col.overlap_duty_cycle < col.min_duty_cycle)
                    col.increase_permanences(0.1 * HTM_parameters.CONNECTED_PERMANENCE);
            }

            return htm.average_receptive_field_size();
        }

 
    }
}
