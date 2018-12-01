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
        public Neuron neuron0;
        public Neuron neuron1;
        public Neuron neuron2;
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        private DateTime TimerStart;
        private bool newFlow;
        private double time;
        private double flow;
        public string currentConf { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //Debug.Print("This is debug prin");


            //model1uc = new Model1UC();
            //Grid.SetColumn(model1uc, 1);
            //Grid.SetRow(model1uc, 1);
            //gridModel1Main.Children.Add(model1uc);


            neuron0 = new Neuron(0);
            Grid.SetColumn(neuron0, 1);
            Grid.SetRow(neuron0, 1);
            gridModel1Main.Children.Add(neuron0);

            //neuron0 = new Neuron0();
            //Grid.SetColumn(neuron0, 1);
            //Grid.SetRow(neuron0, 1);
            //gridModel1Main.Children.Add(neuron0);

            neuron1 = new Neuron(1);
            Grid.SetColumn(neuron1, 1);
            Grid.SetRow(neuron1, 1);
            gridModel2Main.Children.Add(neuron1);

            neuron2 = new Neuron(2);
            foreach (Dendrite den in neuron2.dendrites_list)
                den.isBlocked = false;
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
                Console.WriteLine("In bad place");
                string[] seconds = timerTextBlock.Text.Split(':');
                delay = Int32.Parse(seconds[1].Trim(new Char[] { '0', }));
                newTime = time - delay;
                timer.Interval = TimeSpan.FromSeconds(newTime + 1);
                timer.Start();
                timer2.Start();
                this.TimerStart = DateTime.Now.AddSeconds(-delay);
                //neuron0.continueFlow((int)newTime);
                //neuron1.continueFlow((int)newTime);
                //neuron2.continueFlow((int)newTime);
            }
            else
            {
                Console.WriteLine("In good place");


                timer.Interval = TimeSpan.FromSeconds(time);
                timer2.Interval = TimeSpan.FromMilliseconds(100);


                if ((flow > 0) && (time > 0))
                {
                    Console.WriteLine("Deeper");
                    startButton.IsEnabled = false;
                    M1VolumeBlock.Text = "0";
                    M2VolumeBlock.Text = "0";
                    M3VolumeBlock.Text = "0";

                    this.TimerStart = DateTime.Now;
                    timer2.Tick += (sender2, e2) =>
                    {
                        this.neuronFlow(sender2, e2, this.neuron0, flow);
                        this.neuronFlow(sender2, e2, this.neuron1, flow);
                        this.neuronFlow(sender2, e2, this.neuron2, flow);
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
            }

        }


        private double neuronFlow(object sender, EventArgs e, Neuron neuron, double flow)
        {
            double toPush = 0;
            double volumeToPushNext = 0;
            Console.WriteLine("In neuron flow");
            if (neuron.dendrites_list.Count() == 0)
            {
                Console.WriteLine("Neuron 0");
                volumeToPushNext = neuron.axon.newFlow(sender, e, flow);
                neuron.volumeToPush = volumeToPushNext;
                return volumeToPushNext;
            }

            Console.WriteLine("Multiply neuron");
            foreach (Dendrite dendrite in neuron.dendrites_list)
            {
                if (!dendrite.isBlocked)
                {
                    Tuple<bool, double> dendriteRes = dendrite.newFlow(sender, e, flow);
                    if (dendriteRes.Item1) toPush += dendriteRes.Item2;
                }
            }
            Thread.Sleep(10);
            if (toPush > 0)
            {
                Tuple<bool, double> somaRes = neuron.soma.newFlow(sender, e, toPush);
                if (somaRes.Item1)
                {
                    volumeToPushNext = neuron.axon.newFlow(sender, e, somaRes.Item2);
                    neuron.volumeToPush = volumeToPushNext;
                }
            }
            return volumeToPushNext;
        }

        public void flow_neuron(object sender, EventArgs e, Neuron neuron, double flow)
        {
            double toPush = 0;
            foreach (Dendrite dendrite in neuron.dendrites_list)
            {
                Tuple<bool, double> dendriteRes = dendrite.newFlow(sender, e, flow);
                if (dendriteRes.Item1) toPush += dendriteRes.Item2;

            }
            Thread.Sleep(10);
            if (toPush > 0)
            {
                Tuple<bool, double> somaRes = neuron.soma.newFlow(sender, e, toPush);
                if (somaRes.Item1)
                {
                    neuron.axon.newFlow(sender, e, somaRes.Item2);
                }
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

            timerTextBlock.Text = "00:00";

            //neuron0.reset();
            //neuron1.reset();
            //neuron2.reset();

            this.newFlow = true;
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DragAndDropPanel myWindow = new DragAndDropPanel();

            myWindow.ShowDialog();

            //MessageBox.Show(myWindow.tbReturn.Text);
        }



        //private void defaultParButton_Click(object sender, RoutedEventArgs e)
        //{
        //    neuronLenBox.Text = "40";
        //    denDiamBox.Text = "0,4";
        //    axonDiamBox.Text = "0,4";
        //    flowBox.Text = "8";
        //    timeBox.Text = "30";
        //}

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
            SetParametersWindow setParamsWindow;
            if (currentConf != null) {
                setParamsWindow = new SetParametersWindow(this.getConfParamsXML, this,  this.currentConf);
            }
            else
            {
                setParamsWindow = new SetParametersWindow(this.getConfParamsXML, this);
            }
            setParamsWindow.ShowDialog();
        }

    
        private void getConfParamsXML(string path, double time, double flow)
        {
            this.currentConf = path;
            this.time = time;
            this.flow = flow / 10;
            Console.WriteLine("In main Window" + path);
        }

    }


}
