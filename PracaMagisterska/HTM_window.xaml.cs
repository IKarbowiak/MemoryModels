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
        private System.Windows.Media.SolidColorBrush inactive_color = System.Windows.Media.Brushes.LightGoldenrodYellow;
        private System.Windows.Media.SolidColorBrush predicting_color = System.Windows.Media.Brushes.Silver;
        private System.Windows.Media.SolidColorBrush active_color = System.Windows.Media.Brushes.Teal;

        private PracaMagisterska.HTM.HTM htm;
        private List<List<Rectangle>> input_rectangle_list;
        private List<List<Rectangle>> reverse_input_rectangle_list;

        public HTM_window()
        {
            InitializeComponent();
            htm = new PracaMagisterska.HTM.HTM();
            initialize_model();

            this.active_legend.Fill = active_color;
            this.inactive_legend.Fill = inactive_color;
            this.predictive_legend.Fill = predicting_color;

            this.input_rectangle_list = new List<List<Rectangle>>();
            input_rectangle_list.Add(this.split_rectangle(input_rec, htm.data.Length * htm.data[0].Length, input_grid));

            this.reverse_input_rectangle_list = new List<List<Rectangle>>();
            reverse_input_rectangle_list.Add(this.split_rectangle(input_rec_reverse, htm.data.Length * htm.data[0].Length, input_grid_reverse));

            List<int> input_data_for_filling = this.prepare_data_for_filling_rectangles(htm.data);
            fill_rectangle_with_data(input_data_for_filling, input_rectangle_list.Last());

            List<int> reverse_input_data_for_filling = this.prepare_data_for_filling_rectangles(htm.data, true);
            fill_rectangle_with_data(reverse_input_data_for_filling, reverse_input_rectangle_list.Last());
        }

        public int[][] data_generator()
        {
            int[][] old_data = this.htm.data;
            int[][] data = new int[old_data.Length][];

            for (int i = 0; i < old_data.Length; i++) 
            {
                int[] row = old_data[i];
                int[] new_row = new int[old_data[0].Length];

                for (int j = 0; j < row.Length; j++)
                {
                    new_row[j] = row[j] == 1 ? 0 : 1;
                }

                data[i] = new_row;
            }

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
            this.execute(excite_history, this.data_generator, 100);

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

                htm.execute();
                history.update_history(htm);
                iteration_number -= 1;
            }

            foreach (List<int> data in history.cell_excite_history)
            {
                foreach (int i in data)
                {
                    if (i == 1)
                        Console.WriteLine("predicting");
                }
            }
        }

        // windwow functions
        public List<Rectangle> split_rectangle(Rectangle rectangle, int rec_number, Grid rec_grid)
        {
            double rec_height = rectangle.Height;
            double rec_width = rectangle.Width;
            double new_rec_width = rec_width / rec_number;

            List<Rectangle> rec_list = new List<Rectangle>();
            for (int i = 0; i < rec_number; i++)
            {
                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength(new_rec_width);
                rec_grid.ColumnDefinitions.Add(c1);

                Rectangle new_rec = new Rectangle()
                {
                    Width = new_rec_width,
                    Height = rec_height,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                };

                rec_list.Add(new_rec);
                rec_grid.Children.Add(new_rec);
                Grid.SetColumn(new_rec, i);
                Grid.SetRow(new_rec, 0);
            }

            return rec_list;

        }

        public List<int> prepare_data_for_filling_rectangles(int[][] data, bool reverse = false)
        {
            List<int> new_data = new List<int>();
            foreach (int[] row in data)
            {
                foreach(int value in row)
                {
                    int new_value = value;
                    if (reverse)
                        new_value = value == 0 ? 1 : 0;
                    new_data.Add(new_value);
                }
            }

            return new_data;
        }

        public void fill_rectangle_with_data(List<int> data, List<Rectangle> rec_list)
        {


            const int INACTIVE = 0;
            const int ACTIVE = 1;
            const int PREDICTING = 2;

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] == INACTIVE)
                    rec_list[i].Fill = inactive_color;
                else if (data[i] == ACTIVE)
                    rec_list[i].Fill = active_color;
                else if (data[i] == PREDICTING)
                    rec_list[i].Fill = predicting_color;
            }

        }

    }
}
