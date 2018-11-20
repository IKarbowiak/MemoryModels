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
        private Neuron0 neuron0;
        private Neuron1 neuron1;
        private Neuron2 neuron2;
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        private DateTime TimerStart { get; set; }
        private bool newFlow;
        private double time;

        public MainWindow()
        {
            InitializeComponent();
            //Debug.Print("This is debug prin");


            //model1uc = new Model1UC();
            //Grid.SetColumn(model1uc, 1);
            //Grid.SetRow(model1uc, 1);
            //gridModel1Main.Children.Add(model1uc);

            neuron0 = new Neuron0();
            Grid.SetColumn(neuron0, 1);
            Grid.SetRow(neuron0, 1);
            gridModel1Main.Children.Add(neuron0);

            neuron1 = new Neuron1();
            Grid.SetColumn(neuron1, 1);
            Grid.SetRow(neuron1, 1);
            gridModel2Main.Children.Add(neuron1);

            neuron2 = new Neuron2();
            Grid.SetColumn(neuron2, 1);
            Grid.SetRow(neuron2, 1);
            gridModel3Main.Children.Add(neuron2);

            timer = new System.Windows.Threading.DispatcherTimer();
            timer2 = new System.Windows.Threading.DispatcherTimer();
            this.newFlow = true;
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            double newTime;
            int delay;
            if (!this.newFlow && timerTextBlock.Text != "00:00")
            {
                string[] seconds = timerTextBlock.Text.Split(':');
                delay = Int32.Parse(seconds[1].Trim(new Char[] { '0', }));
                newTime = time - delay;
                timer.Interval = TimeSpan.FromSeconds(newTime + 1);
                timer.Start();
                timer2.Start();
                this.TimerStart = DateTime.Now.AddSeconds(-delay);
                neuron0.continueFlow((int)newTime);
                neuron1.continueFlow((int)newTime);
                neuron2.continueFlow((int)newTime);
            }
            else
            {

                double[] validateRes = validate_boxes();

                double neuronLength = validateRes[0];
                double denDiam = validateRes[1];
                double axDiam = validateRes[2];
                double flow = validateRes[3] / 10;
                time = validateRes[4];

                timer.Interval = TimeSpan.FromSeconds(time);
                timer2.Interval = TimeSpan.FromMilliseconds(100);


                if ((neuronLength > 0) && (denDiam > 0) && (axDiam > 0) && (flow > 0) && (time > 0))
                {
                    //model1uc.length = neuronLength;
                    //double[] re1 = model1uc.Flow(20);
                    startButton.IsEnabled = false;
                    M1VolumeBlock.Text = "0";
                    M2VolumeBlock.Text = "0";
                    M3VolumeBlock.Text = "0";

                    neuron1.neuronLength = neuronLength;
                    neuron1.denDiam = denDiam;
                    neuron1.axDiam = axDiam;

                    neuron2.neuronLength = neuronLength;
                    neuron2.denDiam = denDiam;
                    neuron2.axDiam = axDiam;

                    //neuron0.flow((double)time, flow);
                    //neuron1.flow((double)time, flow);
                    //neuron2.flow((double)time, flow);

                    this.TimerStart = DateTime.Now;
                    timer2.Tick += (sender2, e2) =>
                    {
                        this.neuron0.axon.newFlow(sender2, e2, flow);

                        Tuple<bool, double> dendriteRes = neuron1.dendrite.newFlow(sender, e, flow);
                        Thread.Sleep(10);
                        if (dendriteRes.Item1)
                        {
                            Tuple<bool, double> somaRes = neuron1.soma.newFlow(sender, e, dendriteRes.Item2);
                            if (somaRes.Item1)
                            {
                                neuron1.axon.newFlow(sender, e, somaRes.Item2);
                            }
                        }

                        Tuple<bool, double> dendriteRes1 = neuron2.dendrite1.newFlow(sender, e, flow);
                        Tuple<bool, double> dendriteRes2 = neuron2.dendrite2.newFlow(sender, e, flow);
                        Thread.Sleep(10);
                        if (dendriteRes1.Item1 | dendriteRes2.Item1)
                        {
                            Tuple<bool, double> somaRes = neuron2.soma.newFlow(sender, e, dendriteRes1.Item2 + dendriteRes2.Item2);
                            if (somaRes.Item1)
                            {
                                neuron2.axon.newFlow(sender, e, somaRes.Item2);
                            }
                        }

                        this.myTimerTick(sender2, e2);
                        
                    };
                    timer2.Start();


                    timer.Tick += (sender2, e1) =>
                    {
                        Console.WriteLine("In tick");
                        showResults(sender2, e);
                    };
                    timer.Start();


                    this.newFlow = false;
                }
           


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

        private void myTimerTick(object sender, EventArgs e)
        {
            TimeSpan currentValue = DateTime.Now - this.TimerStart;
            this.timerTextBlock.Text = currentValue.ToString(@"mm\:ss");
        }

        private void showResults(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer2.Stop();
            M1VolumeBlock.Text = neuron0.outFlowVolume.ToString();
            M2VolumeBlock.Text = neuron1.outFlowVolume.ToString();
            M3VolumeBlock.Text = neuron2.outFlowVolume.ToString();
            startButton.IsEnabled = true;
            this.newFlow = true;

        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            //M1TimeBlock.Text = "";
            //M2TimeBlock.Text = "";
            //M3TimeBlock.Text = "";

            M1VolumeBlock.Text = "";
            M2VolumeBlock.Text = "";
            M3VolumeBlock.Text = "";

            neuronLenBox.Text = "";
            denDiamBox.Text = "";
            axonDiamBox.Text = "";
            flowBox.Text = "";
            timeBox.Text = "";
            timerTextBlock.Text = "00:00";

            neuron0.reset();
            neuron1.reset();
            neuron2.reset();

            this.newFlow = true;
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
                //infoNeuron.Foreground = Brushes.Red;
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
            DragAndDropPanel myWindow = new DragAndDropPanel();

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
            timer.Stop();
            timer2.Stop();
            //neuron0.stopFlow();
            //neuron1.stopFlow();
            //neuron2.stopFlow();
            startButton.IsEnabled = true;
            this.newFlow = false;
        }

        private void setParamButton_Click(object sender, RoutedEventArgs e)
        {
            SetParametersWindow myWindow = new SetParametersWindow();
            int n;

            myWindow.flowBoxM1.Text = this.flowBox.Text;
            myWindow.flowBoxM2.Text = this.flowBox.Text;
            myWindow.flowBoxM3.Text = this.flowBox.Text;

            myWindow.timeBoxM1.Text = this.timeBox.Text;
            myWindow.timeBoxM2.Text = this.timeBox.Text;
            myWindow.timeBoxM3.Text = this.timeBox.Text;

            myWindow.denDiamBoxM1.Text = this.denDiamBox.Text;
            myWindow.denDiamBoxM2.Text = this.denDiamBox.Text;
            myWindow.den1DiamBoxM3.Text = this.denDiamBox.Text;
            myWindow.den2DiamBoxM3.Text = this.denDiamBox.Text;

            myWindow.neuronLenBoxM1.Text = this.neuronLenBox.Text;

            if (int.TryParse(this.neuronLenBox.Text, out n))
            {
                myWindow.denLenBoxM2.Text = (Int32.Parse(this.neuronLenBox.Text) / 26).ToString();
                myWindow.den1LenBoxM3.Text = (Int32.Parse(this.neuronLenBox.Text) / 26).ToString();
                myWindow.den2LenBoxM3.Text = (Int32.Parse(this.neuronLenBox.Text) / 26).ToString();

                myWindow.axonLenM2.Text = (Int32.Parse(this.neuronLenBox.Text) * 20 / 26).ToString();
                myWindow.axonLenM3.Text = (Int32.Parse(this.neuronLenBox.Text) * 20 / 26).ToString();

                myWindow.somaDiamBoxM2.Text = (Int32.Parse(this.neuronLenBox.Text) * 10 / 26).ToString();
                myWindow.somaDiamBoxM3.Text = (Int32.Parse(this.neuronLenBox.Text) * 10 / 26).ToString();
            }

            myWindow.axonDiamBoxM1.Text = this.axonDiamBox.Text;
            myWindow.axonDiamM2.Text = this.axonDiamBox.Text;
            myWindow.axonDiamM3.Text = this.axonDiamBox.Text;

            myWindow.ShowDialog();

        }
    }


}
