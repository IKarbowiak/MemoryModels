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
using System.Text.RegularExpressions;
using PracaMagisterska.PersonalSolution;

namespace PracaMagisterska
{

    public partial class DragAndDropPanel : Window
    {
        public Dictionary<NeuronViewbox, double[]> canvasElements { get; set; }
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer drainingTimer;
        private List<List<NeuronViewbox>> neuronQueue = new List<List<NeuronViewbox>>();
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
        private List<NeuronViewbox> neuronsToCloseDendrites = new List<NeuronViewbox>();
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
            canvasElements = new Dictionary<NeuronViewbox, double[]>();
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
                    {
                        this.stop(true);
                        this.enableViewboxInQueuMoving();
                    }
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
                foreach (NeuronViewbox viewbox in this.canvasElements.Keys)
                {
                    bool empty = viewbox.drain(this.drainingVolume);
                    emptyResults.Add(empty);
                }
                if (!emptyResults.Contains(false))
                {
                    Console.WriteLine("Stop draining timer");
                    Console.WriteLine("Stop draining timer");
                    drainingTimer.Stop();
                    this.enableViewboxInQueuMoving();
                }
            };
        }

        // create neurons in the left panel, which can be click to create duplicated object in neuron panel
        private void createVieboxWithNeuron(int dendNumber)
        {
            TextBlock modelName = new TextBlock() { TextAlignment = TextAlignment.Center, Text = "Model " + (dendNumber + 1) };
            objectHandlerPanel.Children.Add(modelName);

            Viewbox viewbox = new Viewbox() { Name = "n" + dendNumber, StretchDirection = StretchDirection.Both, Stretch = Stretch.Uniform };
            Neuron newNeuron = new Neuron(dendNumber);
            newNeuron.Height = 150;
            newNeuron.Width = 450;
            viewbox.Child = newNeuron;
            viewbox.MouseDown += new MouseButtonEventHandler(this.create_neuron);

            objectHandlerPanel.Children.Add(viewbox);
        }

        private void setCurrentConf()
        {
            Regex pattern = new Regex("(.*)(\\\\bin\\\\Debug)", RegexOptions.Compiled);
            Match match = pattern.Match(Directory.GetCurrentDirectory());
            GroupCollection groups = match.Groups;
            this.currentConf = groups[1].Value + "\\defaultConf.xml";
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
        private void addToLeft(List<int> checkEl, NeuronViewbox viewbox)
        {
            this.removeAbilityToMove(this.neuronQueue[checkEl[0]]);
            // the checkEl can't be longer than 2 elements
            if (checkEl[1] > 0)
            {
                List<NeuronViewbox> queue = this.neuronQueue[checkEl[0]];
                List<NeuronViewbox> elements = queue.GetRange(checkEl[1], queue.Count() - checkEl[1]);
                elements.Insert(0, viewbox);
                this.neuronQueue.Add(elements);
                return;
            }
            this.neuronQueue[checkEl[0]].Insert(checkEl[1], viewbox);
        }

        // add neuron to the right side of queue
        private void addToRight(List<int> checkEl, NeuronViewbox viewbox)
        {
            this.removeAbilityToMove(this.neuronQueue[checkEl[0]]);
            // checkEl list can be longer than 2 elements, but always is linked to the end of list
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

        // remove ability to move neuron in neuron panel
        private void removeAbilityToMove(List<NeuronViewbox> checkList) 
        {
            if (checkList.Count() > 1)
            {
                NeuronViewbox viewbox = checkList[checkList.Count() - 1];
                Console.WriteLine("Remove ability");
                viewbox.removeViewboxAbilityToMove();
            }
        }

        // remove empty queue (residue after unlinked) form neuron queue
        private void removeEmptyNeuronQueue()
        {
            for (int i = 0; i < this.neuronQueue.Count(); i++)
            {
                if (this.neuronQueue[i].Count() == 1 || this.neuronQueue[i].Count() == 0)
                    this.neuronQueue.RemoveAt(i);
            }
        }


        // link moving neuron to queue
        // check if neuron is out of border, 
        private bool linkLeftOrRight(NeuronViewbox viewbox, String site, KeyValuePair<NeuronViewbox, Double[]> element)
        {
            //this.removeEmptyNeuronQueue();
            bool outOfBorder = viewbox.checkIfQuitBorder();
            double[] newPosition = viewbox.getCanvasParameters();
            // Check if something is on this place if is, come back to previous position
            if (!outOfBorder && this.checkDictValue(this.canvasElements.Values.ToArray(), newPosition))
            {
                viewbox.backToPreviousPosition();
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
                    List<NeuronViewbox> elements = new List<NeuronViewbox>();
                    if (site == "left")
                    {
                        NeuronViewbox elBox = (NeuronViewbox)element.Key;
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
        // removing is possible only from first and last element of the list
        // after removing, check if the rest of list should be removed or not
        private void checkIfNeuronLeaveQueue(NeuronViewbox viewbox)
        {
            List<int> checkList = neuronQueueContainsCheck(viewbox);
            if (checkList.Count == 0)
                return;

            int list_num = checkList[0];
            int elInList = checkList[1];
            Console.WriteLine("Delete from List");
            this.neuronQueue[list_num].RemoveAt(elInList);
            bool removeList = false;
            this.clearFlow();
            // block posibility to remind if the queus changed
            reminderButton.IsEnabled = false;
            
            // check if other list contain reset of the neuron in the same order as in the updating one
            foreach (List<NeuronViewbox> neuronList in this.neuronQueue)
            {
                var res =  this.neuronQueue[list_num].Except(neuronList).ToList();
                if (neuronList != this.neuronQueue[list_num] && !this.neuronQueue[list_num].Except(neuronList).Any())
                    removeList = true;
            }

            if (removeList == true)
                this.neuronQueue.RemoveAt(list_num);

            this.enableViewboxInQueuMoving();
        }

        // set connection to specific dendrit in neuron with more than one dendrite
        private void setConnectionToDen(NeuronViewbox viewbox, string direction, NeuronViewbox element, string side)
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
        private void set_Position(NeuronViewbox viewbox, KeyValuePair<NeuronViewbox, Double[]> element, bool condition, string side, double offset)
        {
            double[] param = viewbox.getCanvasParameters();
            double posX;
            double posY;
            if (side == "left")
                posX = element.Value[0] - 1;
            else
                posX = element.Value[1] + 1 ;

            if (condition)
            {
                if (element.Value[2] - param[2] <= 0)
                {
                    Console.WriteLine("Down");
                    posY = element.Value[2] + offset;
                    setConnectionToDen(viewbox, "down", element.Key, side);
                }
                else
                {
                    posY = element.Value[2] - offset;
                    Console.WriteLine("Up");
                    setConnectionToDen(viewbox, "up", element.Key, side);
                }
            }
            else
            {
                posY = element.Value[2];
            }
            viewbox.changePosition(posX, posY, side);

        }

        // link neuron to queue
        private bool linkNeuron(NeuronViewbox viewbox)
        {
            int catchValue_rightleft = 30;
            int catchValue_updown = 15;
            double offset = 11.6;

            foreach (KeyValuePair<NeuronViewbox, Double[]> element in canvasElements)
            {
                double[] viewbox_params = viewbox.getCanvasParameters();
                if ((element.Key != viewbox) && ((Math.Abs(element.Value[2] - viewbox_params[2]) <= catchValue_updown) ||
                    (Math.Abs(element.Value[3] - viewbox_params[3]) <= catchValue_updown)))
                {
                    bool results = false;
                    NeuronViewbox elBox = (NeuronViewbox)element.Key;
                    // check if neuron is near to the left side of queue
                    double val = Math.Abs(element.Value[0] - viewbox_params[0]);
                    double val2 = Math.Abs(element.Value[1] - viewbox_params[1]);
                    if (Math.Abs(element.Value[0] - viewbox_params[1]) <= catchValue_rightleft)
                    {
                        set_Position(viewbox, element, elBox.getNumberOfDendrites() > 1, "left", offset);
                        results = this.linkLeftOrRight(viewbox, "left", element);
                        return results;
                    }
                    // check if neuron is near to the right side of queue
                    else if (Math.Abs(element.Value[1] - viewbox_params[0]) <= catchValue_rightleft)
                    {
                        set_Position(viewbox, element, (viewbox.getNumberOfDendrites() > 1), "right", offset);
                        results = this.linkLeftOrRight(viewbox, "right", element);
                        return results;
                    }
                }
            }
            return false;
        }

        // try to link neuron to queue after mouse up from neuron
        public void after_mouseUp(NeuronViewbox viewboxObj)
        {
            bool linked = this.linkNeuron(viewboxObj);
            List<int> checkList = neuronQueueContainsCheck(viewboxObj);

            if (!linked)
            {
                //TODO: function to finish - it's finished probably
                this.checkIfNeuronLeaveQueue(viewboxObj);
            }

            double[] parameters = viewboxObj.getCanvasParameters();
            canvasElements[viewboxObj] = parameters;


            Console.WriteLine("List count " + this.neuronQueue.Count());
            Console.WriteLine(canvasElements[viewboxObj][0]);
            Console.WriteLine("Dictionary count !!!!! " + canvasElements.Count());

            // Check to remove 
            counter = 0;
            foreach (List<NeuronViewbox> el in this.neuronQueue)
            {
                Console.WriteLine("List " + counter);
                Console.WriteLine(el.Count());
                counter++;
            }
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
            this.blockAllViewboxMoving();

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

            this.clearFlow();

            Console.WriteLine("Current path: " + this.currentConf);
            if (currentConf == null)
            {
                this.setCurrentConf();
            }
            this.loadParams();
            Console.WriteLine(this.flowVolume);

            counter = 0;
            //double divider = ((double)1000 / (double)this.timerTimeSpan);
            //this.flowVolume = this.flowVolume / divider;
            //this.drainingVolume = this.drainingVolume / divider;

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

        private void setFlowAndDrainingVolume(double flowValue, double drainingValue)
        {
            double divider = ((double)1000 / (double)this.timerTimeSpan);
            this.flowVolume = flowValue / divider;
            this.drainingVolume = drainingValue / divider;
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
                    this.setFlowAndDrainingVolume(double.Parse(values_list[0].Value.ToString()), double.Parse(values_list[2].Value.ToString()));
                    this.flowTime = double.Parse(values_list[1].Value.ToString());
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

            double divider = ((double)1000 / (double)this.timerTimeSpan);
            foreach (NeuronViewbox element in canvasElements.Keys)
            {
                double numberOfDendrites = element.getNumberOfDendrites();
                if (numberOfDendrites == 0)
                     element.setNeuronParams(neuron0_params, divider);
                else if (numberOfDendrites == 1)
                    element.setNeuronParams(neuron1_params, divider);
                else if (numberOfDendrites == 2)
                    element.setNeuronParams(neuron2_params, divider);
            }

            Console.WriteLine("Drag and drop " + this.blockTheEnd);
            if (blockTheEnd)
            {
                foreach (List<NeuronViewbox> viewbox_list in this.neuronQueue)
                    viewbox_list[viewbox_list.Count - 1].blockAxonEnd();
            }
        }


        // block moving all neurons during flow
        private void blockAllViewboxMoving()
        {
            foreach (NeuronViewbox element in this.canvasElements.Keys)
            {
                element.removeViewboxAbilityToMove();
            }
        }

        // unblock moving neurons
        private void enableViewboxInQueuMoving()
        {
            // add posibility to move first and last element in queue
            foreach (List<NeuronViewbox> queue in this.neuronQueue)
            {
                if (queue.Count() > 0)
                {
                    queue[0].enableViewboxMoving();
                    queue[queue.Count() - 1].enableViewboxMoving();
                }
            }
        }

        // main function which push fluid to neurons
        private void flow(object sender, EventArgs e, double flow)
        {
            Dictionary<NeuronViewbox, List<double>> whatToPush = new Dictionary<NeuronViewbox, List<double>>();
            Console.WriteLine("Count whatToPush" + whatToPush.Count());

            // if there is no queue but any neuron in panel exist
            if (this.neuronQueue.Count() == 0 && this.canvasElements.Count > 0)
            {
                foreach (KeyValuePair<NeuronViewbox, double[]> element in this.canvasElements)
                {
                    NeuronViewbox viewboxObj = (NeuronViewbox)element.Key;
                    if (viewboxObj.getNumberOfDendrites() > 1)
                    {
                        viewboxObj.openDendrites();
                        this.addToNeuronsToCloseDendriteList(viewboxObj);
                        whatToPush[viewboxObj] = new List<double> { flow, flow };
                    }
                    else
                        whatToPush[viewboxObj] = new List<double> { flow };
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
                    NeuronViewbox first_el = this.neuronQueue[i][0];
                    if (!whatToPush.ContainsKey(first_el))
                    {
                        if (first_el.getNumberOfDendrites() > 1)
                        {
                            first_el.openDendrites();
                            this.addToNeuronsToCloseDendriteList(first_el);
                            whatToPush[first_el] = new List<double>() { flow, flow };
                        }
                        else
                            whatToPush[first_el] = new List<double>() { flow };
                    }

                    double toPush = 0;
                    for (int j = 1; j < this.neuronQueue[i].Count(); j++)
                    {
                        toPush = 0;
                        NeuronViewbox viewbox_prev = this.neuronQueue[i][j - 1];
                        NeuronViewbox viewbox_current = this.neuronQueue[i][j];

                        toPush += viewbox_prev.getVolumeToPush();

                        if (viewbox_current.neuronIsFull())
                        {
                            toPush += viewbox_current.getVolumeToPush();
                            viewbox_current.setVolumeToPush(0);
                            if (viewbox_prev.getNumberOfDendrites() > 1 && whatToPush.ContainsKey(viewbox_prev) && whatToPush[viewbox_prev].Count() > 1)
                            {
                                if (whatToPush.ContainsKey(viewbox_prev) && whatToPush[viewbox_prev].Count() >1)
                                {
                                    // When neuron has two or more dendrites
                                    List<double> newFlowValues = new List<double>();
                                    foreach (Double value in whatToPush[viewbox_prev])
                                    {
                                        newFlowValues.Add(value + toPush / whatToPush[viewbox_prev].Count());
                                    }
                                    whatToPush[viewbox_prev] = newFlowValues;
                                }
                            }
                            else if (whatToPush.ContainsKey(viewbox_prev))
                            {
                                double value = whatToPush[viewbox_prev][0];
                                whatToPush[viewbox_prev] = new List<double> { value + toPush };
                            }
                            else
                                whatToPush[viewbox_prev] = new List<double> { toPush };
                            viewbox_prev.blockAxonEnd();
                            break;
                        }

                        else if (toPush > 0)
                        {
                            if (viewbox_current.getNumberOfDendrites() > 1 )
                                whatToPush = this.addVolumeFlowUpOrDown(viewbox_prev.Name, viewbox_current, whatToPush, toPush);
                            else
                            {
                                if (whatToPush.ContainsKey(viewbox_current))
                                {
                                    double value = whatToPush[viewbox_current][0];
                                    whatToPush[viewbox_current] = new List<double> { value + toPush };
                                }
                                else
                                    whatToPush[viewbox_current] = new List<double> { toPush };
                            }
                            Console.WriteLine(viewbox_prev.Name);
                            string side = viewbox_prev.Name;
                            if (side == "up" || side == "down")
                            {
                                viewbox_current.openOneDendrite(side);
                                this.addToNeuronsToCloseDendriteList(viewbox_current);
                            }
                        }
                        Console.WriteLine("After break @@@@@@@@@@@@@@@@@@@@@@@@@@@@");

                    }
                }
            }

            // push volume to neuron
            foreach (KeyValuePair<NeuronViewbox, List<double>> element in whatToPush)
            {
                NeuronViewbox viewboxObj = element.Key;
                bool missMaxAxonSpeed = this.remindStarted && this.startOutFlowTime == 0 ? true : false; 
                viewboxObj.neuronFlow(sender, e, element.Value, color, missMaxAxonSpeed);
            }
        }

        // add volume to up or down dendrite
        private Dictionary<NeuronViewbox, List<double>> addVolumeFlowUpOrDown(string site, NeuronViewbox neuron, Dictionary<NeuronViewbox, List<double>> whatToPush, double volumeToPush)
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
        private void addToNeuronsToCloseDendriteList(NeuronViewbox neuron)
        {
            if (!this.neuronsToCloseDendrites.Contains(neuron))
            {
                this.neuronsToCloseDendrites.Add(neuron);
            }
        }

        // set beggining of out flow time and beggining of out flow time in reminder and increase total out flow volume
        public void setOutFlowParameters(NeuronViewbox viewboxObj, double axonOutFlowVolume)
        {
            if (axonOutFlowVolume > 0)
            {
                if (this.timeBegginingOfOutflowInReminder == 0 &&  ( (this.neuronQueue.Count() == 0) || viewboxObj == this.neuronQueue[0][this.neuronQueue[0].Count() - 1]))
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

            NeuronViewbox viewboxObj = new NeuronViewbox(den_number);
            dropCanvas.Children.Add(viewboxObj);
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

        private void unloadNeurons()
        {
            List<NeuronViewbox> visited = new List<NeuronViewbox>();
            if (this.neuronQueue.Count > 0)
            {
                foreach (List<NeuronViewbox> elList in this.neuronQueue)
                {
                    for (int i = elList.Count - 1; i >= 0; i--)
                    {
                        if (!visited.Contains(elList[i]))
                        {
                            elList[i].unloadNeuron(remindStarted);
                            Thread.Sleep(100);
                            visited.Add(elList[i]);
                        }
                    }
                }
            }
            else
            {
                foreach (NeuronViewbox viewboxObj in this.canvasElements.Keys)
                {
                    if (!visited.Contains(viewboxObj))
                    {
                        viewboxObj.unloadNeuron(remindStarted);
                        Thread.Sleep(100);
                        visited.Add(viewboxObj);
                    }
                }
            }
        }

        // stop flow after 'Stop' button clicked
        private void stop(bool fromTimer)
        {
            Console.WriteLine("In stop!!!!!!!!!!!");
            Console.WriteLine(this.tickThreshold);
            Console.WriteLine(this.counter);
            this.timer.Stop();
            startButton.IsEnabled = true;
            this.timeOffset = TimeSpan.Parse("00:00:00");
            this.pauseFlow = false;


            if (fromTimer)
            {
                // Unload all neurons
                reminderButton.IsEnabled = true;

                this.unloadNeurons();

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
                {
                    this.somethingInNeuron = true;
                }
                else if (this.startOutFlowTime == 0 && this.timeBegginingOfOutflowInReminder <= this.minTimeToOutFlow)
                {
                    this.somethingInNeuron = true;
                }
                else if (this.startOutFlowTime == 0)
                {
                    double timedifference = this.timeBegginingOfOutflowInReminder - this.minTimeToOutFlow + (double)(this.somaAmount * this.timerTimeSpan)/1000;
                    timedifference = Math.Floor(timedifference * 10) / 10;
                    double additionalVolume = (timedifference / 0.2) * (this.flowVolume);
                    this.somethingInNeuron = (this.maxSomaVolumeInQueue <= additionalVolume) ? false : true;
                }
                else
                {
                    this.somethingInNeuron = false;
                }
            }

            this.remindStarted = false;
            this.color = System.Windows.Media.Brushes.DodgerBlue;

        }

        // close dendrites from list
        private void blockNeuronsDendrites()
        {
            foreach (NeuronViewbox viewboxObj in this.neuronsToCloseDendrites)
                viewboxObj.closeDendrites();
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

        // reset flow after click 'Reset flow' button
        private void resetFlowButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop(false);
            this.clearFlow();
            this.blockNeuronsDendrites();
            
        }

        private void clearFlow()
        {
            foreach (NeuronViewbox viewbox in this.canvasElements.Keys)
                viewbox.resetNeuron();
            this.resetParams();
        }

        // stop flow and draining after 'Stop' button clisk
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop(false);
            this.enableViewboxInQueuMoving();
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
            foreach (NeuronViewbox viewbox in this.canvasElements.Keys)
                viewbox.unblockAxonEnd();
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
            this.blockAllViewboxMoving();
        }

        // calculate minimum time of out flow, below which, the neuron is said to contains some information
        private void calculateTimeOfOutFlow()
        {
            if (this.neuronQueue.Count() == 0)
                return;
            List<Double> resList = new List<double>();
            List<int> somaCounter = new List<int>();
            List<int> elementsList = new List<int>();
            List<Double> sumOfSomaVolume = new List<double>();
            int elements = 0;
            foreach (List<NeuronViewbox> neuronList in this.neuronQueue)
            {
                double volumeForNeuron = 0;
                int somaC = 0;
                int elementsAmount = 0;
                double somaVol = 0;
                foreach (NeuronViewbox viewbox in neuronList)
                {
                    volumeForNeuron += viewbox.getVolumeToOutflowWhenNeuronIsFull();
                    if (!viewbox.somaIsNull())
                    {
                        somaC += 1;
                        elementsAmount += 2;
                        somaVol += viewbox.getSomaThreshold();
                    }
                    else
                    {
                        elementsAmount += 1;
                    }
                }
                //elements = elements == 0 ? elementsAmount: elements;
                //elements = elements > elementsAmount ? elementsAmount : elements;
                Console.WriteLine("Single neuron volume" + volumeForNeuron);
                resList.Add(volumeForNeuron);
                somaCounter.Add(somaC);
                sumOfSomaVolume.Add(somaVol);
                elementsList.Add(elementsAmount);

            }
            this.maxSomaVolumeInQueue = sumOfSomaVolume.Max();
            int index = sumOfSomaVolume.FindIndex(el => el == this.maxSomaVolumeInQueue);
            this.somaAmount = somaCounter[index];
            double maxVolume = resList[index];
            elements = elementsList[index];
            this.queueNumberForReminder = index;
            double minTimeForQueue = (((double)maxVolume / ((double)this.flowVolume)) * (double)this.timerTimeSpan) / (double)1000;
            if (minTimeForQueue * 1000 < elements * this.timerTimeSpan)
                minTimeForQueue = this.timerTimeSpan * elements / (double)1000;
            double round = Math.Floor(minTimeForQueue * 10) / 10;
            this.minTimeToOutFlow = Math.Floor(minTimeForQueue * 10) / 10 + ((double)this.timerTimeSpan * (double)somaAmount) / (double)1000;
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
