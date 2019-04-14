using PracaMagisterska.HTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

        private List<PracaMagisterska.HTM.HTM> htm_layers;
        private PracaMagisterska.HTM.HTM htm;
        private PracaMagisterska.HTM.HTM htm_layer_2;

        private List<List<Rectangle>> input_rectangle_list;
        private List<List<Rectangle>> reverse_input_rectangle_list;
        private List<List<Rectangle>> htm_results_list;
        private List<List<Rectangle>> htm_results_list_layer_2;

        private HTM_excite_history excite_history;
        private HTM_excite_history excite_history_layer_2;
        private bool after_reset = false;

        private double rec_width = 0;

        public HTM_window()
        {
            InitializeComponent();
            htm = new PracaMagisterska.HTM.HTM();
            htm_layer_2 = new PracaMagisterska.HTM.HTM();
            initialize_model();

            int width = (int)input_grid.Width;
            int height = (int)input_grid.Height;

            this.active_legend.Fill = active_color;
            this.inactive_legend.Fill = inactive_color;
            this.predictive_legend.Fill = predicting_color;

            this.htm_results_list = new List<List<Rectangle>>();
            this.htm_results_list_layer_2 = new List<List<Rectangle>>();

            int total_data_length = htm.data.Count * htm.data[0].Count;

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
            if (after_reset)
            {
                htm = new PracaMagisterska.HTM.HTM();
                htm_layer_2 = new PracaMagisterska.HTM.HTM();
                initialize_model();
            }

            Tuple<int, int> grid_column_shape = htm.get_columns_grid_size();
            excite_history = new HTM_excite_history(htm.layer, htm.cells_per_column, grid_column_shape.Item1, grid_column_shape.Item2);


            bool is_iteration_field_correct = this.validate_field(iteration_textbox);
            bool is_layer_filed_correct = this.validate_field(layer_textbox);

            if (!is_iteration_field_correct || !is_layer_filed_correct)
            {
                return;
            }

            int iteration_number = Int32.Parse(iteration_textbox.Text);
            if (iteration_number > 200 || iteration_number < 5)
            {
                iteration_textbox.BorderBrush = Brushes.Red;
                return;
            }

            int layer_number = Int32.Parse(layer_textbox.Text);
            if (layer_number > 5 || layer_number < 1)
            {
                layer_textbox.BorderBrush = Brushes.Red;
                return;
            }

            this.execute(excite_history, this.data_generator, iteration_number);
            show_results(excite_history.cell_excite_history[0][0].Count);
        }

        private bool validate_field(TextBox textbox)
        {
            double i = 0;
            if (String.IsNullOrEmpty(textbox.Text) || textbox.Text.Contains('.') || !double.TryParse(textbox.Text, out i))
            {
                textbox.BorderBrush = Brushes.Red;
                return false;
            }
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
            this.after_reset = true;
        }

        public List<List<int>> data_generator()
        {
            List<List<int>> old_data = this.htm.data;
            List<List<int>> data = new List<List<int>>();

            for (int i = 0; i < old_data.Count; i++) 
            {
                List<int> row = old_data[i];
                List<int> new_row = new List<int>();

                for (int j = 0; j < row.Count; j++)
                {
                    int value = row[j] == 1 ? 0 : 1;
                    new_row.Add(value);
                }

                data.Add(new_row);
            }

            return data;
        }


        public void initialize_model()
        {
            List<List<int>> data = new List<List<int>>();
            data.Add(new List<int> { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 });
            data.Add(new List<int> { 0, 0, 0, 0, 1, 0, 1, 0, 1, 1 });
            data.Add(new List<int> { 0, 0, 1, 0, 1, 0, 0, 0, 0, 1 });
            data.Add(new List<int> { 0, 1, 0, 0, 1, 1, 0, 1, 0, 1 });
            data.Add(new List<int> { 1, 0, 1, 0, 1, 0, 1, 0, 0, 1 });
            data.Add(new List<int> { 0, 0, 0, 1, 1, 0, 0, 0, 1, 1 });

            htm.initialize_input(data, 1);

        }

        public void execute(HTM_excite_history history, Func<List<List<int>>> data_generator, int iteration_number, bool learning = true)
        {
            // data_generator: generate next input data sample
            // post_generator: function to call after each interation

            int iteration_counter = iteration_number;
            while (iteration_counter > 0)
            {
                List<List<int>> new_data = data_generator();
                bool data_uptaded = htm.update_data(new_data);
                if (!data_uptaded)
                {
                    Console.WriteLine("Different input size. Execution breaked.");
                    break;
                }

                htm.execute();
                history.update_history(htm);

                List<List<int>> layer_one_data = excite_history.cell_excite_history.Last();
                if (iteration_counter == iteration_number)
                {
                    htm_layer_2.initialize_input(layer_one_data, 2, 2.2);
                    Tuple<int, int> grid_column_shape_layer_2 = htm_layer_2.get_columns_grid_size();
                    excite_history_layer_2 = new HTM_excite_history(htm_layer_2.layer, htm_layer_2.cells_per_column, 
                        grid_column_shape_layer_2.Item1, grid_column_shape_layer_2.Item2);
                }
                else
                {
                    htm_layer_2.update_data(layer_one_data);
                }
                htm_layer_2.execute();
                excite_history_layer_2.update_history(htm_layer_2);

                iteration_counter -= 1;
            }

        }

        // windwow functions
        public List<Rectangle> split_rectangle(double width, int height, int rec_number, Grid rec_grid)
        {
            if (rec_width == 0)
                rec_width = width / rec_number;

            List<Rectangle> rec_list = new List<Rectangle>();
            for (int i = 0; i < rec_number; i++)
            {
                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength(rec_width);
                rec_grid.ColumnDefinitions.Add(c1);

                Rectangle new_rec = new Rectangle()
                {
                    Width = rec_width,
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

        public List<int> prepare_data_for_filling_rectangles(List<List<int>> data, bool reverse = false)
        {
            List<int> new_data = new List<int>();
            foreach (List<int> row in data)
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

                row_counter = 0;
                List<List<int>> layer2 = excite_history_layer_2.cell_excite_history[counter - 1];
                foreach (List<int> row in layer2)
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
                    List<Rectangle> rectangles_list = this.split_rectangle(rec_width * row.Count, 11, row.Count, grid_for_rec);
                    fill_rectangle_with_data(row, rectangles_list);
                    htm_results_list_layer_2.Add(rectangles_list);
                    row_counter++;
                }

                Line line = new Line()
                {
                    Stretch = Stretch.Fill,
                    VerticalAlignment = VerticalAlignment.Top,
                    Stroke = Brushes.Black,
                    X2 = 2,
                    Margin = new Thickness(0, 10, 0, 20),
                    StrokeThickness = 2,
                };

                result_panel.Children.Add(line);

                counter++;
            }

        }

    }
}
