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
        private System.Windows.Media.SolidColorBrush predicting_color = System.Windows.Media.Brushes.Turquoise;
        private System.Windows.Media.SolidColorBrush active_color = System.Windows.Media.Brushes.Teal;

        private PracaMagisterska.HTM.HTM htm;
        private List<List<Rectangle>> input_rectangle_list;
        private List<List<Rectangle>> reverse_input_rectangle_list;
        private List<List<Rectangle>> htm_results_list;

        private HTM_excite_history excite_history;

        public HTM_window()
        {
            InitializeComponent();
            htm = new PracaMagisterska.HTM.HTM();
            initialize_model();

            int width = (int)input_grid.Width;
            int height = (int)input_grid.Height;

            this.active_legend.Fill = active_color;
            this.inactive_legend.Fill = inactive_color;
            this.predictive_legend.Fill = predicting_color;

            this.htm_results_list = new List<List<Rectangle>>();

            int total_data_length = htm.data.Length * htm.data[0].Length;

            this.input_rectangle_list = new List<List<Rectangle>>();
            input_rectangle_list.Add(this.split_rectangle(width, height, total_data_length, input_grid));

            this.reverse_input_rectangle_list = new List<List<Rectangle>>();
            reverse_input_rectangle_list.Add(this.split_rectangle(width, height, total_data_length, input_grid_reverse));

            List<int> input_data_for_filling = this.prepare_data_for_filling_rectangles(htm.data);
            fill_rectangle_with_data(input_data_for_filling, input_rectangle_list.Last());

            List<int> reverse_input_data_for_filling = this.prepare_data_for_filling_rectangles(htm.data, true);
            fill_rectangle_with_data(reverse_input_data_for_filling, reverse_input_rectangle_list.Last());

        }

        public void start_button_click(object sender, RoutedEventArgs e)
        {
            htm = new PracaMagisterska.HTM.HTM();
            initialize_model();
            Tuple<int, int> grid_column_shape = htm.get_columns_grid_size();
            excite_history = new HTM_excite_history(htm.layer, htm.cells_per_column, grid_column_shape.Item1, grid_column_shape.Item2);

            bool is_field_correct = this.validate_field(iteration_textbox);
            if (!is_field_correct)
            {
                iteration_textbox.BorderBrush = Brushes.Red;
                return;
            }
            int iteration_number = Int32.Parse(iteration_textbox.Text);
            if (iteration_number > 200 || iteration_number < 5)
            {
                iteration_textbox.BorderBrush = Brushes.Red;
                return;
            }
            this.execute(excite_history, this.data_generator, iteration_number);
            show_results(excite_history.cell_excite_history[0][0].Count);
        }

        private bool validate_field(TextBox textbox)
        {
            double i = 0;
            if (String.IsNullOrEmpty(textbox.Text) || textbox.Text.Contains('.') || !double.TryParse(textbox.Text, out i))
                return false;
            textbox.BorderBrush = Brushes.Gray;
            return true;
        }

        public void reset_button_click(object sender, RoutedEventArgs e)
        {
            this.htm_results_list = new List<List<Rectangle>>();
            this.input_rectangle_list = new List<List<Rectangle>>();
            this.input_rectangle_list = new List<List<Rectangle>>();

            result_panel.Children.Clear();
            iteration_panel.Children.Clear();
            TextBlock info = new TextBlock()
            {
                Text = "Iteration number",
                TextAlignment = TextAlignment.Center,
                FontSize = 12,
                FontWeight = FontWeights.DemiBold,
            };
            iteration_panel.Children.Add(info);

            TextBlock blank = new TextBlock() { };
            result_panel.Children.Add(blank);
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

        }

        // windwow functions
        public List<Rectangle> split_rectangle(int width, int height, int rec_number, Grid rec_grid)
        {
            double new_rec_width = width / rec_number;

            List<Rectangle> rec_list = new List<Rectangle>();
            for (int i = 0; i < rec_number; i++)
            {
                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength(new_rec_width);
                rec_grid.ColumnDefinitions.Add(c1);

                Rectangle new_rec = new Rectangle()
                {
                    Width = new_rec_width,
                    Height = height,
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


        public void show_results(int rec_number)
        {
            int counter = 1;
            foreach (List<List<int>> iteration_res in this.excite_history.cell_excite_history)
            {
                int row_number = iteration_res.Count();

                TextBlock iter_num = new TextBlock()
                {
                    Text = counter.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 9, 0, 5),
                    Height = 11 * row_number - 4,
                    FontSize = 12,
                    FontWeight = FontWeights.DemiBold,
                };
                iteration_panel.Children.Add(iter_num);

                int row_counter = 0;
                foreach (List<int> row in iteration_res)
                {
                    Grid grid_for_rec = new Grid()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    if (row_counter == row_number - 1)
                        grid_for_rec.Margin = new Thickness(10, 0, 0, 10);
                    else
                        grid_for_rec.Margin = new Thickness(10, 0, 0, 0);

                    result_panel.Children.Add(grid_for_rec);
                    List<Rectangle> rectangles_list = this.split_rectangle(600, 11, rec_number, grid_for_rec);
                    fill_rectangle_with_data(row, rectangles_list);
                    htm_results_list.Add(rectangles_list);
                    row_counter++;
                }

                counter++;
            }

        }

    }
}
