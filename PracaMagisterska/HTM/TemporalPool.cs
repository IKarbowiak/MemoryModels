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

        public void perform()
        {
            this.phase1();
            this.phase2();
            this.phase3();
        }

        public void phase1()
        {
            foreach (Column column in this.htm.get_active_columns())
            {
                bool bu_predicted = false;
                bool is_cell_chosen = false;
                foreach (Cell cell in column.cells)
                {
                    if (cell.was_predicted)
                    {
                        Segment segment = cell.get_active_segment();
                        if (segment != null && segment.distal)
                        {
                            bu_predicted = true;
                            cell.active = true;

                            // TODO: fix it => always false !!!
                            if (this.learning && segment.was_active_from_learning_cells())
                            {
                                is_cell_chosen = true;
                                cell.learning = true;
                            }

                        }
                    }
                }

                if (bu_predicted)
                {
                    foreach (Cell cell in column.cells)
                        cell.active = true;
                }

                // learning part of phase 1

            }
        }

        public void phase2()
        {

        }

        public void phase3()
        {

        }

    }
}
