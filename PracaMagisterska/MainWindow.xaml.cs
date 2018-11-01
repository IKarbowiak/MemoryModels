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
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };


        public static void Refresh(this UIElement uiElement)

        {
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }
    }


    public partial class MainWindow : Window
    {
        private Model1UC model1uc;
        private Dendrite dendrite;
        private Soma soma;
        private Axon axon;
        private Neuron0 neuron0;
        private Neuron1 neuron1;
        private Neuron2 neuron2;

        public MainWindow()
        {
            InitializeComponent();


            model1uc = new Model1UC();
            Grid.SetColumn(model1uc, 1);
            Grid.SetRow(model1uc, 1);
            gridModel1Main.Children.Add(model1uc);

            //neuron0 = new Neuron0();
            //Grid.SetColumn(neuron0, 1);
            //Grid.SetRow(neuron0, 1);
            //gridModel1Main.Children.Add(neuron0);

            neuron1 = new Neuron1();
            Grid.SetColumn(neuron1, 1);
            Grid.SetRow(neuron1, 1);
            gridModel2Main.Children.Add(neuron1);

            neuron2 = new Neuron2();
            Grid.SetColumn(neuron2, 1);
            Grid.SetRow(neuron2, 1);
            gridModel3Main.Children.Add(neuron2);

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

                neuron0.flow((double)time, speed);

                neuron1.flow((double)time, speed);

                neuron2.flow((double)time, speed);

                //Parallel.Invoke(() => neuron0.flow((double)time, speed), () => neuron2.flow((double)time, speed), () => double[] re1 = model1uc.Flow(20));

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
            denDiamBox.Text = "";
            flowBox.Text = "";
            timeBox.Text = "";
        }

        private double[] validate_boxes()
        {
            double neuronLength = 0;
            double denAxdiam = 0;
            double speed = 0;
            double time = 0;

            if (String.IsNullOrEmpty(neuronLenBox.Text) || denDiamBox.Text.Contains('.') || (Double.Parse(neuronLenBox.Text) < 30))
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

            if (String.IsNullOrEmpty(denDiamBox.Text) || denDiamBox.Text.Contains('.') || (0.3 > Double.Parse(denDiamBox.Text)) || (Double.Parse(denDiamBox.Text) > 0.5))
            {
                denDiamBox.BorderBrush = System.Windows.Media.Brushes.Red;
                denDiamBox.Foreground = Brushes.Red;
            }
            else
            {
                denDiamBox.BorderBrush = System.Windows.Media.Brushes.Gray;
                denDiamBox.Foreground = Brushes.Black;
                denAxdiam = Double.Parse(denDiamBox.Text);
            }

            if (flowBox.Text.Contains('.') || String.IsNullOrEmpty(flowBox.Text) || (Double.Parse(flowBox.Text) < 0))
            { 
                flowBox.BorderBrush = System.Windows.Media.Brushes.Red;
            }
            else
            {
                flowBox.BorderBrush = System.Windows.Media.Brushes.Gray;
                speed = Double.Parse(flowBox.Text);
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

        private void defaultParButton_Click(object sender, RoutedEventArgs e)
        {
            neuronLenBox.Text = "40";
            denDiamBox.Text = "0,4";
            axonDiamBox.Text = "0,4";
            flowBox.Text = "8";
            timeBox.Text = "30";
        }
    }


}
