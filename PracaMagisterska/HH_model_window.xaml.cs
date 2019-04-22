using OxyPlot;
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
using OxyPlot.Wpf;
using System.Windows.Media;

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for HH_model.xaml
    /// </summary>
    public partial class HH_model_window : Window
    {
        private HH_model hh_model;
        public List<DataPoint> potential_points { get; private set; }
        public List<DataPoint> current_points { get; private set; }
        public HH_model_window()
        {
            hh_model = new HH_model();
            DataContext = hh_model;
            InitializeComponent();
        }


        private void start_button(object sender, RoutedEventArgs e)
        {
            Tuple<List<DataPoint>, List<DataPoint>> res = hh_model.start_action();
            this.potential_points = res.Item1;
            this.current_points = res.Item2;

            this.create_plot("Change of membrane potential in time", "Membrane potential [mV]", "Time [ms]", this.potential_points, plot1_grid);
            this.create_plot("Change of current in time", "Current [mA]", "Time [ms]", this.current_points, plot2_grid);
        }

        public void create_plot(string title, string y_axis, string x_axis, List<DataPoint> points, Grid plot_grid)
        {
            PlotView plot_view = new PlotView();
            PlotModel plot_model = new PlotModel
            {
                Title = title,
                TitleFont = "Arial",
                TitleFontSize = 14,
                TitleFontWeight = OxyPlot.FontWeights.Bold
            };

            // Adjust Y-axis title
            plot_model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = y_axis,
            });

            // Adjust X-axis title 
            plot_model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                MajorStep = 5,
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = x_axis,
            });

            OxyPlot.Series.LineSeries line_series = new OxyPlot.Series.LineSeries() { Color = OxyPlot.OxyColor.Parse("#206040") };
            foreach (DataPoint data_point in points)
            {
                line_series.Points.Add(data_point);
            }
            plot_model.Series.Add(line_series);

            plot_view.Model = plot_model;

            plot_grid.Children.Add(plot_view);
        }
    }
}
