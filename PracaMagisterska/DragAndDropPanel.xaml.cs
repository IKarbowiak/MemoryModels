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
    public static class ReflectionExt
    {
        public static object GetAttr(this object obj, string name)
        {
            Type type = obj.GetType();
            BindingFlags flags = BindingFlags.Instance |
                                     BindingFlags.Public |
                                     BindingFlags.GetProperty;

            return type.InvokeMember(name, flags, Type.DefaultBinder, obj, null);
        }
    }

    public partial class DragAndDropPanel : Window
    {
        private double maxX;
        private double maxY;
        private double[] startPosition;
        private Dictionary<object, double[]> canvasElements;
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        private List<List<object>> neuronQueue = new List<List<object>>();
        private int counter = 0;
        //private List<object> elementsToCheck = new List<object>();



        public DragAndDropPanel()
        {
            InitializeComponent();
            canvasElements = new Dictionary<object, double[]>();
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
            string neuronType = viewbox.Child.GetType().ToString().Split('.')[1];
            Console.WriteLine("In mouse up " + neuronType);
            if (neuronType == "Neuron1")
            {
                Neuron1 neuron1 = (Neuron1)viewbox.Child;
                //neuron1.flow((double)20, 8);
            }
            var neuron = viewbox.Child;


            viewbox.ReleaseMouseCapture();
            viewbox.MouseMove -= neuron_MouseMove;
            viewbox.MouseUp -= neuron_MouseUp;

            void set_TopAndBottom_Property(KeyValuePair<Object, Double[]> element, bool condition)
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
                    for (int i=0; i < this.neuronQueue.Count(); i++)
                    {
                        for (int j=0; j < this.neuronQueue[i].Count(); j++)
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
                foreach (KeyValuePair<Object, Double[]> element in canvasElements)
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
                            set_TopAndBottom_Property(element, (elBox.Child.GetType().ToString().Split('.')[1] == "Neuron2"));

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
                                    List<object> elements = new List<object>();
                                    elements.Add(viewbox);
                                    elements.Add(elBox);
                                    this.neuronQueue.Add(elements);
                                }
                                else if (checkEl.Count() > 0)
                                //else if (this.neuronQueue.Contains(el_list))
                                {
                                    if (checkEl[1] > 0)
                                    {
                                        List<object> elements = new List<object>();
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
                            set_TopAndBottom_Property(element, (neuronType == "Neuron2"));
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
                                    List<object> elements = new List<object>();
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
                    for (int x = elInList; x > -1 ; x--)
                    {
                        this.neuronQueue[list_num].RemoveAt(x);
                    }
                }

            }


            Console.WriteLine("List count " + this.neuronQueue.Count());
            double[] parameters = { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };
            canvasElements[sender] =  parameters;
            Console.WriteLine(canvasElements[sender][0]);
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

        private void create_neuron0(object sender, MouseButtonEventArgs e)
        {
            Viewbox viewbox = new Viewbox() { StretchDirection = StretchDirection.Both, Stretch = Stretch.Uniform, Height = 40, Width = 250};
            Neuron0 newNeuron = new Neuron0();
            viewbox.Child = newNeuron;
            viewbox.MouseDown += new MouseButtonEventHandler(this.neuron_MouseDown);
            dropCanvas.Children.Add(viewbox);
        }

        private void create_neuron1(object sender, MouseButtonEventArgs e)
        {
            Viewbox viewbox = new Viewbox() { StretchDirection = StretchDirection.Both, Stretch = Stretch.Uniform, Height = 40, Width = 250 };
            Neuron1 newNeuron = new Neuron1();
            int gridWidth = (int)newNeuron.neuronGrid.Width;
            
            viewbox.Child = newNeuron;
            viewbox.MouseDown += new MouseButtonEventHandler(this.neuron_MouseDown);
            dropCanvas.Children.Add(viewbox);
        }

        private void create_neuron2(object sender, MouseButtonEventArgs e)
        {
            Viewbox viewbox = new Viewbox() { StretchDirection = StretchDirection.Both, Stretch = Stretch.Uniform, Height = 40, Width = 250};
            Neuron2 newNeuron = new Neuron2();
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
            Dictionary<object, double> whatToPush = new Dictionary<object, double>();

            if (this.neuronQueue.Count() == 0 && this.canvasElements.Count > 0)
            {
                foreach (KeyValuePair<object, double[]> element in this.canvasElements)
                {
                    whatToPush[element.Key] = 8;
                }

            }

            for (int i = 0; i < this.neuronQueue.Count(); i++)
            {
                Console.WriteLine("In flow 2");
                if (this.neuronQueue[i].Count() > 0)
                {
                    object first_el = this.neuronQueue[i][0];
                    if (!whatToPush.ContainsKey(first_el))
                    {
                        whatToPush.Add(first_el, 8);
                    }

                    double toPush = 0;

                    for (int j = 1; j < this.neuronQueue[i].Count(); j++)
                    {
                        toPush = 0;
                        object previous_el = this.neuronQueue[i][j - 1];
                        Viewbox viewbox_prev = (Viewbox)previous_el;
                        string neuronType = viewbox_prev.Child.GetType().Name;
                        if (neuronType == "Neuron0")
                        {
                            Neuron0 neuron = (Neuron0)viewbox_prev.Child;
                            toPush += neuron.volumeToPush;
                        }
                        else if (neuronType == "Neuron1")
                        {
                            Neuron1 neuron = (Neuron1)viewbox_prev.Child;
                            toPush += neuron.volumeToPush;
                        }
                        else
                        {
                            Neuron2 neuron = (Neuron2)viewbox_prev.Child;
                            toPush += neuron.volumeToPush;
                        }

                        if (toPush > 0)
                        {
                            object nextEl = this.neuronQueue[i][j];
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


            foreach (KeyValuePair<object, double> element in whatToPush)
            {
                Viewbox viewbox_el = (Viewbox)element.Key;
                string neuronType = viewbox_el.Child.GetType().Name;
                //object neuron = viewbox_el.Child;
                //bool isFlow = (bool)neuron.GetAttr("isFlow");

                if (neuronType == "Neuron0")
                {
                    Neuron0 neuron = (Neuron0)viewbox_el.Child;
                    neuron.flowVolume = element.Value;

                    if (!neuron.isFlow)
                    {
                        Console.WriteLine("N0 flow");
                        neuron.flow(50);
                    }

                }
                else if (neuronType == "Neuron1")
                {
                    Neuron1 neuron = (Neuron1)viewbox_el.Child;
                    neuron.flowVolume = element.Value;
                    if (!neuron.isFlow)
                    {
                        Console.WriteLine("N1 flow");
                        neuron.flow(50);
                    }
                }
                else
                {
                    Neuron2 neuron = (Neuron2)viewbox_el.Child;
                    neuron.flowVolume = element.Value;

                    if (!neuron.isFlow)
                    {
                        Console.WriteLine("N2 flow");
                        neuron.flow(50);
                    }

                }
            }
            

        }

        private void stop(bool fromTimer)
        {
            Console.WriteLine("In stop!!!!!!!!!!!");
            timer.Stop();
            timer2.Stop();
            startButton.IsEnabled = true;

            foreach (KeyValuePair<Object, Double[]> element in canvasElements)
            {
                Viewbox viewbox_el = (Viewbox)element.Key;
                string neuronType = viewbox_el.Child.GetType().Name;

                if (neuronType == "Neuron0")
                {
                    Neuron0 neuron = (Neuron0)viewbox_el.Child;
                    if (neuron.isFlow)
                    {
                        if (fromTimer)
                        {
                            neuron.stopTimer();
                        }
                        else
                        {
                            neuron.stopFlow();
                        }
                        Console.WriteLine("Stop neuron!!!!!!!!!");
                    }

                }
                else if (neuronType == "Neuron1")
                {
                    Neuron1 neuron = (Neuron1)viewbox_el.Child;
                    if (neuron.isFlow)
                    {
                        if (fromTimer)
                        {
                            neuron.stopTimer();
                        }
                        else
                        {
                            neuron.stopFlow();
                        }
                        Console.WriteLine("Stop neuron!!!!!!!!!");
                    }

                }
                else
                {
                    Neuron2 neuron = (Neuron2)viewbox_el.Child;
                    if (neuron.isFlow)
                    {
                        if (fromTimer)
                        {
                            neuron.stopTimer();
                        }
                        else
                        {
                            neuron.stopFlow();
                        }
                        Console.WriteLine("Stop neuron!!!!!!!!!");
                    }

                }
            }
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
