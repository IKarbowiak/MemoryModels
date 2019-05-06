using PracaMagisterska.HTM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private System.Windows.Media.SolidColorBrush active_and_predicting_color = System.Windows.Media.Brushes.Orange;
        private System.Windows.Media.SolidColorBrush damage_color = System.Windows.Media.Brushes.Brown;

        private List<PracaMagisterska.HTM.HTM> htm_layers;
        private List<HTM_excite_history> layers_excite_history;
        private Random rnd = new Random(Guid.NewGuid().GetHashCode());

        private double rec_width = 0;
        private int modulo_value = 3;
        private int iteration_number = 0;
        private double cell_damage = 0;
        private double compression_factor = 2.1;
        private double row_height = 11;
        private int iteration_counter = 0;
        private bool is_learning = true;

        private string coverage_file_path = "D:\\PWR\\Magisterka\\coverage_res.csv";
        private string matching_file_path = "D:\\PWR\\Magisterka\\matching_res.csv";
        private StringBuilder csv = new StringBuilder();
        private StringBuilder csv_matching = new StringBuilder();
        private Dictionary<int, int> input_mapping = new Dictionary<int, int>();


        public HTM_window()
        {
            InitializeComponent();

            this.htm_layers = new List<HTM.HTM>();
            this.layers_excite_history = new List<HTM_excite_history>();

            this.active_legend.Fill = active_color;
            this.inactive_legend.Fill = inactive_color;
            this.predictive_legend.Fill = predicting_color;
            this.active_predictive__legend.Fill = active_and_predicting_color;
            this.damage_legend.Fill = damage_color;

            input_mapping.Add(1, 1);
            input_mapping.Add(2, 5);
            input_mapping.Add(3, 3);

        }

        public void start_button_click(object sender, RoutedEventArgs e)
        {
            this.reset_panels();

            bool is_iteration_field_correct = this.validate_field(iteration_textbox);
            bool is_layer_filed_correct = this.validate_field(layer_textbox);
            bool cell_damage_correct = this.validate_field(cellDamage_textbox);

            if (!is_iteration_field_correct || !is_layer_filed_correct)
            {
                return;
            }

            this.iteration_number = Int32.Parse(iteration_textbox.Text);
            if (iteration_number > 500 || iteration_number < 5)
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

            this.cell_damage = Double.Parse(cellDamage_textbox.Text);
            if (cell_damage < 0 || cell_damage > 100)
            {
                cellDamage_textbox.BorderBrush = Brushes.Red;
                return;
            }

            this.layers_excite_history.Clear();
            this.htm_layers.Clear();

            for (int i = 0 ; i < layer_number; i++)
            {
                HTM.HTM htm_layer = new HTM.HTM();
                this.htm_layers.Add(htm_layer);
            }

            this.is_learning = (bool)this.learning_check_box.IsChecked;
            if (!is_learning)
            {
                this.modulo_value = 50;
                this.iteration_number = 50;
                this.iteration_textbox.Text = "50";
            }
            this.execute(iteration_number, cell_damage, is_learning);
            show_results(this.layers_excite_history[0].cell_excite_history[0][0].Count);

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

        private void reset_panels()
        {
            result_panel.Children.Clear();
            iteration_panel.Children.Clear();
            input_marker_panel.Children.Clear();
            layer_panel.Children.Clear();

            foreach (KeyValuePair<StackPanel, string> data in new Dictionary<StackPanel, string> { { iteration_panel, "Iteration number" },
                { result_panel, "HTM layers" }, { input_marker_panel, "Best matching" },  { layer_panel, "Layer" } })
            {
                TextBlock info = new TextBlock()
                {
                    Text = data.Value,
                    TextAlignment = TextAlignment.Center,
                    FontSize = 12,
                    FontWeight = FontWeights.DemiBold,
                    Margin = new Thickness(0, 0, 0, 5),
                };
                data.Key.Children.Add(info);
            }
        }

        public void change_sequence_button_click(object sender, RoutedEventArgs e)
        {
            int old_iter_num = this.iteration_number;
            this.iteration_number = Int32.Parse(iteration_textbox.Text);
            int new_input_value = rnd.Next(20, 80);

            this.input_mapping[3] = new_input_value;
            this.execute(iteration_number, cell_damage, true, true);
            show_results(this.layers_excite_history[0].cell_excite_history[0][0].Count, old_iter_num);
        }


        public void execute(int iteration_number, double cell_damage, bool learning = true, bool additional_iter = false)
        {
            int iteration_counter = iteration_number;
            int counter = 0;
            while (iteration_counter > 0)
            {
                this.execute_ones(iteration_counter == iteration_number, counter, cell_damage, compression_factor, learning, additional_iter);

                iteration_counter -= 1;
                counter++;
            }

            this.iteration_counter = counter;

        }

        public void execute_ones(bool first_iter, int counter, double cell_damage, double compression_factor, bool learning, bool additional_iter = false)
        {
            List<List<int>> input_data = this.process_layer_1(this.htm_layers[0], first_iter,
                    cell_damage, counter, learning, additional_iter);
            if (input_data.Count() == 0)
                return;

            for (int i = 1; i < this.htm_layers.Count; i++)
            {
                HTM.HTM next_layer = this.htm_layers[i];

                if (first_iter)
                {
                    next_layer.initialize_input(input_data, i + 1, cell_damage, compression_factor);
                    Tuple<int, int> layer_grid_column_shape = next_layer.get_columns_grid_size();
                    this.layers_excite_history.Add(new HTM_excite_history(next_layer.layer, next_layer.cells_per_column,
                        layer_grid_column_shape.Item1, layer_grid_column_shape.Item2));
                }
                else
                {
                    next_layer.update_data(input_data);
                }

                next_layer.execute(learning);

                HTM_excite_history layer_history = this.layers_excite_history[i];
                if (additional_iter)
                    counter = this.iteration_counter;
                layer_history.update_history(next_layer, counter + 1);
                input_data = this.prepare_input_data(layer_history.cell_excite_history.Last());
            }
        }

        private List<List<int>> prepare_input_data(List<List<int>> prev_layer_data)
        {
            List<List<int>> input_data = new List<List<int>>();
            foreach (List<int> row in prev_layer_data)
            {
                List<int> new_row = new List<int>();
                foreach (int value in row)
                {
                    if (value == 1 || value == 2)
                        new_row.Add(1);
                    else
                        new_row.Add(0);
                }
                input_data.Add(new_row);
            }
            return input_data;
        }

        private List<List<int>> process_layer_1(HTM.HTM HTM_layer_1, bool initialize, double cell_damage, int input_value, bool learning, bool additional_iter = false)
        {
            int input_data_index = input_value % this.modulo_value + 1;
            if (this.is_learning)
                input_data_index = this.input_mapping[input_data_index];
            //if (additional_iter)
            //    input_data_index = input_value;

            List<List<int>> data = HTM_parameters.INPUT_DATA[input_data_index];
            List<List<int>> layer_one_data = new List<List<int>>();
            if (initialize && !additional_iter)
            {
                HTM_layer_1.initialize_input(data, 1, cell_damage);
                Tuple<int, int> grid_column_shape = HTM_layer_1.get_columns_grid_size();
                this.layers_excite_history.Add(new HTM_excite_history(HTM_layer_1.layer, HTM_layer_1.cells_per_column, grid_column_shape.Item1, grid_column_shape.Item2));
                if (cell_damage > 0)
                    this.htm_layers[0].damageCells(cell_damage);
            }
            else
            {
                bool data_uptaded = HTM_layer_1.update_data(data);
                if (!data_uptaded)
                {
                    Console.WriteLine("Different input size. Execution breaked.");
                    return layer_one_data;
                }
            }
            HTM_excite_history HTM_history = this.layers_excite_history[0];
            HTM_layer_1.execute(learning);
            HTM_history.update_history(HTM_layer_1, input_data_index);
            layer_one_data = HTM_history.cell_excite_history.Last();
            return this.prepare_input_data(layer_one_data);
        }

        // windwow functions
        public List<Rectangle> split_rectangle(double width, double height, int rec_number, Grid rec_grid)
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
            const int ACTIVE_AND_PREDICTING = 3;
            const int DAMAGE = 4;

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] == INACTIVE)
                    rec_list[i].Fill = inactive_color;
                else if (data[i] == ACTIVE)
                    rec_list[i].Fill = active_color;
                else if (data[i] == PREDICTING)
                    rec_list[i].Fill = predicting_color;
                else if (data[i] == ACTIVE_AND_PREDICTING)
                    rec_list[i].Fill = active_and_predicting_color;
                else if (data[i] == DAMAGE)
                    rec_list[i].Fill = damage_color;
            }
        }

        public TextBlock prepare_text_block(string text, double height, Thickness margin)
        {

            TextBlock text_block = new TextBlock()
            {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = margin,
                Height = height,
                FontSize = height == 11 ? 9 : 12,
                FontWeight = FontWeights.DemiBold,
            };

            return text_block;
        }

        public void show_results(int rec_number, int start_value = 0)
        {
            for (int iter_counter = start_value; iter_counter < this.layers_excite_history[0].cell_excite_history.Count(); iter_counter++)
            {
                this.show_result_from_one_iteration(this.row_height, iter_counter);
            }

            File.WriteAllText(this.coverage_file_path, csv.ToString());
            if (!this.is_learning)
                File.WriteAllText(this.matching_file_path, csv_matching.ToString());
        }

        public void show_result_from_one_iteration(double row_height, int iter_counter, bool additional_iter = false)
        {
            int row_number = this.layers_excite_history[0].cell_excite_history[0].Count();
            double height = row_height * row_number * (this.htm_layers.Count) + 10 * (this.htm_layers.Count) + 22 - 4; // 22 from Line margins

            TextBlock iter_num = this.prepare_text_block(String.Format("{0}", (iter_counter + 1)),
                height, new Thickness(0, 9, 0, 5));
            iteration_panel.Children.Add(iter_num);

            this.show_input_data(iter_counter, additional_iter);

            this.show_layer_results(row_number, iter_counter, row_height);

            check_coverage(this.layers_excite_history[0], iter_counter, this.csv);

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
        }

        public void check_coverage(HTM_excite_history history, int iter_num, StringBuilder csv)
        {
            List<int> predictive_previous = new List<int>();
            List<int> active_current = new List<int>();
            if (iter_num > 0)
            {
                predictive_previous = history.predictive_cell_history[iter_num - 1];
                active_current = history.active_cell_history[iter_num];

                List<int> list_difference = active_current.Except(predictive_previous).ToList();
                double cov = (((double)active_current.Count() - (double)list_difference.Count()) / (double)active_current.Count() * 100.0);
                csv.AppendLine(String.Format("{0}; {1}; {2}; {3}", iter_num + 1, ((double)active_current.Count() - (double)list_difference.Count()), active_current.Count(), cov));
            }
        }

        public void show_layer_results(int row_number, int iter_counter, double row_height)
        {
            for (int layer_counter = 0; layer_counter < this.htm_layers.Count; layer_counter++)
            {
                HTM_excite_history layer_history = this.layers_excite_history[layer_counter];

                List<List<int>> iteration_result = layer_history.cell_excite_history[iter_counter];
                int row_counter = 0;
                foreach (List<int> row in iteration_result)
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

                    double width = this.rec_width == 0 ? 800 : this.rec_width * row.Count;

                    List<Rectangle> rectangles_list = this.split_rectangle(width, row_height, row.Count, grid_for_rec);
                    fill_rectangle_with_data(row, rectangles_list);
                    row_counter++;
                }

                double layer_info_height = row_height * row_number;
                if (layer_counter == htm_layers.Count() - 1)
                    layer_info_height += 32;

                TextBlock layer = this.prepare_text_block((layer_counter + 1).ToString(), layer_info_height, new Thickness(15, 10, 0, 0));
                this.layer_panel.Children.Add(layer);

                if (!this.is_learning)
                {
                    int best_matching_input = layer_history.find_similarities(iter_counter + 1);
                    csv_matching.AppendLine(String.Format("{0}; {1}", iter_counter + 1, best_matching_input));
                    TextBlock marker = this.prepare_text_block(best_matching_input.ToString(), layer_info_height, new Thickness(15, 10, 0, 0));
                    input_marker_panel.Children.Add(marker);
                }
            }
        }

        public void show_input_data(int input_value, bool additional_iter= false)
        {
            int input_data_index = input_value % this.modulo_value + 1;
            if (this.is_learning)
                input_data_index = this.input_mapping[input_data_index];
            if (additional_iter)
                input_data_index = input_value;


            List<StackPanel> panel_list = new List<StackPanel> {this.layer_panel, this.iteration_panel, this.input_marker_panel};
            foreach ( StackPanel panel in panel_list)
            {
                string text = panel == this.layer_panel ? String.Format("input value: {0}", input_data_index) : "";
                TextBlock line = this.prepare_text_block(text, 11, new Thickness(10, 0, 0, 10));
                panel.Children.Add(line);
            };

            List<List<int>> input_data = HTM_parameters.INPUT_DATA[input_data_index];
            int width = 800;
            int height = 11;

            Grid grid_for_rec = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            grid_for_rec.Margin = new Thickness(10, 0, 0, 10);

            result_panel.Children.Add(grid_for_rec);
            List<int> input_data_for_filling = this.prepare_data_for_filling_rectangles(input_data);
            List<Rectangle> input_rectangle_list = this.split_rectangle(width, height, input_data_for_filling.Count, grid_for_rec);
            fill_rectangle_with_data(input_data_for_filling, input_rectangle_list);
        }

    }
}
