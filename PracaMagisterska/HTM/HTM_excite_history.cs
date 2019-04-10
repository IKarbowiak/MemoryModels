using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    class HTM_excite_history
    {
        private const int INACTIVE = 0;
        private const int PREDICTING = 1;
        private const int ACTIVE = 2;


        private int layer;
        private int cells_per_column;
        private int column_width;
        private int column_length;
        private List<List<int>> cell_excite_history;
        private List<List<int>> column_excite_history;

        public HTM_excite_history(int layer, int cells_in_column, int column_width, int column_length)
        {
            this.layer = layer;
            this.cells_per_column = cells_in_column;
            this.column_width = column_width;
            this.column_length = column_length;
            this.cell_excite_history = new List<List<int>>();
            this.column_excite_history = new List<List<int>>();
        }

        public void update_history(HTM htm)
        {
            List<int> cell_exicte_time_slice = new List<int>();
            
            foreach (Cell cell in htm.get_cells())
            {
                int state = INACTIVE;
                if (cell.active)
                    state = ACTIVE;
                else if (cell.predicting)
                    state = PREDICTING;
                cell_exicte_time_slice.Add(state);
            }

            List<int> column_exicte_time_slice = new List<int>();

            foreach (Column column in htm.get_columns())
            {
                int state = column.active ? ACTIVE : INACTIVE;
                column_exicte_time_slice.Add(state);
            }

            this.cell_excite_history.Add(cell_exicte_time_slice);
            this.column_excite_history.Add(column_exicte_time_slice);
            
        }

    }
}
