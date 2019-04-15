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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for HH_model.xaml
    /// </summary>
    public partial class HH_model_window : Window
    {
        private HH_model hh_model;
        public HH_model_window()
        {
            hh_model = new HH_model();
            DataContext = hh_model;
            hh_model.start_action();
            InitializeComponent();
            this.action_res.InvalidatePlot(true);

        }

        public void refresh_button(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                action_res.InvalidatePlot(true);
            });
            foreach (DataPoint point in hh_model.Points)
            {
                Console.WriteLine(point);
            }
        }


    }
}
