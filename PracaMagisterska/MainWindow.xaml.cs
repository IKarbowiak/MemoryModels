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
using System.Xml.Linq;

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    //public static class ExtensionMethods
    //{
    //    private static Action EmptyDelegate = delegate () { };


    //    public static void Refresh(this UIElement uiElement)

    //    {
    //        uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
    //    }
    //}


    public partial class MainWindow : Window
    {
        public Neuron neuron0;
        public Neuron neuron1;
        public Neuron neuron2;
        private System.Windows.Threading.DispatcherTimer drainingTimer;
        private System.Windows.Threading.DispatcherTimer timer;
        private DateTime TimerStart;
        private bool newFlow;
        private double time;
        private double flow;
        public string currentConf { get; set; }
        private int counter = 0;
        private int tickThreshold;
        private int timerTimeSpan;
        private double drainingVolume;
        private System.Windows.Media.SolidColorBrush color = System.Windows.Media.Brushes.DodgerBlue;
        private bool remindStarted = false;
        private bool blockTheEnd;


        public MainWindow()
        {
            InitializeComponent();
            // It have to be divider of 1000
            this.timerTimeSpan = 500;

            neuron0 = new Neuron(0);

            Grid.SetColumn(neuron0, 1);
            Grid.SetRow(neuron0, 1);
            gridModel1Main.Children.Add(neuron0);

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
            this.adjustTimer();
            this.newFlow = true;
        }

        private void adjustTimer()
        {

            drainingTimer = new System.Windows.Threading.DispatcherTimer();
            drainingTimer.Interval = TimeSpan.FromMilliseconds(this.timerTimeSpan);
            drainingTimer.Tick += (sender2, e1) =>
            {
                bool empty1 = neuron0.draining(this.drainingVolume);
                bool empty2 = neuron1.draining(this.drainingVolume);
                bool empty3 = neuron2.draining(this.drainingVolume);
                if (empty1 && empty2 && empty3)
                {
                    Console.WriteLine("Stop draining timer");
                    drainingTimer.Stop();
                }
            };

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(this.timerTimeSpan);

            timer.Tick += (sender2, e2) =>
            {
                this.myTimerTick(sender2, e2);
                this.neuronFlow(sender2, e2, this.neuron0, this.flow);
                this.neuronFlow(sender2, e2, this.neuron1, this.flow);
                this.neuronFlow(sender2, e2, this.neuron2, this.flow);
                counter += 1;
                if (counter >= this.tickThreshold)
                {
                    this.showResults(sender2, e2);
                }

            };
        }

        private void start_Click(object sender, RoutedEventArgs e)
        { 
            double newTime;
            int delay;
            this.color = System.Windows.Media.Brushes.DodgerBlue;
            neuron0.outFlowVolume = 0;
            neuron1.outFlowVolume = 0;
            neuron2.outFlowVolume = 0;
           
            reminderButton.IsEnabled = false;
            counter = 0;
            this.loadParams();

            if (!this.newFlow && timerTextBlock.Text != "00:00")
            {
                Console.WriteLine("In bad place");
                string[] seconds = timerTextBlock.Text.Split(':');
                delay = Int32.Parse(seconds[1].Trim(new Char[] { '0', }));
                newTime = time - delay;
                timer.Start();
                this.TimerStart = DateTime.Now.AddSeconds(-delay);
            }
            else
            {
                Console.WriteLine("In good place");
                this.tickThreshold = (int)(this.time * 1000 / this.timerTimeSpan);

                if ((this.flow > 0) && (time > 0))
                {
                    Console.WriteLine("Deeper");
                    startButton.IsEnabled = false;
                    this.TimerStart = DateTime.Now;

                    timer.Start();

                    this.newFlow = false;
                }
            }

        }


        private void neuronFlow(object sender, EventArgs e, Neuron neuron, double flow)
        {
            Console.WriteLine("Tick treshold" + this.tickThreshold);
            double toPush = 0;
            Console.WriteLine("Flow !" + flow);
            bool axonFull = neuron.axon.isFull && neuron.axon.blockTheEnd;
            Console.WriteLine("In neuron flow");
            if (neuron.dendrites_list.Count() == 0)
            {
                Console.WriteLine("Neuron 0");
                Tuple<bool, double> axRes = neuron.axon.newFlow(sender, e, flow, color);
                axonFull = neuron.axon.isFull && neuron.axon.blockTheEnd;
                neuron.volumeToPush = axRes.Item2;
                if (!axonFull && !remindStarted)
                {
                    Console.WriteLine("Out flow Volume" + neuron.outFlowVolume);
                    neuron.outFlowVolume += axRes.Item2;
                    Console.WriteLine("AXON $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$" + axRes.Item2);
                }

                return;
            }

            Console.WriteLine("Multiply neuron");
            foreach (Dendrite dendrite in neuron.dendrites_list)
            {
                if (!dendrite.isBlocked)
                {
                    Tuple<bool, double> dendriteRes = dendrite.newFlow(sender, e, flow, color);
                    if (dendriteRes.Item1)
                        toPush += dendriteRes.Item2;
                }
            }
            if (toPush > 0)
            {
                Console.WriteLine("Axon is full : " + axonFull);
                Tuple<bool, double> somaRes = neuron.soma.newFlow(sender, e, toPush, axonFull, color);
                if (somaRes.Item1 && !axonFull)
                {
                    Tuple<bool, double> axonRes = neuron.axon.newFlow(sender, e, somaRes.Item2, color);
                    axonFull = neuron.axon.isFull && neuron.axon.blockTheEnd;
                    if (!axonFull && !remindStarted)
                    {
                        Console.WriteLine("AXON ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^" + axonRes.Item2);
                        neuron.outFlowVolume += axonRes.Item2;
                    }
                }
                else if (somaRes.Item1 && axonFull)
                {
                    neuron.soma.newFlow(sender, e, somaRes.Item2, axonFull, color);
                }
                
            }
        }

        private void myTimerTick(object sender, EventArgs e)
        {
            if (counter < this.tickThreshold)
            {
                TimeSpan currentValue = DateTime.Now - this.TimerStart;
                this.timerTextBlock.Text = currentValue.ToString(@"mm\:ss");
            }
        }

        private void showResults(object sender, EventArgs e)
        {
            timer.Stop();
            neuron0.unload(this.remindStarted);
            neuron1.unload(this.remindStarted);
            neuron2.unload(this.remindStarted);

            M1VolumeBlock.Text = neuron0.outFlowVolume.ToString("0.00");
            M2VolumeBlock.Text = neuron1.outFlowVolume.ToString("0.00");
            M3VolumeBlock.Text = neuron2.outFlowVolume.ToString("0.00");

            Console.WriteLine("In show results");
            Console.WriteLine(neuron1.outFlowVolume);
            Console.WriteLine(neuron1.totalOutFlowVolume);
            Console.WriteLine(neuron1.outFlowVolume + Double.Parse(M2VolumeTotalBlock.Text));

            M1VolumeTotalBlock.Text = (neuron0.totalOutFlowVolume).ToString("0.00");
            M2VolumeTotalBlock.Text = (neuron1.totalOutFlowVolume).ToString("0.00");
            M3VolumeTotalBlock.Text = (neuron2.totalOutFlowVolume).ToString("0.00");

            startButton.IsEnabled = true;
            this.newFlow = true;
            if (!remindStarted)
                drainingTimer.Start();

            this.remindStarted = false;

            Console.WriteLine(counter);

        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            M1VolumeBlock.Text = "0";
            M2VolumeBlock.Text = "0";
            M3VolumeBlock.Text = "0";

            M1VolumeTotalBlock.Text = "0";
            M2VolumeTotalBlock.Text = "0";
            M3VolumeTotalBlock.Text = "0";

            timerTextBlock.Text = "00:00";

            neuron0.reset();
            neuron1.reset();
            neuron2.reset();

            this.newFlow = true;
            this.remindStarted = false;
            this.reminderButton.IsEnabled = true;
        }

        

        private void DragAndDropButton_Click(object sender, RoutedEventArgs e)
        {
            DragAndDropPanel myWindow = new DragAndDropPanel();

            myWindow.ShowDialog();

        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            drainingTimer.Stop();
            timer.Stop();
            startButton.IsEnabled = true;
            this.newFlow = false;
            this.remindStarted = false;
            this.reminderButton.IsEnabled = true;
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

    
        private void getConfParamsXML(string path, double time, double flow, double drainingSpeed)
        {
            this.currentConf = path;
            this.time = time;
            double divider = ((double)1000 / (double)this.timerTimeSpan);
            this.flow = flow / divider;
            this.drainingVolume = drainingSpeed / divider;
            Console.WriteLine("In main Window" + path);
        }

        private void loadParams()
        {
            List<double> neuron_params = new List<double>();
            Console.WriteLine("In load");
            XElement xmlTree = XElement.Load(this.currentConf, LoadOptions.None);
            foreach (XElement element in xmlTree.Elements())
            {
                string element_name = element.Name.ToString();
                if (element_name == "General")
                {
                    List<XElement> values_list = element.Elements().ToList();
                    this.flow = double.Parse(values_list[0].Value.ToString());
                    this.time = double.Parse(values_list[1].Value.ToString());
                    this.drainingVolume = double.Parse(values_list[2].Value.ToString());
                    this.blockTheEnd = values_list[3].Value.ToString() == "True" ? true : false;
                }
                else if (element_name == "Model1")
                {
                    neuron_params = element.Elements().Select(el => double.Parse(el.Value)).ToList();
                    this.setNeuronParams(neuron0, neuron_params);
                }
                else if (element_name == "Model2")
                {
                    neuron_params = element.Elements().Select(el => double.Parse(el.Value)).ToList();
                    this.setNeuronParams(neuron1, neuron_params);
                }
                else if (element_name == "Model3")
                {
                    neuron_params = element.Elements().Select(el => double.Parse(el.Value)).ToList();
                    this.setNeuronParams(neuron2, neuron_params);
                }
            }

            Console.WriteLine("Main Window" + this.blockTheEnd);
            if (blockTheEnd)
            {
                neuron0.axon.blockTheEnd = true;
                neuron1.axon.blockTheEnd = true;
                neuron2.axon.blockTheEnd = true;
            }
        }

        private void setNeuronParams(Neuron neuron, List<double> params_list)
        {
            List<Tuple<double, double>> denList = new List<Tuple<double, double>>();
            int params_length = params_list.Count();
            if (neuron.dendrites_list.Count() == 0)
            {
                Console.WriteLine("set params in neuron 0 ");
                neuron.SetParameters(new List<Tuple<double, double>>(), 0, params_list[1], params_list[0], false);
            }
            else if (neuron.dendrites_list.Count() > 0)
            {
                Console.WriteLine("set params in neuron 1 or 2 ");
                for (int i = 0; i < params_length - 4; i += 2)
                {
                    Tuple<double, double> denTuple = new Tuple<double, double>(params_list[i + 1], params_list[i]);
                    denList.Add(denTuple);
                }
                neuron.SetParameters(denList, params_list[params_length - 4], params_list[params_length - 3], params_list[params_length - 2], false);
            }

        }

        private void reminderButton_Click(object sender, RoutedEventArgs e)
        {
            this.drainingTimer.Stop();
            this.blockTheEnd = false;
            this.counter = 0;
            this.color = System.Windows.Media.Brushes.Maroon;
            this.timerTextBlock.Text = "00:00";
            this.TimerStart = DateTime.Now;
            this.timer.Start();
            this.remindStarted = true;
            this.reminderButton.IsEnabled = false;
            this.startButton.IsEnabled = false;
        }

    }


}