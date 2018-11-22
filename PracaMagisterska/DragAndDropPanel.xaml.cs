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
        private Dictionary<Viewbox, double[]> canvasElements;
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        private List<List<Viewbox>> neuronQueue = new List<List<Viewbox>>();
        private int counter = 0;
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
            int catchValue_updown = 10;
            int offset = 8;
            Neuron neuron = (Neuron)viewbox.Child;
            Console.WriteLine("In mouse up " );


            viewbox.ReleaseMouseCapture();
            viewbox.MouseMove -= neuron_MouseMove;
            viewbox.MouseUp -= neuron_MouseUp;

            void set_TopAndBottom_Property(KeyValuePair<Viewbox, Double[]> element, bool condition)
            {
                if (condition)
                {
                    if ((element.Value[2] - Canvas.GetTop(viewbox)) <= 0)
                    {
                        viewbox.SetValue(Canvas.TopProperty, element.Value[2] + offset);
                        viewbox.SetValue(Canvas.BottomProperty, element.Value[3] + offset);
                    }
                    else
                    {
                        viewbox.SetValue(Canvas.TopProperty, element.Value[2] - offset);
                        viewbox.SetValue(Canvas.BottomProperty, element.Value[3] - offset);
                    }
                }
                else
                {
                    viewbox.SetValue(Canvas.TopProperty, element.Value[2]);
                    viewbox.SetValue(Canvas.BottomProperty, element.Value[3]);
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

                    if ((element.Key != viewbox) && ((Math.Abs(element.Value[2] - Canvas.GetTop(viewbox)) <= catchValue_updown) || (Math.Abs(element.Value[3] - Canvas.GetBottom(viewbox)) <= catchValue_updown)))
                    {
                        //List<object> el_list = new List<object> { element.Key };

                        if (Math.Abs(element.Value[0] - Canvas.GetRight(viewbox)) <= catchValue_rightleft)
                        {
                            Viewbox elBox = (Viewbox)element.Key;
                            set_TopAndBottom_Property(element, ((Neuron)elBox.Child).dendrites_list.Count() > 1);

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
                            else if (!quit)
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
                                        List<Viewbox> elements = new List<Viewbox>();
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
                            set_TopAndBottom_Property(element, (neuron.dendrites_list.Count() > 1));
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

                                Console.WriteLine("Neuron Queue" + this.neuronQueue[0][0].GetType().ToString());

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

            Console.WriteLine(x + "  ; " + y + "  maxX" + maxX + "  maxY" + maxY + "get Left" + Canvas.GetLeft(viewbox) + " gwt Right " + Canvas.GetRight(viewbox));

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


            //if (Canvas.GetLeft(viewbox) < 0)
            //{
            //    quit = true;
            //    viewbox.SetValue(Canvas.LeftProperty, 0.0);
            //    viewbox.SetValue(Canvas.LeftProperty, viewbox.Width);
            //};

            //if (Canvas.GetRight(viewbox) > maxX)
            //{
            //    quit = true;
            //    viewbox.SetValue(Canvas.RightProperty, maxX);
            //    viewbox.SetValue(Canvas.LeftProperty, maxX - viewbox.Width);
            //};

            //if (Canvas.GetBottom(viewbox) < 0)
            //{
            //    quit = true;
            //    viewbox.SetValue(Canvas.BottomProperty, 0);
            //    viewbox.SetValue(Canvas.TopProperty, viewbox.Height);
            //}

            //if (Canvas.GetTop(viewbox) > maxY)
            //{
            //    quit = true;
            //    viewbox.SetValue(Canvas.TopProperty, maxY);
            //    viewbox.SetValue(Canvas.BottomProperty, maxY - viewbox.Height);
            //}

            return quit;
            
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

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (canvasElements.Count() > 0)
            {
                timer.Interval = TimeSpan.FromSeconds(1);
                timer2.Interval = TimeSpan.FromSeconds(60);

                timer.Tick += (sender1, e1) =>
                {
                    flow2(sender1, e1);
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

        private void flow2(object sender, EventArgs e)
        {
            Dictionary<Neuron, double> whatToPush = new Dictionary<Neuron, double>();

            if (this.neuronQueue.Count() == 0 && this.canvasElements.Count > 0)
            {
                foreach (KeyValuePair<Viewbox, double[]> element in this.canvasElements)
                {
                    whatToPush[(Neuron)(element.Key).Child] = 8;
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
                        whatToPush.Add(first_el, 8);
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
                            if (whatToPush.ContainsKey(nextEl))
                            {
                                whatToPush[nextEl] += toPush;
                            }
                            else
                            {
                                whatToPush.Add(nextEl, toPush);
                                    
                            }
                        }
                    }
                }
            }

            //void getPushValue<T>(T viewbox)
            //{
            //    //Viewbox viewbox = (Viewbox)element;
            //    T neuron = viewbox;
            //}


            foreach (KeyValuePair<Neuron, double> element in whatToPush)
            {
                //object neuron = viewbox_el.Child;
                //bool isFlow = (bool)neuron.GetAttr("isFlow");
                Neuron neuron = element.Key;
                int denNum = neuron.dendrites_list.Count();
                if (denNum == 0)
                {
                    if (!neuron.isFlow)
                    {
                        Console.WriteLine("N0 flow");
                        // TODO: funtion for flow in neuron 0
                    }

                }
                else if (denNum > 1)
                {
                    // TODO write function for flow in neuron1 and neuron 2
                }
            }
            

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
    }
}
