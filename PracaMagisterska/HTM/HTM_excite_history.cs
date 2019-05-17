using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class HTM_excite_history
    {
        private const int INACTIVE = 0;
        private const int ACTIVE = 1;
        private const int PREDICTING = 2;
        private const int ACTIVE_AND_PREDICTING = 3;
        private const int DEMAGE = 4;

        private int layer;
        private int cells_per_column;
        private int column_width;
        private int column_length;
        //public List<List<int>> column_excite_history;
        public List<List<List<int>>> cell_excite_history;
        public List<List<int>> active_cell_history;
        public List<List<int>> predictive_cell_history;
        public List<List<int>> predictive_and_active_cell_history;
        public Dictionary<int, List<int>> column_excite_history;


        public HTM_excite_history(int layer, int cells_in_column, int column_width, int column_length)
        {
            this.layer = layer;
            this.cells_per_column = cells_in_column;
            this.column_width = column_width;
            this.column_length = column_length;
            this.column_excite_history = new Dictionary<int, List<int>>();
            this.cell_excite_history = new List<List<List<int>>>();
            this.active_cell_history = new List<List<int>>();
            this.predictive_cell_history = new List<List<int>>();
            this.predictive_and_active_cell_history = new List<List<int>>();
        }


        public void update_history(HTM htm, int input_value)
        {
            List<int> active_cell_history_time_slice = new List<int>();
            List<int> predictive_cell_history_time_slice = new List<int>();
            List<int> predictive_and_active_cell_history_time_slice = new List<int>();

            int cell_counter = 0;
            foreach (Column column in htm.get_columns())
            {
                foreach (Cell cell in column.get_cells(false))
                {
                    if (cell.active && cell.predicting)
                        predictive_and_active_cell_history_time_slice.Add(cell_counter);
                    if (cell.predicting)
                        predictive_cell_history_time_slice.Add(cell_counter);
                    if (cell.active)
                        active_cell_history_time_slice.Add(cell_counter);
                    cell_counter++;
                }
            }


            active_cell_history.Add(active_cell_history_time_slice);
            predictive_cell_history.Add(predictive_cell_history_time_slice);
            predictive_and_active_cell_history.Add(predictive_and_active_cell_history_time_slice);

            List<List<int>> cell_exicte_time_slice = new List<List<int>>();
            
            for (int i = 0; i < HTM_parameters.CELLS_PER_COLUMN; i++)
            {
                cell_exicte_time_slice.Add(new List<int>());
            }

            foreach (Column column in htm.get_columns())
            {
                int cell_num = 0;
                foreach (Cell cell in column.get_cells(false))
                {
                    int state = INACTIVE;
                    if (cell.damage)
                        state = DEMAGE;
                    else if (cell.active && cell.predicting)
                        state = ACTIVE_AND_PREDICTING;
                    else if (cell.predicting)
                        state = PREDICTING;
                    else if (cell.active)
                        state = ACTIVE;

                    cell_exicte_time_slice[cell_num].Add(state);
                    cell_num++;
                }

            }


            List<int> column_exicte_time_slice = new List<int>();
            int column_counter = 0;

            foreach (Column column in htm.get_columns())
            {
                if (column.active)
                    column_exicte_time_slice.Add(column_counter);
                column_counter++;
            }

            this.column_excite_history[input_value] = column_exicte_time_slice;
            this.cell_excite_history.Add(cell_exicte_time_slice);

        }

        public int find_similarities(int input_value)
        {
            List<int> active_columns = this.column_excite_history[input_value];
            List<int> matching_input_list = new List<int>();
            int best_matching_input = 0;
            int differs_columns_num = active_columns.Count() + 1;
            foreach (KeyValuePair<int, List<int>> key_value in this.column_excite_history)
            {
                if (key_value.Key == input_value)
                    continue;
                List<int> list_difference = active_columns.Except(key_value.Value).ToList();
                if (list_difference.Count < differs_columns_num)
                {
                    differs_columns_num = list_difference.Count();
                    best_matching_input = key_value.Key;
                    matching_input_list = new List<int> { best_matching_input};
                }
                else if (list_difference.Count == differs_columns_num)
                {
                    matching_input_list.Add(key_value.Key);
                }
                 
            }
            if (matching_input_list.Count > 1)
            {
                List<int> sorted_input = new List<int>() ;
                sorted_input = matching_input_list.FindAll(x => x < input_value).ToList();
                if (sorted_input.Count > 0)
                    best_matching_input = sorted_input.Max();
                else
                    best_matching_input = matching_input_list.Min();
            }
            return best_matching_input;
        }

    }
}
