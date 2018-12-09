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

        //private List<object> elementsToCheck = new List<object>();



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

        private void adjustTimer()
        {

            this.timer = new System.Windows.Threading.DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(this.timerTimeSpan);

            this.timer.Tick += (sender1, e1) =>
            {
                this.myTimerTick(sender1, e1);
                this.flow(sender1, e1, flowVolume);
                counter += 1;
                if (counter >= this.tickThreshold)
                {
                    this.stop(true);
                }

            };

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
                    drainingTimer.Stop();
                }
            };
        }

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

            void set_TopAndBottom_Property(KeyValuePair<Viewbox, Double[]> element, bool condition, string side)
            {
                if (condition)
                {
                    if ((element.Value[2] - Canvas.GetTop(viewbox)) <= 0)
                    {
                        Console.WriteLine("Down");
                        viewbox.SetValue(Canvas.TopProperty, element.Value[2] + offset);
                        viewbox.SetValue(Canvas.BottomProperty, element.Value[3] + offset);
                        setConnectionToDen("down", element.Key, side);
                    }
                    else
                    {
                        Console.WriteLine("Up");
                        viewbox.SetValue(Canvas.TopProperty, element.Value[2] - offset);
                        viewbox.SetValue(Canvas.BottomProperty, element.Value[3] - offset);
                        setConnectionToDen("up", element.Key, side);
                    }
                }
                else
                {
                    viewbox.SetValue(Canvas.TopProperty, element.Value[2]);
                    viewbox.SetValue(Canvas.BottomProperty, element.Value[3]);
                }

            }

            void setConnectionToDen(string direction, Viewbox element, string side)
            {
                string newDirection = " ";
                if (side == "left")
                {
                    viewbox.Name = direction;
                }
                else if (side == "right")
                {
                    newDirection = direction == "up" ? "down" : "up";
                    // przydałaby się tutaj obsługa błędów
                    element.Name = newDirection;
                }
            }

            List<int> neuronQueueContainsCheck(object element)
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

            bool linkNeuron()
            {
                foreach (KeyValuePair<Viewbox, Double[]> element in canvasElements)
                {
                    Console.WriteLine(element.Key == viewbox);
                    Console.WriteLine((Math.Abs(element.Value[2] - Canvas.GetTop(viewbox))));
                    Console.WriteLine((Math.Abs(element.Value[3] - Canvas.GetBottom(viewbox))));

                    if ((element.Key != viewbox) && ((Math.Abs(element.Value[2] - Canvas.GetTop(viewbox)) <= catchValue_updown) ||
                        (Math.Abs(element.Value[3] - Canvas.GetBottom(viewbox)) <= catchValue_updown)))
                    {

                        if (Math.Abs(element.Value[0] - Canvas.GetRight(viewbox)) <= catchValue_rightleft)
                        {
                            Viewbox elBox = (Viewbox)element.Key;
                            set_TopAndBottom_Property(element, ((Neuron)elBox.Child).dendrites_list.Count() > 1, "left");

                            viewbox.SetValue(Canvas.LeftProperty, element.Value[0] - viewbox.Width - 1);
                            viewbox.SetValue(Canvas.RightProperty, element.Value[0] - 1);

                            var pos = e.GetPosition(this.dropCanvas);
                            bool quit = checkIfQuitBorder(pos.X, pos.Y, viewbox);

                            double[] newPosition = new double[] { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };

                            if (!quit && this.checkDictValue(this.canvasElements.Values.ToArray(), newPosition))
                            {
                                Console.WriteLine("HOP HOP!");
                                viewbox.SetValue(Canvas.LeftProperty, this.startPosition[0]);
                                viewbox.SetValue(Canvas.RightProperty, this.startPosition[0]);
                                viewbox.SetValue(Canvas.TopProperty, this.startPosition[0]);
                                viewbox.SetValue(Canvas.BottomProperty, this.startPosition[0]);
                            }
                            else if (!quit) // outOfBorder
                            {
                                List<int> checkEl = neuronQueueContainsCheck(element.Key);
                                if (this.neuronQueue.Count() == 0)
                                {
                                    List<Viewbox> elements = new List<Viewbox>();
                                    elements.Add(viewbox);
                                    elements.Add(elBox);
                                    this.neuronQueue.Add(elements);
                                }
                                else if (checkEl.Count() > 0)
                                {
                                    if (checkEl[1] > 0)
                                    {
                                        List<Viewbox> elements = this.neuronQueue[checkEl[0]].GetRange(checkEl[1], this.neuronQueue[checkEl[0]].Count - 1);
                                        elements.Insert(0, viewbox);
                                        this.neuronQueue.Add(elements);
                                    }
                                    else
                                    {
                                        this.neuronQueue[checkEl[0]].Insert(checkEl[1], viewbox);
                                    }

                                }

                                return true;
                            }

                        }
                        else if (Math.Abs(element.Value[1] - Canvas.GetLeft(viewbox)) <= catchValue_rightleft)
                        {
                            set_TopAndBottom_Property(element, (neuron.dendrites_list.Count() > 1), "right");
                            List<int> checkEl = neuronQueueContainsCheck(element.Key);

                            viewbox.SetValue(Canvas.LeftProperty, element.Value[1] + 1);
                            viewbox.SetValue(Canvas.RightProperty, element.Value[1] + viewbox.Width + 1);

                            var pos = e.GetPosition(this.dropCanvas);
                            bool quit = checkIfQuitBorder(pos.X, pos.Y, viewbox);
                            Console.WriteLine("Quit " + quit);

                            double[] newPosition = new double[] { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };

                            if (!quit && this.checkDictValue(this.canvasElements.Values.ToArray(), newPosition))
                            {
                                Console.WriteLine("HOP HOP!");
                                viewbox.SetValue(Canvas.LeftProperty, this.startPosition[0]);
                                viewbox.SetValue(Canvas.RightProperty, this.startPosition[0]);
                                viewbox.SetValue(Canvas.TopProperty, this.startPosition[0]);
                                viewbox.SetValue(Canvas.BottomProperty, this.startPosition[0]);
                            }
                            else if (!quit)
                            {
                                Console.WriteLine("Not quit so LINK");
                                if (this.neuronQueue.Count() == 0)
                                {
                                    List<Viewbox> elements = new List<Viewbox>();
                                    elements.Add(element.Key);
                                    elements.Add(viewbox);
                                    this.neuronQueue.Add(elements);
                                }
                                else if (checkEl.Count() > 2)
                                {

                                    for (int j = 0; j < checkEl.Count(); j += 2)
                                    {
                                        this.neuronQueue[checkEl[j]].Insert(checkEl[j + 1] + 1, viewbox);
                                    }

                                }
                                else if (checkEl.Count() > 0)
                                {
                                    this.neuronQueue[0].Insert(checkEl[1] + 1, viewbox);
                                }


                                return true;

                            }


                        }

                    }

                }
                return false;
            };

            bool linked = linkNeuron();
            List<int> checkList = neuronQueueContainsCheck(viewbox);

            if (!linked && checkList.Count > 0)
            {
                int list_num = checkList[0];
                int elInList = checkList[1];
                Console.WriteLine("Delete from List");
                if (checkList.Count > 2)
                {
                    // czo!?
                }
                else if (elInList > this.neuronQueue[list_num].Count / 2)
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


            Console.WriteLine("List count " + this.neuronQueue.Count());
            double[] parameters = { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };
            canvasElements[(Viewbox)sender] = parameters;
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

        private bool checkIfQuitBorder(double x, double y, Viewbox viewbox)
        {
            double newX = x - (viewbox.Width / 2);
            double newY = y - (viewbox.Height / 2);

            Console.WriteLine(x + "  ; " + y + "  maxX" + maxX + "  maxY" + maxY + "get Left" + Canvas.GetLeft(viewbox) + " gwt Right " + Canvas.GetRight(viewbox));    // gwt?

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

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            reminderButon.IsEnabled = false;
            resultButton.IsEnabled = false;
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
            if (this.currentConf != null)
            {
                Console.WriteLine("Current path exists");
                this.loadParams();
            }
            Console.WriteLine(this.flowVolume);
            this.blockViewboxMoving();

            counter = 0;
            double divider = ((double)1000 / (double)this.timerTimeSpan);
            this.flowVolume = this.flowVolume / divider;
            this.drainingVolume = this.drainingVolume / divider;

            Console.WriteLine("Flow Volume!!! Drag nad Drop!!!!" + this.flowVolume);
            if (canvasElements.Count() > 0)
            {
                this.tickThreshold = (int)(this.flowTime * 1000 / this.timerTimeSpan);
                this.TimerStart = DateTime.Now;
                timer.Start();
                startButton.IsEnabled = false;
            }
        }

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

        private void blockViewboxMoving()
        {
            foreach (Viewbox element in this.canvasElements.Keys)
            {
                element.MouseDown -= new MouseButtonEventHandler(this.neuron_MouseDown);
            }
        }

        private void enableViewboxMoving()
        {
            foreach (Viewbox element in this.canvasElements.Keys)
            {
                element.MouseDown += new MouseButtonEventHandler(this.neuron_MouseDown);
            }
        }

        private void flow(object sender, EventArgs e, double flow)
        {

            Dictionary<Neuron, double> whatToPush = new Dictionary<Neuron, double>();
            Console.WriteLine("Count whatToPush" + whatToPush.Count());

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
                    whatToPush[neuron] = flow;
                }
            }

            for (int i = 0; i < this.neuronQueue.Count(); i++)
            {
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
                        whatToPush.Add(first_el, flow);
                    }

                    double toPush = 0;
                    Console.WriteLine("I interation" + i);
                    for (int j = 1; j < this.neuronQueue[i].Count(); j++)
                    {
                        Console.WriteLine("J iteration " + j);
                        toPush = 0;
                        Viewbox viewbox_prev = this.neuronQueue[i][j - 1];
                        Neuron prevNeuron = (Neuron)viewbox_prev.Child;
                        Neuron currentNeuron = (Neuron)(this.neuronQueue[i][j]).Child;

                        toPush += prevNeuron.volumeToPush;

                        if (currentNeuron.isFull)
                        {
                            toPush += currentNeuron.volumeToPush;
                            currentNeuron.volumeToPush = 0;
                            if (whatToPush.ContainsKey(prevNeuron)) whatToPush[prevNeuron] += toPush;
                            else whatToPush.Add(prevNeuron, toPush);
                            prevNeuron.axon.blockTheEnd = true;
                            Console.WriteLine("Brake loop!!!!! @@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            break;
                        }

                        else if (toPush > 0)
                        {
                            if (whatToPush.ContainsKey(currentNeuron))
                                whatToPush[currentNeuron] += toPush;
                            else
                                whatToPush.Add(currentNeuron, toPush);

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

            foreach (KeyValuePair<Neuron, double> element in whatToPush)
            {
                Neuron neuron = element.Key;
                this.neuronFlow(sender, e, neuron, element.Value);
            }
        }

        private void addToNeuronsToCloseDendriteList(Neuron neuron)
        {
            if (!this.neuronsToCloseDendrites.Contains(neuron))
            {
                this.neuronsToCloseDendrites.Add(neuron);
            }
        }

        private double neuronFlow(object sender, EventArgs e, Neuron neuron, double flow)
        {

            double toPush = 0;
            double volumeToPushNext = 0;
            Console.WriteLine("In neuron flow");
            if (neuron.dendrites_list.Count() == 0)
            {
                Tuple<bool, double> axonRes = neuron.axon.newFlow(sender, e, flow, color);
                volumeToPushNext = axonRes.Item2;
                neuron.volumeToPush = volumeToPushNext;
                return volumeToPushNext;
            }

            foreach (Dendrite dendrite in neuron.dendrites_list)
            {
                if (!dendrite.isBlocked)
                {
                    Tuple<bool, double> dendriteRes = dendrite.newFlow(sender, e, flow, color);
                    if (dendriteRes.Item1)
                        toPush += dendriteRes.Item2;
                }
            }
            Thread.Sleep(10);
            if (toPush > 0)
            {
                bool axonFull = neuron.axon.isFull && neuron.axon.blockTheEnd;
                Console.WriteLine("Axon is full : " + axonFull);
                Tuple<bool, double> somaRes = neuron.soma.newFlow(sender, e, toPush, axonFull, color);
                if (somaRes.Item1 && !axonFull)
                {
                    Tuple<bool, double> axonRes = neuron.axon.newFlow(sender, e, somaRes.Item2, color);
                    volumeToPushNext = axonRes.Item2;
                    neuron.volumeToPush = volumeToPushNext;
                }
                else if (somaRes.Item1 && axonFull)
                {
                    neuron.isFull = true;
                    neuron.volumeToPush = somaRes.Item2;
                }
            }
            return volumeToPushNext;
        }

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

        private void myTimerTick(object sender, EventArgs e)
        {
            Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            Console.WriteLine(this.timeOffset);
            Console.WriteLine(DateTime.Now - this.TimerStart);
            Console.WriteLine(DateTime.Now - this.TimerStart - this.timeOffset);
            TimeSpan currentValue = DateTime.Now - this.TimerStart - this.timeOffset;
            this.timerTextBlock.Text = currentValue.ToString(@"mm\:ss");
        }


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
                reminderButon.IsEnabled = true;
                resultButton.IsEnabled = true;

                List<Viewbox> visited = new List<Viewbox>();
                foreach (List<Viewbox> elList in this.neuronQueue)
                {
                    for (int i = elList.Count - 1; i >= 0; i--)
                    {
                        if (!visited.Contains(elList[i]))
                        {
                            Neuron neuron = (Neuron)elList[i].Child;
                            neuron.unload();
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
                this.drainingTimer.Start();

            this.remindStarted = false;
            this.color = System.Windows.Media.Brushes.DodgerBlue;

        }

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


        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop(false);
            this.dropCanvas.Children.Clear();
            this.neuronQueue.Clear();
            this.canvasElements.Clear();
            timerTextBlock.Text = "00:00";
            this.drainingTimer.Stop();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop(false);
            this.timeOffset = TimeSpan.Parse("00:00:00");
            this.drainingTimer.Stop();
        }

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

        private void getConfParamsXML(string path, double time, double flow, double drainingSpeed)
        {
            this.currentConf = path;
            Console.WriteLine("In main Window" + path);
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            this.timer.Stop();
            this.pauseFlow = true;
            startButton.IsEnabled = true;
        }

        private void resetFlowButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop(false);
            foreach (Viewbox viewbox in this.canvasElements.Keys)
            {
                Neuron neuron = (Neuron)viewbox.Child;
                neuron.reset();
                neuron.isFull = false;
            }
            timerTextBlock.Text = "00:00";
            this.blockNeuronsDendrites();
            this.pauseFlow = false;
            this.drainingTimer.Stop();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.timer.Stop();

        }

        private void reminderButon_Click(object sender, RoutedEventArgs e)
        {
            this.drainingTimer.Stop();
            this.blockTheEnd = false;
            this.counter = 0;
            this.color = System.Windows.Media.Brushes.Maroon;
            this.timerTextBlock.Text = "00:00";
            this.TimerStart = DateTime.Now;
            this.timer.Start();
            this.remindStarted = true;
            this.reminderButon.IsEnabled = false;
            this.startButton.IsEnabled = false;

        }

        private void resultButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
