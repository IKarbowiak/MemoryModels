﻿using System;
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
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;
using PracaMagisterska.HTM;
using PracaMagisterska.PersonalSolution;

namespace PracaMagisterska
{
    public partial class FlowModel : Window
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
        private System.Windows.Media.SolidColorBrush color = System.Windows.Media.Brushes.DodgerBlue;
        private bool remindStarted = false;
        private bool blockTheEnd;
        private List<double> timeThresholdForMemoryStorage = new List<double>();
        private Dictionary<Neuron, double> timeBegginingOfOutflowInReminder = new Dictionary<Neuron, double>();
        private Dictionary<Neuron, double> startOutFlowTime = new Dictionary<Neuron, double>();
        private int drainingCounter = 0;
        private int simulationNumber = 0;

        // set main parameters and create neurons objects
        public FlowModel()
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

        // create timer objects, adjust therir interval and functions - what they will do in every tick
        private void adjustTimer()
        {
            drainingTimer = new System.Windows.Threading.DispatcherTimer();
            drainingTimer.Interval = TimeSpan.FromMilliseconds(this.timerTimeSpan);
            drainingTimer.Tick += (sender2, e1) =>
            {
                this.drainingCounter += 1;
                double remainingMemory = this.getRemainingMemory();
                bool empty1 = neuron0.draining(remainingMemory);
                bool empty2 = neuron1.draining(remainingMemory);
                bool empty3 = neuron2.draining(remainingMemory);
                if (empty1 && empty2 && empty3)
                {
                    drainingTimer.Stop();
                }
            };

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(this.timerTimeSpan);
            int moreTicks = 0;

            timer.Tick += (sender2, e2) =>
            {
                counter += 1;
                this.myTimerTick(sender2, e2);
                this.neuronFlow(sender2, e2, this.neuron0, this.flow);
                this.neuronFlow(sender2, e2, this.neuron1, this.flow);
                this.neuronFlow(sender2, e2, this.neuron2, this.flow);
                if (counter >= this.tickThreshold && !remindStarted)
                {
                    this.showResults(sender2, e2);
                }
                if (remindStarted && this.timeBegginingOfOutflowInReminder.Count() == 2)
                {
                    if (moreTicks >= 5)
                        this.showResults(sender2, e2);
                    else
                        moreTicks += 1;
                }

            };
        }

        public double getRemainingMemory()
        {
            double remainingMemory = Math.Exp(-((double)this.drainingCounter * (double)this.timerTimeSpan / 1000) / (double)this.simulationNumber);
            if (remainingMemory < 0.01)
                remainingMemory = 0;
            return remainingMemory;
        }

        private void setCurrentConf()
        {
            Regex pattern = new Regex("(.*)(\\\\bin\\\\Debug)", RegexOptions.Compiled);
            Match match = pattern.Match(Directory.GetCurrentDirectory());
            GroupCollection groups = match.Groups;
            this.currentConf = groups[1].Value + "\\defaultConf.xml";
        }

        // starts the simulation - starts timer
        private void start_Click(object sender, RoutedEventArgs e)
        {
            double newTime;
            int delay;
            this.color = System.Windows.Media.Brushes.DodgerBlue;
            this.simulationNumber += 1;
            neuron0.outFlowVolume = 0;
            neuron1.outFlowVolume = 0;
            neuron2.outFlowVolume = 0;
            this.clear_params();

            reminderButton.IsEnabled = false;
            counter = 0;
            if (currentConf == null)
            {
                this.setCurrentConf();
            }
            this.loadParams();
            this.calculateTimeOfOutFlow();
            if (!this.newFlow && timerTextBlock.Text != "00:00")
            {
                string[] seconds = timerTextBlock.Text.Split(':');
                delay = Int32.Parse(seconds[1].Trim(new Char[] { '0', }));
                newTime = time - delay;
                timer.Start();
                this.TimerStart = DateTime.Now.AddSeconds(-delay);
            }
            else
            {
                this.tickThreshold = (int)(this.time * 1000 / this.timerTimeSpan);

                if ((this.flow > 0) && (time > 0))
                {
                    startButton.IsEnabled = false;
                    this.TimerStart = DateTime.Now;

                    timer.Start();

                    this.newFlow = false;
                }
            }

        }

