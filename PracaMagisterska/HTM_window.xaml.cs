using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PracaMagisterska.HTM;
using NumSharp.Core;

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for HTM_window.xaml
    /// </summary>
    public partial class HTM_window : Window
    {
        private PracaMagisterska.HTM.HTM htm;

        public HTM_window()
        {
            InitializeComponent();
            htm = new PracaMagisterska.HTM.HTM();
            initialize_model();
        }

        public int[][] data_generator()
        {
            int[][] data = new int[6][];
            data[0] = new int[] { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            data[1] = new int[] { 0, 0, 0, 0, 1, 0, 1, 0, 1, 1 };
            data[2] = new int[] { 0, 0, 1, 0, 1, 0, 0, 0, 0, 1 };
            data[3] = new int[] { 0, 1, 0, 0, 1, 1, 0, 1, 0, 1 };
            data[4] = new int[] { 1, 0, 1, 0, 1, 0, 1, 0, 0, 1 };
            data[5] = new int[] { 0, 0, 0, 1, 1, 0, 0, 0, 1, 1 };
            return data;
        }

        public void initialize_model()
        {
            int[][] data = new int[6][];
            data[0] = new int[] { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            data[1] = new int[] { 0, 0, 0, 0, 1, 0, 1, 0, 1, 1 };
            data[2] = new int[] { 0, 0, 1, 0, 1, 0, 0, 0, 0, 1 };
            data[3] = new int[] { 0, 1, 0, 0, 1, 1, 0, 1, 0, 1 };
            data[4] = new int[] { 1, 0, 1, 0, 1, 0, 1, 0, 0, 1 };
            data[5] = new int[] { 0, 0, 0, 1, 1, 0, 0, 0, 1, 1 };

            htm.initialize_input(data, 1);
            Tuple<int, int> grid_column_shape = htm.get_columns_grid_size();
            HTM_excite_history excite_history = new HTM_excite_history(htm.layer, htm.cells_per_column, grid_column_shape.Item1, grid_column_shape.Item2);
            this.execute(excite_history, this.data_generator, 20);

        }

        public void execute(HTM_excite_history history, Func<int[][]> data_generator, int iteration_number, bool learning = true)
        {
            // data_generator: generate next input data sample
            // post_generator: function to call after each interation

            while (iteration_number > 0)
            {
                int[][] new_data = data_generator();
                bool data_uptaded = htm.update_data(new_data);
                if (!data_uptaded)
                {
                    Console.WriteLine("Different input size. Execution breaked.");
                    break;
                }

                history.update_history(htm);
                htm.execute();
                iteration_number -= 1;
            }
        }

    }
}
