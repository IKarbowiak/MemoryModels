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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Threading;

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Rectangle[][] arrayModel1;
        private Model1UC model1uc;

        public MainWindow()
        {
            InitializeComponent();


            model1uc = new Model1UC();
            Grid.SetColumn(model1uc, 1);
            Grid.SetRow(model1uc, 1);
            gridModel1Main.Children.Add(model1uc);

        }

        private void start_Click(object sender, RoutedEventArgs e)
        {

            double[] validateRes = validate_boxes();

            double neuronLength = validateRes[0];
            double denAxdiam = validateRes[1];
            double speed = validateRes[2];
            double time = validateRes[3];


            if ((neuronLength > 0 ) && ( denAxdiam >0) && (speed > 0))
            {
                model1uc.length = neuronLength;
                double[] re1 = model1uc.Flow(20);
                M1TimeBlock.Text = re1[0].ToString("0.##");
                M1VolumeBlock.Text = re1[1].ToString("0.##");
                //Model1b model1b = new Model1b(neuronLength, denAxdiam, speed, arrayModel1);
                //double[] re = model1b.Flow(time);
                //Thread.Sleep(3000);
                //M1TimeBlock.Text = re[0].ToString("0.##");
                //M1VolumeBlock.Text = re[1].ToString("0.##");

                Model2 model2 = new Model2(neuronLength, denAxdiam, speed);
                double[] re2 = model2.Flow(time);
                M2TimeBlock.Text = re2[0].ToString("0.##");
                M2VolumeBlock.Text = re2[1].ToString("0.##");

                Model3 model3 = new Model3(neuronLength, denAxdiam, speed);
                double[] re3 = model3.Flow(time);
                M3TimeBlock.Text = re3[0].ToString("0.##");
                M3VolumeBlock.Text = re3[1].ToString("0.##");



            }

        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            M1TimeBlock.Text = "";
            M2TimeBlock.Text = "";
            M3TimeBlock.Text = "";

            M1VolumeBlock.Text = "";
            M2VolumeBlock.Text = "";
            M3VolumeBlock.Text = "";

            neuronLenBox.Text = "";
            denAxDiamBox.Text = "";
            speedBox.Text = "";
            timeBox.Text = "";
        }

        private double[] validate_boxes()
        {
            double neuronLength = 0;
            double denAxdiam = 0;
            double speed = 0;
            double time = 0;

            if (String.IsNullOrEmpty(neuronLenBox.Text) || denAxDiamBox.Text.Contains('.') || (Double.Parse(neuronLenBox.Text) < 30))
            {
                neuronLenBox.BorderBrush = System.Windows.Media.Brushes.Red;
                infoNeuron.Foreground = Brushes.Red;
            }

            else
            {
                neuronLenBox.BorderBrush = System.Windows.Media.Brushes.Gray;
                infoNeuron.Foreground = Brushes.Black;
                neuronLength = Int32.Parse(neuronLenBox.Text);
            }

            if (String.IsNullOrEmpty(denAxDiamBox.Text) || denAxDiamBox.Text.Contains('.') || (0.3 > Double.Parse(denAxDiamBox.Text)) || (Double.Parse(denAxDiamBox.Text) > 0.5))
            {
                denAxDiamBox.BorderBrush = System.Windows.Media.Brushes.Red;
                denAxDiamInfo.Foreground = Brushes.Red;
            }
            else
            {
                denAxDiamBox.BorderBrush = System.Windows.Media.Brushes.Gray;
                denAxDiamInfo.Foreground = Brushes.Black;
                denAxdiam = Double.Parse(denAxDiamBox.Text);
            }

            if (speedBox.Text.Contains('.') || String.IsNullOrEmpty(speedBox.Text) || (Double.Parse(speedBox.Text) < 0))
            {
                speedBox.BorderBrush = System.Windows.Media.Brushes.Red;
            }
            else
            {
                speedBox.BorderBrush = System.Windows.Media.Brushes.Gray;
                speed = Double.Parse(speedBox.Text);
            }


            if (timeBox.Text.Contains('.') || String.IsNullOrEmpty(timeBox.Text) || (Double.Parse(timeBox.Text) < 0))
            {
                timeBox.BorderBrush = System.Windows.Media.Brushes.Red;
            }
            else
            {
                timeBox.BorderBrush = System.Windows.Media.Brushes.Gray;
                time = Double.Parse(timeBox.Text);
            }

            double[] res = { neuronLength, denAxdiam, speed, time };
            return res;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window1 myWindow = new Window1();

            myWindow.ShowDialog();

            //MessageBox.Show(myWindow.tbReturn.Text);
        }
    }


}