        // function that manages the flow of fluids through neurons
        private void neuronFlow(object sender, EventArgs e, Neuron neuron, double flow)
        {
            double toPush = 0;
            bool axonFull = neuron.axon.isFull && neuron.axon.blockTheEnd;
            if (neuron.dendrites_list.Count() == 0)
            {
                if (neuron.axon.maxSpeed < flow)
                {
                    flow = neuron.axon.maxSpeed;
                }
                Tuple<bool, double> axRes = neuron.axon.newFlow(sender, e, flow, color);
                axonFull = neuron.axon.isFull && neuron.axon.blockTheEnd;
                neuron.volumeToPush = axRes.Item2;
                if (!axonFull && !remindStarted)
                {
                    neuron.outFlowVolume += axRes.Item2;
                }
                if (axRes.Item2 > 0 && !this.startOutFlowTime.ContainsKey(neuron))
                {
                    this.startOutFlowTime[neuron] = ((double)counter * (double)this.timerTimeSpan) / 1000;
                }

                return;
            }

            foreach (Dendrite dendrite in neuron.dendrites_list)
            {
                if (!dendrite.isBlocked)
                {
                    Tuple<bool, double> dendriteRes = dendrite.newFlow(sender, e, flow, color);
                    if (dendriteRes.Item1)
                        toPush += dendriteRes.Item2;
                }
                if (remindStarted)
                    break;
            }
            if (toPush > 0)
            {
                Tuple<bool, double> somaRes = neuron.soma.newFlow(sender, e, toPush, axonFull, color);
                if (somaRes.Item1 && !axonFull)
                {
                    double pushValue = somaRes.Item2;
                    double additionalVolume = 0;
                    if (!remindStarted && pushValue > neuron.axon.maxSpeed)
                    {
                        additionalVolume = pushValue - neuron.axon.maxSpeed;
                        pushValue = neuron.axon.maxSpeed;
                    }

                    if (additionalVolume > 0)
                    {
                        neuron.soma.newFlow(sender, e, additionalVolume, true, color);
                    }

                    Tuple<bool, double> axonRes = neuron.axon.newFlow(sender, e, pushValue, color);
                    axonFull = neuron.axon.isFull && neuron.axon.blockTheEnd;
                    if (!axonFull && !remindStarted)
                    {
                        neuron.outFlowVolume += axonRes.Item2;
                        if (axonRes.Item2 > 0 && !this.startOutFlowTime.ContainsKey(neuron))
                        {
                            this.startOutFlowTime[neuron] = ((double)counter * (double)this.timerTimeSpan) / 1000;
                        }
                    }
                    else if (remindStarted)
                    {
                        if (axonRes.Item2 > 0 && !this.timeBegginingOfOutflowInReminder.ContainsKey(neuron))
                        {
                            this.timeBegginingOfOutflowInReminder[neuron] = ((double)counter * (double)this.timerTimeSpan) / 1000;
                        }
                    }
                }
                else if (somaRes.Item1 && axonFull)
                {
                    neuron.soma.newFlow(sender, e, somaRes.Item2, axonFull, color);
                }
            }
        }

        // change time in the stopwatch
        private void myTimerTick(object sender, EventArgs e)
        {
            if (counter < this.tickThreshold)
            {
                TimeSpan currentValue = DateTime.Now - this.TimerStart;
                this.timerTextBlock.Text = currentValue.ToString(@"mm\:ss");
            }
        }

        // show results in the table in main window when flow stopped
        private void showResults(object sender, EventArgs e)
        {
            timer.Stop();
            neuron0.unload(this.remindStarted);
            neuron1.unload(this.remindStarted);
            neuron2.unload(this.remindStarted);

            M1VolumeBlock.Text = neuron0.outFlowVolume.ToString("0.00");
            M2VolumeBlock.Text = neuron1.outFlowVolume.ToString("0.00");
            M3VolumeBlock.Text = neuron2.outFlowVolume.ToString("0.00");

            M1VolumeTotalBlock.Text = (neuron0.totalOutFlowVolume).ToString("0.00");
            M2VolumeTotalBlock.Text = (neuron1.totalOutFlowVolume).ToString("0.00");
            M3VolumeTotalBlock.Text = (neuron2.totalOutFlowVolume).ToString("0.00");

            startButton.IsEnabled = true;
            this.newFlow = true;
            if (!remindStarted)
                drainingTimer.Start();

            this.remindStarted = false;
        }

