using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PracaMagisterska
{

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

        }

        private void personal_sulution_button(object sender, RoutedEventArgs e)
        {
            FlowModel flow_model = new FlowModel();
            flow_model.ShowDialog();
        }

        private void HTM_model_button(object sender, RoutedEventArgs e)
        {
            HTM_window HTM_window = new HTM_window();
            HTM_window.ShowDialog();
        }

        private void HH_model_button(object sender, RoutedEventArgs e)
        {
            HH_model_window hh_model = new HH_model_window();
            hh_model.ShowDialog();
        }
    }


}