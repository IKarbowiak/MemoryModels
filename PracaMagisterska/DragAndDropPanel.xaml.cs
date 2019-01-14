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
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using System.IO;

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 

    public partial class DragAndDropPanel : Window
    {
        private double maxX;
        private double maxY;
        private double[] startPosition;
        public Dictionary<Viewbox, double[]> canvasElements { get; set; }
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer drainingTimer;
        private List<List<Viewbox>> neuronQueue = new List<List<Viewbox>>();
        private int counter = 0;
        private string currentConf;
        private double flowVolume;
        private double flowTime;
        private bool blockTheEnd = false;
        private DateTime TimerStart;
        private bool pauseFlow = false;
        private TimeSpan timeOffset;
        private int tickThreshold;
        private int timerTimeSpan;
        private List<Neuron> neuronsToCloseDendrites = new List<Neuron>();
        private double drainingVolume;
        private System.Windows.Media.SolidColorBrush color = System.Windows.Media.Brushes.DodgerBlue;
        private bool remindStarted = false;
        private double timeBegginingOfOutflowInReminder;
        private double startOutFlowTime = 0;
        private bool somethingInNeuron = false;
        private double totalOutFlow = 0;
        private double minTimeToOutFlow = 0;
        private double maxSomaVolumeInQueue = 0;
        private int queueNumberForReminder;
        private int somaAmount;

        // set main parameters
        public DragAndDropPanel()
        {
            InitializeComponent();
            this.timerTimeSpan = 200;
            canvasElements = new Dictionary<Viewbox, double[]>();
            this.adjustTimer();
            this.flowTime = 40;
            this.flowVolume = 12;
            this.createVieboxWithNeuron(0);
            this.createVieboxWithNeuron(1);
            this.createVieboxWithNeuron(2);

        }

        // create timer objects, adjust therir interval and functions - what they will do in every tick
        private void adjustTimer()
        {

            // timer for flow
            this.timer = new System.Windows.Threading.DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(this.timerTimeSpan);
            int moreTicks = 0;

            this.timer.Tick += (sender1, e1) =>
            {
                this.myTimerTick(sender1, e1);
                this.flow(sender1, e1, flowVolume);
                counter += 1;
                if (counter >= this.tickThreshold && !this.remindStarted)
                {
                    this.stop(true);
                }

                if (remindStarted && this.timeBegginingOfOutflowInReminder > 0)
                {
                    if (moreTicks >= 5)
                        this.stop(true);
                    else
                        moreTicks += 1;
                }

            };

            // timer for draining
            drainingTimer = new System.Windows.Threading.DispatcherTimer();
            drainingTimer.Interval = TimeSpan.FromMilliseconds(this.timerTimeSpan);
            drainingTimer.Tick += (sender2, e1) =>
            {
                List<bool> emptyResults = new List<bool>();
                foreach (Viewbox viewbox in this.canvasElements.Keys)
                {
                    Neuron neuron = (Neuron)viewbox.Child;
                    bool empty = neuron.draining(this.drainingVolume);
                    emptyResults.Add(empty);
                }
                if (!emptyResults.Contains(false))
                {
                    Console.WriteLine("Stop draining timer");
                    Console.WriteLine("Stop draining timer");
                    drainingTimer.Stop();
                }
            };
        }

        // create neurons in the left panel, which can be click to create duplicated object in neuron panel
        private void createVieboxWithNeuron(int dendNumber)
        {
            TextBlock modelName = new TextBlock() { TextAlignment = TextAlignment.Center, Text = "Model " + dendNumber };
            objectHandlerPanel.Children.Add(modelName);

            Viewbox viewbox = new Viewbox() { Name = "n" + dendNumber, StretchDirection = StretchDirection.Both, Stretch = Stretch.Uniform };
            Neuron newNeuron = new Neuron(dendNumber);
            newNeuron.Height = 150;
            newNeuron.Width = 450;
            viewbox.Child = newNeuron;
            viewbox.MouseDown += new MouseButtonEventHandler(this.create_neuron);

            objectHandlerPanel.Children.Add(viewbox);

        }

        // check if matrix contain specific array of value, return True if contain
        private bool checkDictValue(double[][] values, double[] compareArray)
        {
            foreach (double[] value in values)
            {
                if (Enumerable.SequenceEqual(value, compareArray))
                {
                    return true;
                }
            }
            return false;
        }

        // add mause up an mause move function to element when it will be clicked and set starting position of clicked object
        private void neuron_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Viewbox viewbox = (Viewbox)sender;
            maxX = dropCanvas.ActualWidth - viewbox.Width;
            maxY = dropCanvas.ActualHeight - viewbox.Height;

            this.startPosition = new double[] { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };

            viewbox.CaptureMouse();
            Console.WriteLine("In Mouse Down");
            viewbox.MouseMove += neuron_MouseMove;
            viewbox.MouseUp += neuron_MouseUp;
        }

        // adjust current position of element during move, prevent from moving out of border
        private void neuron_MouseMove(object sender, MouseEventArgs e)
        {
            Viewbox viewbox = (Viewbox)sender;
            var pos = e.GetPosition(dropCanvas);
            var newX = pos.X - (viewbox.Width / 2);
            var newY = pos.Y - (viewbox.Height / 2);

            if (newX < 0) newX = 0;
            if (newX > maxX) newX = maxX;

            if (newY < 0) newY = 0;
            if (newY > maxY) newY = maxY;

            viewbox.SetValue(Canvas.LeftProperty, newX);
            viewbox.SetValue(Canvas.RightProperty, newX + viewbox.Width);
            viewbox.SetValue(Canvas.TopProperty, newY);
            viewbox.SetValue(Canvas.BottomProperty, newY + viewbox.Height);

        }

        // check if neuron Queue contains specific element
        // return position in queue of searching element if found, else return empty list
        private List<int> neuronQueueContainsCheck(object element)
        {
            List<int> res_List = new List<int>();

            if (this.neuronQueue.Count > 0)
            {
                for (int i = 0; i < this.neuronQueue.Count(); i++)
                {
                    for (int j = 0; j < this.neuronQueue[i].Count(); j++)
                    {
                        if (this.neuronQueue[i][j] == element)
                        {
                            res_List.Add(i);
                            res_List.Add(j);
                            Console.WriteLine("Contains " + i + " " + j);
                        }
                    }
                }
            }
            return res_List;
        }

        // add neuron to the left side of queue
        private void addToLeft(List<int> checkEl, Viewbox viewbox)
        {
            if (checkEl[1] > 0)
            {
                List<Viewbox> queue = this.neuronQueue[checkEl[0]];
                List<Viewbox> elements = queue.GetRange(checkEl[1], queue.Count() - checkEl[1]);
                elements.Insert(0, viewbox);
                this.neuronQueue.Add(elements);
                return;
            }
            this.neuronQueue[checkEl[0]].Insert(checkEl[1], viewbox);
        }

        // add neuron to the right side of queue
        private void addToRight(List<int> checkEl, Viewbox viewbox)
        {
            if (checkEl.Count() > 2)
            {
                for (int j = 0; j < checkEl.Count(); j += 2)
                {
                    this.neuronQueue[checkEl[j]].Insert(checkEl[j + 1] + 1, viewbox);
                }
                return;
            }
            this.neuronQueue[checkEl[0]].Insert(checkEl[1] + 1, viewbox);
        }

        // link moving neuron to queue
        // check if neuron is out of border, 
        private bool linkLeftOrRight(MouseButtonEventArgs e, Viewbox viewbox, String site, KeyValuePair<Viewbox, Double[]> element)
        {
            var pos = e.GetPosition(this.dropCanvas);
            bool outOfBorder = checkIfQuitBorder(pos.X, pos.Y, viewbox);
            double[] newPosition = new double[] { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };
            // Check if something is on this place if is, come back to previous position
            if (!outOfBorder && this.checkDictValue(this.canvasElements.Values.ToArray(), newPosition))
            {
                this.backToPreviousPosition(viewbox);
                // true because neuron does not change position
                return true;
            }
            else if (!outOfBorder)
            {
                // get position of element to which the neuron should be linked
                List<int> checkEl = neuronQueueContainsCheck(element.Key);

                // do if neuron queue is empty
                if (this.neuronQueue.Count() == 0)
                {
                    List<Viewbox> elements = new List<Viewbox>();
                    if (site == "left")
                    {
                        Viewbox elBox = (Viewbox)element.Key;
                        elements.Add(viewbox);
                        elements.Add(elBox);
                        this.neuronQueue.Add(elements);
                    }
                    else
                    {
                        elements.Add(element.Key);
                        elements.Add(viewbox);
                        this.neuronQueue.Add(elements);
                    }
                }
                // do if neuron queue is not empty
                else if (checkEl.Count() > 0)
                {
                    if (site == "left")
                        this.addToLeft(checkEl, viewbox);
                    else
                        this.addToRight(checkEl, viewbox);
                }
                return true;
            }
            return false;
        }

        // remove neuron from queue
        private void checkIfNeuronLeaveQueue(Viewbox viewbox)
        {
            List<int> checkList = neuronQueueContainsCheck(viewbox);
            if (checkList.Count == 0)
                return;

            int list_num = checkList[0];
            int elInList = checkList[1];
            Console.WriteLine("Delete from List");

            if (elInList > this.neuronQueue[list_num].Count / 2)
            {
                for (int x = elInList; x < this.neuronQueue[list_num].Count(); x++)
                {
                    this.neuronQueue[list_num].RemoveAt(x);
                }
            }
            else
            {
                for (int x = elInList; x > -1; x--)
                {
                    this.neuronQueue[list_num].RemoveAt(x);

                }
            }

        }

        // set connection to specific dendrit in neuron with more than one dendrite
        private void setConnectionToDen(Viewbox viewbox, string direction, Viewbox element, string side)
        {
            string newDirection = " ";
            if (side == "left")
            {
                viewbox.Name = direction;
            }
            else if (side == "right")
            {
                newDirection = direction == "up" ? "down" : "up";
                element.Name = newDirection;
            }
        }

        // set top and bottom neuron poisition
        private void set_TopAndBottom_Property(Viewbox viewbox, KeyValuePair<Viewbox, Double[]> element, bool condition, string side, double offset)
        {
            if (condition)
            {
                if ((element.Value[2] - Canvas.GetTop(viewbox)) <= 0)
                {
                    Console.WriteLine("Down");
                    viewbox.SetValue(Canvas.TopProperty, element.Value[2] + offset);
                    viewbox.SetValue(Canvas.BottomProperty, element.Value[3] + offset);
                    setConnectionToDen(viewbox, "down", element.Key, side);
                }
                else
                {
                    Console.WriteLine("Up");
                    viewbox.SetValue(Canvas.TopProperty, element.Value[2] - offset);
                    viewbox.SetValue(Canvas.BottomProperty, element.Value[3] - offset);
                    setConnectionToDen(viewbox, "up", element.Key, side);
                }
            }
            else
            {
                viewbox.SetValue(Canvas.TopProperty, element.Value[2]);
                viewbox.SetValue(Canvas.BottomProperty, element.Value[3]);
            }

        }

        // try to link neuron to queue after mouse up from neuron
        private void neuron_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Viewbox viewbox = (Viewbox)sender;
            int catchValue_rightleft = 30;
            int catchValue_updown = 15;
            double offset = 11.5;
            Neuron neuron = (Neuron)viewbox.Child;
            Console.WriteLine("In mouse up ");


            viewbox.ReleaseMouseCapture();
            viewbox.MouseMove -= neuron_MouseMove;
            viewbox.MouseUp -= neuron_MouseUp;

            // link neuron to queue
            bool linkNeuron()
            {
                foreach (KeyValuePair<Viewbox, Double[]> element in canvasElements)
                {

                    if ((element.Key != viewbox) && ((Math.Abs(element.Value[2] - Canvas.GetTop(viewbox)) <= catchValue_updown) ||
                        (Math.Abs(element.Value[3] - Canvas.GetBottom(viewbox)) <= catchValue_updown)))
                    {
                        bool results = false;
                        Viewbox elBox = (Viewbox)element.Key;
                        // check if neuron is near to the left side of queue
                        double val = Math.Abs(element.Value[0] - Canvas.GetRight(viewbox));
                        double val2 = Math.Abs(element.Value[1] - Canvas.GetLeft(viewbox));
                        if (Math.Abs(element.Value[0] - Canvas.GetRight(viewbox)) <= catchValue_rightleft)
                        {
                            set_TopAndBottom_Property(viewbox, element, ((Neuron)elBox.Child).dendrites_list.Count() > 1, "left", offset);

                            viewbox.SetValue(Canvas.LeftProperty, element.Value[0] - viewbox.Width - 1);
                            viewbox.SetValue(Canvas.RightProperty, element.Value[0] - 1);

                            results = this.linkLeftOrRight(e, viewbox, "left", element);
                            return results;
                        }
                        // check if neuron is near to the right side of queue
                        else if (Math.Abs(element.Value[1] - Canvas.GetLeft(viewbox)) <= catchValue_rightleft)
                        {
                            set_TopAndBottom_Property(viewbox, element, (neuron.dendrites_list.Count() > 1), "right", offset);

                            viewbox.SetValue(Canvas.LeftProperty, element.Value[1] + 1);
                            viewbox.SetValue(Canvas.RightProperty, element.Value[1] + viewbox.Width + 1);

                            results = this.linkLeftOrRight(e, viewbox, "right", element);
                            return results;
                        }
                    }
                }
                return false;
            };

            bool linked = linkNeuron();
            List<int> checkList = neuronQueueContainsCheck(viewbox);

            if (!linked)
            {
                //TODO: function to finish
                this.checkIfNeuronLeaveQueue(viewbox);
            }

            double[] parameters = { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };
            canvasElements[(Viewbox)sender] = parameters;


            Console.WriteLine("List count " + this.neuronQueue.Count());
            Console.WriteLine(canvasElements[(Viewbox)sender][0]);
            Console.WriteLine("Dictionary count !!!!! " + canvasElements.Count());

            // Check to remove 
            counter = 0;
            foreach (List<Viewbox> el in this.neuronQueue)
            {
                Console.WriteLine("List " + counter);
                Console.WriteLine(el.Count());
                counter++;
            }
        }


        // back to the start position of move
        private void backToPreviousPosition(Viewbox viewbox)
        {
            viewbox.SetValue(Canvas.LeftProperty, this.startPosition[0]);
            viewbox.SetValue(Canvas.RightProperty, this.startPosition[1]);
            viewbox.SetValue(Canvas.TopProperty, this.startPosition[2]);
            viewbox.SetValue(Canvas.BottomProperty, this.startPosition[3]);
        }

        // check if neuron quit border of neuron panel
        private bool checkIfQuitBorder(double x, double y, Viewbox viewbox)
        {
            double newX = x - (viewbox.Width / 2);
            double newY = y - (viewbox.Height / 2);

            bool quit = false;
            if (newX < 0) { newX = 0; quit = true; }
            if (newX > maxX) { newX = maxX; quit = true; }

            if (newY < 0) { newY = 0; quit = true; }
            if (newY > maxY) { newY = maxY; quit = true; }

            if (quit)
            {
                viewbox.SetValue(Canvas.LeftProperty, newX);
                viewbox.SetValue(Canvas.RightProperty, newX + viewbox.Width);
                viewbox.SetValue(Canvas.TopProperty, newY);
                viewbox.SetValue(Canvas.BottomProperty, newY + viewbox.Height);
            }

            return quit;

        }

        // start flow simulation after 'Start' button clicked
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (canvasElements.Count() == 0)
            {

                return;
            }
            this.startOutFlowTime = 0;
            this.timeBegginingOfOutflowInReminder = 0;
            this.reminderButton.IsEnabled = false;
            this.somethingInNeuron = false;
            // if flow was paused
            if (this.pauseFlow)
            {
                TimeSpan delay = TimeSpan.Parse("00:" + timerTextBlock.Text);
                TimeSpan currentValue = DateTime.Now - this.TimerStart;
                this.timeOffset = currentValue - delay;
                Console.WriteLine("******************************************************************");
                Console.WriteLine(this.flowTime - delay.Seconds);
                this.timer.Start();
                startButton.IsEnabled = false;
                return;
            }

            Console.WriteLine("Current path: " + this.currentConf);
            if (currentConf == null)
            {
                string projectPath = string.Join("\\", Directory.GetCurrentDirectory().Split('\\').Take(4).ToArray());
                this.currentConf = projectPath + "\\defaultConf.xml";
            }
            this.loadParams();
            Console.WriteLine(this.flowVolume);
            this.blockViewboxMoving();

            counter = 0;
            double divider = ((double)1000 / (double)this.timerTimeSpan);
            this.flowVolume = this.flowVolume / divider;
            this.drainingVolume = this.drainingVolume / divider;

            this.calculateTimeOfOutFlow();

            Console.WriteLine("Flow Volume!!! Drag nad Drop!!!!" + this.flowVolume);
            if (canvasElements.Count() > 0)
            {
                this.tickThreshold = (int)(this.flowTime * 1000 / this.timerTimeSpan);
                this.TimerStart = DateTime.Now;
                timer.Start();
                startButton.IsEnabled = false;
            }
        }

        // apply value of parameters from xml file
        private void loadParams()
        {
            List<double> neuron0_params = new List<double>();
            List<double> neuron1_params = new List<double>();
            List<double> neuron2_params = new List<double>();
            Console.WriteLine("In load");
            XElement xmlTree = XElement.Load(this.currentConf, LoadOptions.None);
            foreach (XElement element in xmlTree.Elements())
            {
                string element_name = element.Name.ToString();
                if (element_name == "General")
                {
                    List<XElement> values_list = element.Elements().ToList();
                    this.flowVolume = double.Parse(values_list[0].Value.ToString());
                    this.flowTime = double.Parse(values_list[1].Value.ToString());
                    this.drainingVolume = double.Parse(values_list[2].Value.ToString());
                    this.blockTheEnd = values_list[3].Value.ToString() == "True" ? true : false;
                }
                else if (element_name == "Model1")
                {
                    neuron0_params = element.Elements().Select(el => double.Parse(el.Value)).ToList();
                }
                else if (element_name == "Model2")
                {
                    neuron1_params = element.Elements().Select(el => double.Parse(el.Value)).ToList();
                }
                else if (element_name == "Model3")
                {
                    neuron2_params = element.Elements().Select(el => double.Parse(el.Value)).ToList();
                }
            }

            foreach (Viewbox element in canvasElements.Keys)
            {
                Neuron neuron = (Neuron)element.Child;
                if (neuron.dendrites_list.Count() == 0)
                {
                    this.setNeuronParams(neuron, neuron0_params);
                }
                else if (neuron.dendrites_list.Count() == 1)
                {
                    this.setNeuronParams(neuron, neuron1_params);
                }
                else if (neuron.dendrites_list.Count() == 2)
                {
                    this.setNeuronParams(neuron, neuron2_params);
                }
            }

            Console.WriteLine("Drag and drop " + this.blockTheEnd);
            if (blockTheEnd)
            {
                foreach (List<Viewbox> viewbox_list in this.neuronQueue)
                {
                    ((Neuron)viewbox_list[viewbox_list.Count - 1].Child).axon.blockTheEnd = true;
                    Console.WriteLine("set axon to block");
                }
            }


        }

        // set params from xml to neuron, function is used by loadParams()
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

        // block moving neurons during flow
        private void blockViewboxMoving()
        {
            foreach (Viewbox element in this.canvasElements.Keys)
            {
                element.MouseDown -= new MouseButtonEventHandler(this.neuron_MouseDown);
            }
        }

        // unblock moving neurons
        private void enableViewboxMoving()
        {
            foreach (Viewbox element in this.canvasElements.Keys)
            {
                element.MouseDown += new MouseButtonEventHandler(this.neuron_MouseDown);
            }
        }

        // main function which push fluid to neurons
        private void flow(object sender, EventArgs e, double flow)
        {
            Dictionary<Neuron, List<double>> whatToPush = new Dictionary<Neuron, List<double>>();
            Console.WriteLine("Count whatToPush" + whatToPush.Count());

            // if there is no queue but any neuron in panel exist
            if (this.neuronQueue.Count() == 0 && this.canvasElements.Count > 0)
            {
                foreach (KeyValuePair<Viewbox, double[]> element in this.canvasElements)
                {
                    Neuron neuron = (Neuron)element.Key.Child;
                    if (neuron.dendrites_list.Count() > 1)
                    {
                        foreach (Dendrite den in neuron.dendrites_list)
                            den.isBlocked = false;
                        this.addToNeuronsToCloseDendriteList(neuron);
                    }
                    whatToPush[neuron] = new List<double> { flow };
                }
            }

            for (int i = 0; i < this.neuronQueue.Count(); i++)
            {
                if (this.startOutFlowTime == 0 && this.remindStarted && i != this.queueNumberForReminder)
                {
                    continue;
                }
                Console.WriteLine("In flow 2");
                if (this.neuronQueue[i].Count() > 0)
                {
                    Neuron first_el = (Neuron)(this.neuronQueue[i][0]).Child;
                    if (!whatToPush.ContainsKey(first_el))
                    {
                        if (first_el.dendrites_list.Count() > 1)
                        {
                            foreach (Dendrite den in first_el.dendrites_list)
                                den.isBlocked = false;
                            this.addToNeuronsToCloseDendriteList(first_el);
                        }
                        whatToPush[first_el] = new List<double>() { flow };
                    }

                    double toPush = 0;
                    for (int j = 1; j < this.neuronQueue[i].Count(); j++)
                    {
                        toPush = 0;
                        Viewbox viewbox_prev = this.neuronQueue[i][j - 1];
                        Neuron prevNeuron = (Neuron)viewbox_prev.Child;
                        Neuron currentNeuron = (Neuron)(this.neuronQueue[i][j]).Child;

                        toPush += prevNeuron.volumeToPush;

                        if (currentNeuron.isFull)
                        {
                            toPush += currentNeuron.volumeToPush;
                            currentNeuron.volumeToPush = 0;
                            if (prevNeuron.dendrites_list.Count() > 1 && whatToPush.ContainsKey(prevNeuron) && whatToPush[prevNeuron].Count() > 1)
                            {
                                if (whatToPush.ContainsKey(prevNeuron) && whatToPush[prevNeuron].Count() >1)
                                {
                                    // When neuron has two or more dendrites
                                    List<double> newFlowValues = new List<double>();
                                    foreach (Double value in whatToPush[prevNeuron])
                                    {
                                        newFlowValues.Add(value + toPush / whatToPush[prevNeuron].Count());
                                    }
                                    whatToPush[prevNeuron] = newFlowValues;
                                }
                            }
                            else if (whatToPush.ContainsKey(prevNeuron))
                            {
                                double value = whatToPush[prevNeuron][0];
                                whatToPush[prevNeuron] = new List<double> { value + toPush };
                            }
                            else
                                whatToPush[prevNeuron] = new List<double> { toPush };
                            prevNeuron.axon.blockTheEnd = true;
                            break;
                        }

                        else if (toPush > 0)
                        {
                            if (currentNeuron.dendrites_list.Count() > 1 )
                                whatToPush = this.addVolumeFlowUpOrDown(viewbox_prev.Name, currentNeuron, whatToPush, toPush);
                            else
                            {
                                if (whatToPush.ContainsKey(currentNeuron))
                                {
                                    double value = whatToPush[currentNeuron][0];
                                    whatToPush[currentNeuron] = new List<double> { value + toPush };
                                }
                                else
                                    whatToPush[currentNeuron] = new List<double> { toPush };
                            }
                            Console.WriteLine(viewbox_prev.Name);
                            if (viewbox_prev.Name == "up")
                            {
                                Console.WriteLine("Unblock Den");
                                ((Dendrite)currentNeuron.dendrites_list[0]).isBlocked = false;
                                this.addToNeuronsToCloseDendriteList(currentNeuron);
                            }
                            else if (viewbox_prev.Name == "down")
                            {
                                Console.WriteLine("Unblock Den");
                                ((Dendrite)currentNeuron.dendrites_list[1]).isBlocked = false;
                                this.addToNeuronsToCloseDendriteList(currentNeuron);
                            }
                        }
                        Console.WriteLine("After break @@@@@@@@@@@@@@@@@@@@@@@@@@@@");

                    }
                }
            }

            // push volume to neuron
            foreach (KeyValuePair<Neuron, List<double>> element in whatToPush)
            {
                Neuron neuron = element.Key;
                this.neuronFlow(sender, e, neuron, element.Value);
            }
        }

        // add volume to up or down dendrite
        private Dictionary<Neuron, List<double>> addVolumeFlowUpOrDown(string site, Neuron neuron, Dictionary<Neuron, List<double>> whatToPush, double volumeToPush)
        {
            int index = site == "up" ? 0 : 1;
            if (whatToPush.ContainsKey(neuron))
            {
                double value = whatToPush[neuron][index];
                whatToPush[neuron][index] = value + volumeToPush;
            }
            else
            {
                if (index == 0)
                    whatToPush[neuron] = new List<double> { volumeToPush, 0 };
                else
                    whatToPush[neuron] = new List<double> { 0, volumeToPush };
            }
            return whatToPush;
        }

        // add neuron to list of neuron which dendrite should be closed
        private void addToNeuronsToCloseDendriteList(Neuron neuron)
        {
            if (!this.neuronsToCloseDendrites.Contains(neuron))
            {
                this.neuronsToCloseDendrites.Add(neuron);
            }
        }

        // put fluid to neuron
        private double neuronFlow(object sender, EventArgs e, Neuron neuron, List<double> flowList)
        {

            double toPush = 0;
            double volumeToPushNext = 0;
            Console.WriteLine("In neuron flow");
            // if there is no dendrite in neuron
            if (neuron.dendrites_list.Count() == 0)
            {
                Tuple<bool, double> axonRes = neuron.axon.newFlow(sender, e, flowList[0], color);
                volumeToPushNext = axonRes.Item2;
                this.setOutFlowParameters(neuron, axonRes.Item2);
                neuron.volumeToPush = volumeToPushNext;
                return volumeToPushNext;
            }

            int counter = 0;
            // push volume to each dendrite
            foreach (Dendrite dendrite in neuron.dendrites_list)
            {
                bool blocked = dendrite.isBlocked;
                if (!dendrite.isBlocked)
                {
                    Tuple<bool, double> dendriteRes = dendrite.newFlow(sender, e, flowList[counter], color);
                    if (dendriteRes.Item1)
                        toPush += dendriteRes.Item2;
                }
                counter++;
            }
            Thread.Sleep(10);
            // if dendrite return some volume push to soma
            if (toPush > 0)
            {
                bool axonFull = neuron.axon.isFull && neuron.axon.blockTheEnd;
                Console.WriteLine("Axon is full : " + axonFull);

                Tuple<bool, double> somaRes = neuron.soma.newFlow(sender, e, toPush, axonFull, color);
                // push volume to axon if axon is not full
                if (somaRes.Item1 && !axonFull)
                {
                    Tuple<bool, double> axonRes = neuron.axon.newFlow(sender, e, somaRes.Item2, color);
                    volumeToPushNext = axonRes.Item2;
                    this.setOutFlowParameters(neuron, axonRes.Item2);
                    neuron.volumeToPush = volumeToPushNext;
                }
                // if axon is full save the voume
                else if (somaRes.Item1 && axonFull)
                {
                    neuron.isFull = true;
                    neuron.volumeToPush = somaRes.Item2;
                }
            }
            return volumeToPushNext;
        }

        // set beggining of out flow time and beggining of out flow time in reminder and increase total out flow volume
        private void setOutFlowParameters(Neuron neuron, double axonOutFlowVolume)
        {
            if (axonOutFlowVolume > 0)
            {
                if (this.timeBegginingOfOutflowInReminder == 0 && neuron == this.neuronQueue[0][this.neuronQueue[0].Count() - 1].Child)
                {
                    Console.WriteLine("Axon return volume !!!! " + counter);
                    if (this.remindStarted)
                        this.timeBegginingOfOutflowInReminder = ((double)counter * (double)this.timerTimeSpan) / 1000;
                    else 
                    {
                        if (this.startOutFlowTime == 0)
                        {
                            this.startOutFlowTime = ((double)counter * (double)this.timerTimeSpan) / 1000;
                        }
                        this.totalOutFlow += axonOutFlowVolume;
                    }
                }
            }
        }

        // create new neuron
        private void create_neuron(object sender, MouseButtonEventArgs e)
        {
            Viewbox viewbox_sender = (Viewbox)sender;
            int den_number = Int32.Parse(viewbox_sender.Name.Replace("n", ""));
            Viewbox viewbox = new Viewbox() { StretchDirection = StretchDirection.Both, Stretch = Stretch.Uniform, Height = 40, Width = 250 };
            Neuron newNeuron = new Neuron(den_number);
            viewbox.Child = newNeuron;
            viewbox.MouseDown += new MouseButtonEventHandler(this.neuron_MouseDown);
            dropCanvas.Children.Add(viewbox);
        }

        // change time in the stopwatch
        private void myTimerTick(object sender, EventArgs e)
        {
            Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            Console.WriteLine(this.timeOffset);
            Console.WriteLine(DateTime.Now - this.TimerStart);
            Console.WriteLine(DateTime.Now - this.TimerStart - this.timeOffset);
            TimeSpan currentValue = DateTime.Now - this.TimerStart - this.timeOffset;
            this.timerTextBlock.Text = currentValue.ToString(@"mm\:ss");
        }

        // stop flow after 'Stop' button clicked
        private void stop(bool fromTimer)
        {
            Console.WriteLine("In stop!!!!!!!!!!!");
            Console.WriteLine(this.tickThreshold);
            Console.WriteLine(this.counter);
            this.timer.Stop();
            this.enableViewboxMoving();
            startButton.IsEnabled = true;
            this.timeOffset = TimeSpan.Parse("00:00:00");
            this.pauseFlow = false;


            if (fromTimer)
            {
                // Unload all neurons
                reminderButton.IsEnabled = true;

                List<Viewbox> visited = new List<Viewbox>();
                foreach (List<Viewbox> elList in this.neuronQueue)
                {
                    for (int i = elList.Count - 1; i >= 0; i--)
                    {
                        if (!visited.Contains(elList[i]))
                        {
                            Neuron neuron = (Neuron)elList[i].Child;
                            neuron.unload(remindStarted);
                            neuron.isFull = false;
                            neuron.volumeToPush = 0;
                            Thread.Sleep(100);
                            visited.Add(elList[i]);
                        }
                    }
                }
  
                // Block dendrite of neurons
                Console.WriteLine("List block length: " + this.neuronsToCloseDendrites.Count());
                this.blockNeuronsDendrites();
            }

            if (!remindStarted)
            {
                this.drainingTimer.Start();
            }
            else
            {
                Console.WriteLine("Counter value!!! " + this.timeBegginingOfOutflowInReminder);
                if (this.startOutFlowTime > 0 && this.timeBegginingOfOutflowInReminder < this.startOutFlowTime)
                    this.somethingInNeuron = true;
                else if (this.startOutFlowTime == 0 && this.timeBegginingOfOutflowInReminder <= this.minTimeToOutFlow)
                    this.somethingInNeuron = true;
                else if (this.startOutFlowTime == 0)
                {
                    double timedifference = this.timeBegginingOfOutflowInReminder - this.minTimeToOutFlow + (double)(this.somaAmount * this.timerTimeSpan)/1000;
                    double additionalVolume = (timedifference / 0.2) * (this.flowVolume);
                    this.somethingInNeuron = (this.maxSomaVolumeInQueue <= additionalVolume || this.maxSomaVolumeInQueue < (additionalVolume + this.flowVolume)) ? false : true;
                }
                else
                    this.somethingInNeuron = false;
            }

            this.remindStarted = false;
            this.color = System.Windows.Media.Brushes.DodgerBlue;

        }

        // close dendrites from list
        private void blockNeuronsDendrites()
        {
            foreach (Neuron neuron in this.neuronsToCloseDendrites)
            {
                foreach (Dendrite dendrite in neuron.dendrites_list)
                {
                    Console.WriteLine("Block DEN!");
                    dendrite.isBlocked = true;
                }

            }
        }


        // reset parameters
        private void resetParams()
        {
            this.timerTextBlock.Text = "00:00";
            this.pauseFlow = false;
            this.drainingTimer.Stop();
            this.remindStarted = false;
            this.totalOutFlow = 0;
            this.minTimeToOutFlow = 0;
            this.somethingInNeuron = false;
            this.startOutFlowTime = 0;
        }

        // reset flow and parameters after click 'Reset' button
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop(false);
            this.dropCanvas.Children.Clear();
            this.neuronQueue.Clear();
            this.canvasElements.Clear();
            this.resetParams();
        }

        // reste flow after click 'Reset flow' button
        private void resetFlowButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop(false);
            foreach (Viewbox viewbox in this.canvasElements.Keys)
            {
                Neuron neuron = (Neuron)viewbox.Child;
                neuron.reset();
                neuron.isFull = false;
            }
            this.blockNeuronsDendrites();
            this.resetParams();
        }

        // stop flow and draining after 'Stop' button clisk
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop(false);
            this.timeOffset = TimeSpan.Parse("00:00:00");
            this.drainingTimer.Stop();
        }

        // open parameters window after 'Set parameters' button click
        private void parametersButton_Click(object sender, RoutedEventArgs e)
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

        // set value of current configuration xml file
        private void getConfParamsXML(string path, double time, double flow, double drainingSpeed)
        {
            this.currentConf = path;
            Console.WriteLine("In main Window" + path);
        }

        // pause flow after 'Pasue' button click
        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            this.timer.Stop();
            this.pauseFlow = true;
            startButton.IsEnabled = true;
        }


        // stop timers after window closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.timer.Stop();
            this.drainingTimer.Stop();
        }

        // unblock end of neurons for reminder flow
        private void unblockEnds()
        {
            foreach (Viewbox viewbox in this.canvasElements.Keys)
            {
                ((Neuron)viewbox.Child).axon.blockTheEnd = false;
            }
        }

        // start reminder simulation
        private void reminderButon_Click(object sender, RoutedEventArgs e)
        {
            this.timeBegginingOfOutflowInReminder = 0;
            this.drainingTimer.Stop();
            this.blockTheEnd = false;
            this.unblockEnds();
            this.counter = 0;
            this.color = System.Windows.Media.Brushes.Maroon;
            this.timerTextBlock.Text = "00:00";
            this.TimerStart = DateTime.Now;
            this.timer.Start();
            this.remindStarted = true;
            this.reminderButton.IsEnabled = false;
            this.startButton.IsEnabled = false;
            this.blockViewboxMoving();
        }

        // calculate minimum time of out flow, below which, the neuron is said to contains some information
        private void calculateTimeOfOutFlow()
        {   
            List<Double> resList = new List<double>();
            List<int> somaCounter = new List<int>();
            List<Double> sumOfSomaVolume = new List<double>();
            int elements = 0;
            foreach (List<Viewbox> neuronList in this.neuronQueue)
            {
                double timeForNeuron = 0;
                int somaC = 0;
                int listAmountElements = 0;
                double somaVol = 0;
                foreach (Viewbox viewbox in neuronList)
                {
                    Neuron neuron = (Neuron)viewbox.Child;
                    timeForNeuron += neuron.volumeToOutFlowWhenNeuronFull;
                    if (neuron.soma != null)
                    {
                        somaC += 1;
                        listAmountElements += 2;
                        somaVol += neuron.soma.threshold;
                    }
                    else
                    {
                        listAmountElements += 1;
                    }
                }
                elements = elements == 0 ? listAmountElements: elements;
                elements = elements > listAmountElements ? listAmountElements : elements;
                Console.WriteLine("Single neuron volume" + timeForNeuron);
                resList.Add(timeForNeuron);
                somaCounter.Add(somaC);
                sumOfSomaVolume.Add(somaVol);

            }
            this.maxSomaVolumeInQueue = sumOfSomaVolume.Max();
            int index = sumOfSomaVolume.FindIndex(el => el == this.maxSomaVolumeInQueue);
            this.somaAmount = somaCounter[index];
            double max = resList[index];
            this.queueNumberForReminder = index;
            double minVolumeForQueue = (((double)max / ((double)this.flowVolume * elements)) * (double)this.timerTimeSpan) / (double)1000;
            if (minVolumeForQueue * 1000 < elements * this.timerTimeSpan)
                minVolumeForQueue = this.timerTimeSpan * elements / (double)1000;
            this.minTimeToOutFlow = minVolumeForQueue + ((double)this.timerTimeSpan * (double)somaAmount) / (double)1000;
            Console.WriteLine("Test time: " + this.minTimeToOutFlow);
        }

        //create add prarameters values and open results window
        private void resultButton_Click(object sender, RoutedEventArgs e)
        {
            ResultsDragAndDropWindow resultsWindow = new ResultsDragAndDropWindow();
            resultsWindow.somethingRememberedTextBlock.Text = this.somethingInNeuron == true ? "True": "False";
            resultsWindow.reminderOutFlowTimeTextBlock.Text = this.timeBegginingOfOutflowInReminder.ToString();
            resultsWindow.outFlowTimeTextBlock.Text = this.startOutFlowTime.ToString();
            resultsWindow.outFlowVolumeTextBlock.Text = this.totalOutFlow.ToString("0.00");
            resultsWindow.ShowDialog();
        }
    }
}