        private void clear_params()
        {
            this.drainingCounter = 0;
            this.newFlow = true;
            this.remindStarted = false;
            this.reminderButton.IsEnabled = true;
            this.timeBegginingOfOutflowInReminder = new Dictionary<Neuron, double>();
            this.startOutFlowTime = new Dictionary<Neuron, double>();

            neuron0.reset();
            neuron1.reset();
            neuron2.reset();
        }

        // reset flow and results table value after 'Reset' button click
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            M1VolumeBlock.Text = "0";
            M2VolumeBlock.Text = "0";
            M3VolumeBlock.Text = "0";

            M1VolumeTotalBlock.Text = "0";
            M2VolumeTotalBlock.Text = "0";
            M3VolumeTotalBlock.Text = "0";

            timerTextBlock.Text = "00:00";
            this.simulationNumber = 0;

            this.clear_params();
        }

        // open drag and drop button click
        private void DragAndDropButton_Click(object sender, RoutedEventArgs e)
        {
            DragAndDropPanel myWindow = new DragAndDropPanel();

            myWindow.ShowDialog();

        }

        // stops the timers which stop the flow after 'Stop' button click
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            drainingTimer.Stop();
            timer.Stop();
            startButton.IsEnabled = true;
            this.newFlow = false;
            this.remindStarted = false;
            this.reminderButton.IsEnabled = true;
        }

        // open window for setting parameters
        private void setParamButton_Click(object sender, RoutedEventArgs e)
        {
            SetParametersWindow setParamsWindow;
            if (currentConf != null)
            {
                setParamsWindow = new SetParametersWindow(this.getConfParamsXML, this, this.currentConf);
            }
            else
            {
                setParamsWindow = new SetParametersWindow(this.getConfParamsXML, this);
            }
            setParamsWindow.ShowDialog();
        }

        // function which is used after closing window with parametersm adjust time and speed of flow
        private void getConfParamsXML(string path, double time, double flow)
        {
            this.currentConf = path;
            this.time = time;
            this.setFlowVolume(flow);
        }

        private void setFlowVolume(double flowValue)
        {
            double divider = ((double)1000 / (double)this.timerTimeSpan);
            this.flow = flowValue / divider;
        }

        // apply the value of parameters from xml 
        private void loadParams()
        {
            List<double> neuron_params = new List<double>();
            XElement xmlTree = XElement.Load(this.currentConf, LoadOptions.None);
            foreach (XElement element in xmlTree.Elements())
            {
                List<XElement> values_list = element.Elements().ToList();
                string element_name = element.Name.ToString();
                if (element_name == "General")
                {
                    this.setFlowVolume(double.Parse(values_list[0].Value.ToString()));
                    this.time = double.Parse(values_list[1].Value.ToString());
                    this.blockTheEnd = values_list[2].Value.ToString() == "True" ? true : false;
                }
                else if (element_name == "Model1")
                {
                    bool neuron_damage = bool.Parse(values_list[values_list.Count - 1].Value);
                    neuron_params = values_list.GetRange(0, values_list.Count - 1).Select(el => double.Parse(el.Value)).ToList();
                    this.setNeuronParams(neuron0, neuron_params, neuron_damage);
                }
                else if (element_name == "Model2")
                {
                    bool neuron_damage = bool.Parse(values_list[values_list.Count - 1].Value);
                    neuron_params = values_list.GetRange(0, values_list.Count - 1).Select(el => double.Parse(el.Value)).ToList();
                    this.setNeuronParams(neuron1, neuron_params, neuron_damage);
                }
                else if (element_name == "Model3")
                {
                    bool neuron_damage = bool.Parse(values_list[values_list.Count - 1].Value);
                    neuron_params = values_list.GetRange(0, values_list.Count - 1).Select(el => double.Parse(el.Value)).ToList();
                    this.setNeuronParams(neuron2, neuron_params, neuron_damage);
                }
            }

            if (blockTheEnd)
            {
                neuron0.axon.blockTheEnd = true;
                neuron1.axon.blockTheEnd = true;
                neuron2.axon.blockTheEnd = true;
            }
        }

        // set params from xml to neuron, function is used by loadParams()
        private void setNeuronParams(Neuron neuron, List<double> params_list, bool damage)
        {
            double divider = ((double)1000 / (double)this.timerTimeSpan);
            List<Tuple<double, double>> denList = new List<Tuple<double, double>>();
            int params_length = params_list.Count();
            if (neuron.dendrites_list.Count() == 0)
            {
                neuron.SetParameters(new List<Tuple<double, double>>(), 0, params_list[1], params_list[0], false, params_list[2] / divider, damage);
            }
            else if (neuron.dendrites_list.Count() > 0)
            {
                for (int i = 0; i < params_length - 4; i += 2)
                {
                    Tuple<double, double> denTuple = new Tuple<double, double>(params_list[i + 1], params_list[i]);
                    denList.Add(denTuple);
                }
                neuron.SetParameters(denList, params_list[params_length - 4], params_list[params_length - 3], params_list[params_length - 2], false, params_list[params_length - 1] / divider, damage);
            }

        }

        // start remaining simulation after 'Remindee' button click
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

        // calculate the time flow below which, it is said that something was in neuron
        private void calculateTimeOfOutFlow()
        {
            foreach (Neuron neuron in new List<Neuron> { neuron1, neuron2 })
            {
                double time = Math.Ceiling((neuron.minVolumeToOutflow / this.flow)) * this.timerTimeSpan / 1000;
                this.timeThresholdForMemoryStorage.Add(time);
            }
        }

        // create results window, add results values to results window and open the window
        private void resultsButton_Click(object sender, RoutedEventArgs e)
        {
            Results resultsWindow = new Results();
            resultsWindow.outFlowTimeTextBlockM0.Text = this.startOutFlowTime.ContainsKey(neuron0) ? this.startOutFlowTime[neuron0].ToString() : "0";
            resultsWindow.outFlowTimeTextBlockM1.Text = this.startOutFlowTime.ContainsKey(neuron1) ? this.startOutFlowTime[neuron1].ToString() : "0";
            resultsWindow.outFlowTimeTextBlockM2.Text = this.startOutFlowTime.ContainsKey(neuron2) ? this.startOutFlowTime[neuron2].ToString() : "0";
            resultsWindow.outFlowVolumeTextBlockM0.Text = this.neuron0.outFlowVolume.ToString("0.00");
            resultsWindow.outFlowVolumeTextBlockM1.Text = this.neuron1.outFlowVolume.ToString("0.00");
            resultsWindow.outFlowVolumeTextBlockM2.Text = this.neuron2.outFlowVolume.ToString("0.00");
            resultsWindow.reminderOutFlowTimeTextBlockM1.Text = this.timeBegginingOfOutflowInReminder.ContainsKey(neuron1) ? this.timeBegginingOfOutflowInReminder[neuron1].ToString() : "0";
            resultsWindow.reminderOutFlowTimeTextBlockM2.Text = this.timeBegginingOfOutflowInReminder.ContainsKey(neuron2) ? this.timeBegginingOfOutflowInReminder[neuron2].ToString() : "0";
            if (this.timeBegginingOfOutflowInReminder.Count() == 2 && this.timeThresholdForMemoryStorage.Count() == 2)
            {
                resultsWindow.somethingRememberesTextBlockM1.Text = this.timeBegginingOfOutflowInReminder[neuron1] < this.timeThresholdForMemoryStorage[0] ? "True" : "False";
                resultsWindow.somethingRememberesTextBlockM2.Text = this.timeBegginingOfOutflowInReminder[neuron2] < this.timeThresholdForMemoryStorage[1] ? "True" : "False";
            }
            else
            {
                resultsWindow.somethingRememberesTextBlockM1.Text = "False";
                resultsWindow.somethingRememberesTextBlockM2.Text = "False";
            }
            resultsWindow.ShowDialog();
        }
    }
}
