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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

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
        private System.Windows.Threading.DispatcherTimer timer2;
        private List<List<Viewbox>> neuronQueue = new List<List<Viewbox>>();
        private int counter = 0;
        public string currentConf { get; set; }
        //private List<object> elementsToCheck = new List<object>();



        public DragAndDropPanel()
        {
            InitializeComponent();
            canvasElements = new Dictionary<Viewbox, double[]>();
            timer = new System.Windows.Threading.DispatcherTimer();
            timer2 = new System.Windows.Threading.DispatcherTimer();
            //Dictionary<string, double[]> check = new Dictionary<string, double[]>();
            //check.Add("a", new double[] {2.0, 3.0, 4.0});
            //Console.WriteLine(check["a"]);
            //bool res = this.checkDictValue(check.Values.ToArray());
            //Console.WriteLine(res);
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
            Console.WriteLine("In mouse up " );


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
                    //if (direction == "up") newDirection = "down";
                    //else if (direction == "down") newDirection = "up";
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
                        //List<object> el_list = new List<object> { element.Key };

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
                                //else if (this.neuronQueue.Contains(el_list))
                                {
                                    if (checkEl[1] > 0)
                                    {
                                        List<Viewbox> elements = new List<Viewbox>();   // fo pszerobienia
                                        elements.Add(viewbox);
                                        elements.Add(elBox);
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
            double flowVolume = (double)8 / (double)10;
            Console.WriteLine(flowVolume);
            if (canvasElements.Count() > 0)
            {
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer2.Interval = TimeSpan.FromSeconds(60);

                timer.Tick += (sender1, e1) =>
                {
                    flow(sender1, e1, flowVolume);
                    Console.WriteLine("In timer 1");
                };
                timer.Start();

                timer2.Tick += (sender1, e1) =>
                {
                    stop(true);
                };

                timer2.Start();

                startButton.IsEnabled = false;
            }
        }

        private void flow(object sender, EventArgs e, double flow)
        {

            Dictionary<Neuron, double> whatToPush = new Dictionary<Neuron, double>();

            if (this.neuronQueue.Count() == 0 && this.canvasElements.Count > 0)
            {
                foreach (KeyValuePair<Viewbox, double[]> element in this.canvasElements)
                {
                    whatToPush[(Neuron)(element.Key).Child] = flow;
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
                            foreach (Dendrite den in first_el.dendrites_list) den.isBlocked = false;
                        }
                        whatToPush.Add(first_el, flow);
                    }

                    double toPush = 0;

                    for (int j = 1; j < this.neuronQueue[i].Count(); j++)
                    {
                        toPush = 0;
                        Viewbox viewbox_prev = this.neuronQueue[i][j - 1];
                        Neuron neuron = (Neuron)viewbox_prev.Child;
                        toPush += neuron.volumeToPush;

                        if (toPush > 0)
                        {
                            Neuron nextEl = (Neuron)(this.neuronQueue[i][j]).Child;

                            if (whatToPush.ContainsKey(nextEl)) whatToPush[nextEl] += toPush; 
                            else whatToPush.Add(nextEl, toPush);

                            Console.WriteLine(viewbox_prev.Name);
                            if (viewbox_prev.Name == "up") ((Dendrite)nextEl.dendrites_list[0]).isBlocked = false;
                            else if (viewbox_prev.Name == "down") ((Dendrite)nextEl.dendrites_list[1]).isBlocked = false;
                        }
                    }
                }
            }

            foreach (KeyValuePair<Neuron, double> element in whatToPush)
            {
                Neuron neuron = element.Key;
                this.neuronFlow(sender, e, neuron, element.Value);
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


        private void stop(bool fromTimer)
        {
            Console.WriteLine("In stop!!!!!!!!!!!");
            timer.Stop();
            timer2.Stop();
            startButton.IsEnabled = true;

        }
   

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            this.dropCanvas.Children.Clear();
            this.neuronQueue.Clear();
            this.canvasElements.Clear();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            this.stop(false);
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

        private void getConfParamsXML(string path, double time, double flow)
        {
            this.currentConf = path;
            Console.WriteLine("In main Window" + path);
        }

    }
}
