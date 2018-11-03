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
            double denDiam = validateRes[1];
            double axDiam = validateRes[2];
            double flow = validateRes[3];
            double time = validateRes[4];


            if ((neuronLength > 0 ) && ( denDiam >0) && (axDiam > 0) && (flow >0 ) && (time> 0))
            {
                model1uc.length = neuronLength;
                double[] re1 = model1uc.Flow(20);

                neuron1.neuronLength = neuronLength;
                neuron1.denDiam = denDiam;
                neuron1.axDiam = axDiam;

                neuron2.neuronLength = neuronLength;
                neuron2.denDiam = denDiam;
                neuron2.axDiam = axDiam;

                neuron1.flow((double)time, flow);
                neuron2.flow((double)time, flow);

                //M1TimeBlock.Text = re1[0].ToString("0.##");
                //M1VolumeBlock.Text = re1[1].ToString("0.##");


                //neuron1.flow((double)time, speed);

                //var task0 = Task.Factory.StartNew(() => model1uc.Flow(20));
                //var task1 = Task.Factory.StartNew(() => neuron1.flow((double)time, speed));
                //var task2 = Task.Factory.StartNew(() => neuron2.flow((double)time, speed));

                //neuron0.flow((double)time, speed);




                //Parallel.Invoke(() => neuron0.flow((double)time, speed), () => neuron2.flow((double)time, speed));

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
            axonDiamBox.Text = "";
            flowBox.Text = "";
            timeBox.Text = "";

            neuron1.reset();
            neuron2.reset();
        }

        private double[] validate_boxes()
        {
            double neuronLength = 0;
            double denDiam = 0;
            double axDiam = 0;
            double flow = 0;
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


            TextBox[] boxes = { axonDiamBox, denDiamBox };
            foreach (TextBox box in boxes)
            {
                if (String.IsNullOrEmpty(box.Text) || box.Text.Contains('.') || (0.3 > Double.Parse(box.Text)) || (Double.Parse(box.Text) > 0.5))
                {
                    box.BorderBrush = System.Windows.Media.Brushes.Red;
                    box.Foreground = Brushes.Red;
                }
                else
                {
                    box.BorderBrush = System.Windows.Media.Brushes.Gray;
                    box.Foreground = Brushes.Black;
                    if (box == axonDiamBox)
                    {
                        axDiam = Double.Parse(axonDiamBox.Text);

                    }
                    else
                    {
                        denDiam = Double.Parse(axonDiamBox.Text);
                    }
                }
            }

            TextBox[] boxes2 = { flowBox, timeBox };
            foreach (TextBox box in boxes2)
            {

                if (box.Text.Contains('.') || String.IsNullOrEmpty(box.Text) || (Double.Parse(box.Text) < 0))
                {
                    box.BorderBrush = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    box.BorderBrush = System.Windows.Media.Brushes.Gray;
                    if (box == flowBox)
                    {
                        flow = Double.Parse(flowBox.Text);
                    }
                    else
                    {
                        time = Double.Parse(timeBox.Text);
                    }
                }
            }


            double[] res = { neuronLength, denDiam, axDiam, flow, time };
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

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            neuron1.stopFlow();
            neuron2.stopFlow();
        }
    }


}
