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

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 
    public partial class DragAndDropPanel : Window
    {
        Point startPoint;
        string dragPanel;
        bool drag = false;
        private double maxX;
        private double maxY;
        private Dictionary<object, double[]> canvasElements;
        private System.Windows.Threading.DispatcherTimer timer;
        private List<List<object>> neuronQueue = new List<List<object>>();
        private int counter = 0;
        //private List<object> elementsToCheck = new List<object>();

        

        public DragAndDropPanel()
        {
            InitializeComponent();
            canvasElements = new Dictionary<object, double[]>();
            timer = new System.Windows.Threading.DispatcherTimer();

        }

      
        private void neuron_MouseDown(object sender, MouseButtonEventArgs e)
        {
            maxX = dropCanvas.ActualWidth - ((Viewbox)sender).Width;
            maxY = dropCanvas.ActualHeight - ((Viewbox)sender).Height;

            ((Viewbox)sender).CaptureMouse();
            Console.WriteLine("In Mouse Down");
            ((Viewbox)sender).MouseMove += neuron_MouseMove;
            ((Viewbox)sender).MouseUp += neuron_MouseUp;
        }

        private void neuron_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(dropCanvas);
            var newX = pos.X - (((Viewbox)sender).Width / 2);
            var newY = pos.Y - (((Viewbox)sender).Height / 2);

            if (newX < 0) newX = 0;
            if (newX > maxX) newX = maxX;

            if (newY < 0) newY = 0;
            if (newY > maxY) newY = maxY;

            ((Viewbox)sender).SetValue(Canvas.LeftProperty, newX);
            ((Viewbox)sender).SetValue(Canvas.RightProperty, newX + ((Viewbox)sender).Width);
            ((Viewbox)sender).SetValue(Canvas.TopProperty, newY);
            ((Viewbox)sender).SetValue(Canvas.BottomProperty, newX + ((Viewbox)sender).Height);

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

            void unpinNeuron()
            {

            }

            Tuple<bool, int> neuronQueueContainsCheck(object element)
            {
                if (this.neuronQueue.Count > 0)
                {
                    for (int i=0; i < this.neuronQueue.Count(); i++)
                    {
                        for (int j=0; j < this.neuronQueue[i].Count(); j++)
                        {
                            if (this.neuronQueue[i][j] == element)
                            {
                                return Tuple.Create(true, i);
                            }
                        }
                    }
                }
                return Tuple.Create(false, 0);
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
                        Console.WriteLine(element.Key + "      " + element.Value);
                        Console.WriteLine("Inside");
                        //List<object> el_list = new List<object> { element.Key };

                        if (Math.Abs(element.Value[0] - Canvas.GetRight(viewbox)) <= catchValue_rightleft)
                        {
                            Viewbox elBox = (Viewbox)element.Key;
                            set_TopAndBottom_Property(element, (elBox.Child.GetType().ToString().Split('.')[1] == "Neuron2"));
                            viewbox.SetValue(Canvas.LeftProperty, element.Value[0] - viewbox.Width - 1);
                            viewbox.SetValue(Canvas.RightProperty, element.Value[0] - 1);
                            Tuple<bool, int> checkEl = neuronQueueContainsCheck(element.Key);
                            if (this.neuronQueue.Count() == 0)
                            {
                                this.neuronQueue.Add(new List<object> { viewbox });
                                this.neuronQueue.Add(new List<object> { element.Key });
                            }
                            else if (checkEl.Item1)
                            //else if (this.neuronQueue.Contains(el_list))
                            {
                                this.neuronQueue.Insert(checkEl.Item2, new List<object> { viewbox });
                            }
                            Console.WriteLine("Neuron Queue" + this.neuronQueue[0][0].GetType().ToString());
                            return true;
                        }
                        else if (Math.Abs(element.Value[1] - Canvas.GetLeft(viewbox)) <= catchValue_rightleft)
                        {
                            set_TopAndBottom_Property(element, (neuronType == "Neuron2"));
                            Tuple<bool, int> checkEl = neuronQueueContainsCheck(element.Key);
                            if (this.neuronQueue.Count() == 0)
                            {
                                this.neuronQueue.Add(new List<object> { viewbox });
                                this.neuronQueue.Add(new List<object> { element.Key });
                            }
                            else if (checkEl.Item1)
                            {
                                this.neuronQueue.Insert(checkEl.Item2 + 1, new List<object> { viewbox });
                            }

                            viewbox.SetValue(Canvas.LeftProperty, element.Value[1] + 1);
                            viewbox.SetValue(Canvas.RightProperty, element.Value[1] + viewbox.Width + 1);
                            Console.WriteLine("Neuron Queue" + this.neuronQueue[0][0].GetType().ToString());

                            return true;

                        }

                    }

                }
                return false;
            };

            bool linked = linkNeuron();
            Tuple<bool, int> viewboxCheck = neuronQueueContainsCheck(viewbox);
            if (!linked && viewboxCheck.Item1)
            {
                Console.WriteLine("Delete from List");
                List<object> item = this.neuronQueue[viewboxCheck.Item2];
                if (item.Count() > 1)
                {
                    int position = item.IndexOf(viewbox);
                    this.neuronQueue[viewboxCheck.Item2].RemoveAt(position);
                }
                else
                {
                    this.neuronQueue.RemoveAt(viewboxCheck.Item2);
                }
            }



            double[] parameters = { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };
            canvasElements[sender] =  parameters;
            Console.WriteLine(canvasElements[sender][0]);
            Console.WriteLine("Dictionary count !!!!! " + canvasElements.Count());
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
            timer.Interval = TimeSpan.FromSeconds(0.5);

            timer.Tick += (sender1, e1) =>
            {
                flow(sender1, e1);
                
            };
            timer.Start();


            if (this.neuronQueue.Count > 0)
            {
                for (int i = 0; i < this.neuronQueue.Count(); i++)
                {
                    for (int j = 0; j < this.neuronQueue[i].Count(); j++)
                    {
                        Console.WriteLine(this.neuronQueue[i][j]);
                    }
                }
            }
            Console.WriteLine(this.neuronQueue.Count());

            if (this.dropCanvas.Children.Count == 1)
            {
                Viewbox viewbox = (Viewbox)this.dropCanvas.Children[0];
                string neuronType = viewbox.Child.GetType().ToString().Split('.')[1];
                if (neuronType == "Neuron0")
                {
                    Neuron0 neuron = (Neuron0)viewbox.Child;
                    neuron.flow((double)20, 8);
                }
                else if (neuronType == "Neuron1")
                {
                    Neuron1 neuron = (Neuron1)viewbox.Child;
                    neuron.flow((double)20, 8);
                }
                else
                {
                    Neuron2 neuron = (Neuron2)viewbox.Child;
                    neuron.flow((double)20, 8);
                }

            }
        }

        private void flow(object sender, EventArgs e)
        {
            Console.WriteLine(this.neuronQueue.Count());
            Console.WriteLine(this.neuronQueue.Count() > counter);
            double toPush = 0;

            if (counter > 0 && this.neuronQueue.Count() > counter - 1)
            {
                Console.WriteLine("In check Drag and Drop!!!!");
                List<Object> elementsToCheck = this.neuronQueue[counter - 1];
                for (int i = 0; i < elementsToCheck.Count(); i++)
                {
                    Viewbox viewbox = (Viewbox)elementsToCheck[i];
                    string neuronType = viewbox.Child.GetType().Name;
                    Console.Write(neuronType);
                    if (neuronType == "Neuron0")
                    {
                        Neuron0 neuron = (Neuron0)viewbox.Child;
                        toPush += neuron.volumeToPush;
                    }
                    else if (neuronType == "Neuron1")
                    {
                        Neuron1 neuron = (Neuron1)viewbox.Child;
                        neuron.flow((double)20, 8);
                        toPush += neuron.volumeToPush;
                    }
                    else
                    {
                        Neuron2 neuron = (Neuron2)viewbox.Child;
                        neuron.flow((double)20, 8);
                        toPush += neuron.volumeToPush;
                    }
                }
            }
            else if (this.neuronQueue.Count() > counter)
            {
                Console.WriteLine("In first flow Drag and Drop!!!");
                List<Object> element_list = this.neuronQueue[counter];
                this.push(element_list, 8, 20);
                counter++;
            }

            if (toPush > 0 && this.neuronQueue.Count() > counter)
            {
                Console.WriteLine("In push Drag and Drop!!!!");
                List<Object> element_list = this.neuronQueue[counter];
                this.push(element_list, toPush, 30);
                counter++;
            }


        }

        private void push(List<Object> element_list, double toPush, double time)
        {
            for (int i = 0; i < element_list.Count(); i++)
            {
                Viewbox viewbox = (Viewbox)element_list[i];
                string neuronType = viewbox.Child.GetType().Name;
                Console.Write(neuronType);

                if (neuronType == "Neuron0")
                {
                    Neuron0 neuron = (Neuron0)viewbox.Child;
                    neuron.flow(time, toPush);
                }
                else if (neuronType == "Neuron1")
                {
                    Neuron1 neuron = (Neuron1)viewbox.Child;
                    neuron.flow(time, toPush);
                }
                else
                {
                    Neuron2 neuron = (Neuron2)viewbox.Child;
                    neuron.flow(time, toPush);
                }

            }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            this.dropCanvas.Children.Clear();
            this.neuronQueue.Clear();
            this.canvasElements.Clear();
        }
    }
}
