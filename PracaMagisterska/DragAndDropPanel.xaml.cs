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
        double maxX;
        double maxY;
        Dictionary<object, double[]> canvasElements;
        

        public DragAndDropPanel()
        {
            InitializeComponent();
            canvasElements = new Dictionary<object, double[]>();

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
            int catchValue = 30;
            int offset = 8;
            string neuronType = viewbox.Child.GetType().ToString().Split('.')[1];
            Console.WriteLine("In mouse up " + neuronType);
            if (neuronType == "Neuron1")
            {
                Neuron1 neuron1 = (Neuron1)viewbox.Child;
                //neuron1.flow((double)20, 8);
                double axonQidth = neuron1.axon.Width;
            }
            var neuron = viewbox.Child;
            
            viewbox.ReleaseMouseCapture();
            viewbox.MouseMove -= neuron_MouseMove;
            viewbox.MouseUp -= neuron_MouseUp;

            foreach(KeyValuePair<Object, Double[]> element in canvasElements)
            {
                Console.WriteLine(element.Key == viewbox);
                Console.WriteLine((Math.Abs(element.Value[2] - Canvas.GetTop(viewbox))));
                Console.WriteLine((Math.Abs(element.Value[3] - Canvas.GetBottom(viewbox))));

                if ((element.Key != viewbox) && ((Math.Abs(element.Value[2] - Canvas.GetTop(viewbox)) <= catchValue) || (Math.Abs(element.Value[3] - Canvas.GetBottom(viewbox)) <= catchValue)))
                {
                    Console.WriteLine(element.Key + "      " + element.Value);
                    Console.WriteLine("Inside");
                    if (Math.Abs(element.Value[0] - Canvas.GetRight(viewbox)) <= catchValue)
                    {
                        Console.WriteLine(Math.Abs(element.Value[0] - Canvas.GetRight(viewbox)));
                        Console.WriteLine(" Bee" + element.Value[0] + " " + Canvas.GetRight(viewbox));
                        Viewbox elBox = (Viewbox)element.Key;
                        Console.WriteLine(elBox.Child.GetType().ToString());
                        Console.WriteLine(elBox.Child.GetType().ToString().Split('.')[1] == "Neuron2");
                        if (elBox.Child.GetType().ToString().Split('.')[1] == "Neuron2")
                        {
                            
                            Console.WriteLine("Ne2");
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
                        viewbox.SetValue(Canvas.LeftProperty, element.Value[0] - viewbox.Width - 1);
                        viewbox.SetValue(Canvas.RightProperty, element.Value[0] - 1);
                        Console.WriteLine(Canvas.GetRight(viewbox));
                    }
                    if (Math.Abs(element.Value[1] - Canvas.GetLeft(viewbox)) <= catchValue)
                    {
                        Console.WriteLine(Math.Abs(element.Value[1] - Canvas.GetRight(viewbox)));
                        Console.WriteLine(" Bee" + element.Value[1] + " " + Canvas.GetRight(viewbox));
                        if (neuronType == "Neuron2")
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

                        viewbox.SetValue(Canvas.LeftProperty, element.Value[1] + 1);
                        viewbox.SetValue(Canvas.RightProperty, element.Value[1] + viewbox.Width + 1);

                    }

                }

            }


            double[] parameters = { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };
            canvasElements[sender] =  parameters;
            Console.WriteLine(canvasElements[sender][0]);
            Console.WriteLine("Dictionary count !!!!! " + canvasElements.Count());
            //Console.WriteLine(sender.GetType().Name);
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

    }
}
