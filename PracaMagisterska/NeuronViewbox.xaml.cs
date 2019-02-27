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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for NeuronViewbox.xaml
    /// </summary>
    public partial class NeuronViewbox : UserControl
    {
        public Neuron neuron { get; set; }
        private double maxX;
        private double maxY;
        private double[] startPosition;
        private DragAndDropPanel parentWindow;

        public NeuronViewbox(int den_number)
        {
            Console.WriteLine("Here");
            InitializeComponent();
            //this.viewbox = new Viewbox() { StretchDirection = StretchDirection.Both, Stretch = Stretch.Uniform, Height = 40, Width = 250 };
            this.neuron = new Neuron(den_number);
            this.viewbox.Child = this.neuron;
            //viewbox.Child = this.newNeuron;
            this.viewbox.MouseDown += new MouseButtonEventHandler(this.neuron_MouseDown);
        }

        private void getParentWindow()
        {
            parentWindow = (DragAndDropPanel)Window.GetWindow(this);
        }

        // add mause up an mause move function to element when it will be clicked and set starting position of clicked object
        private void neuron_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Viewbox viewbox = sender as Viewbox;
            if (this.parentWindow == null)
                this.getParentWindow();
            Canvas canvas = parentWindow.dropCanvas;
            maxX = parentWindow.dropCanvas.ActualWidth - viewbox.Width;
            maxY = parentWindow.dropCanvas.ActualHeight - viewbox.Height;

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
            Canvas canvas = parentWindow.dropCanvas;
            var pos = e.GetPosition(parentWindow.dropCanvas);
            var newX = pos.X - (viewbox.Width / 2);
            var newY = pos.Y - (viewbox.Height / 2);

            if (newX < 0) newX = 0;
            if (newX > maxX) newX = maxX;

            if (newY < 0) newY = 0;
            if (newY > maxY) newY = maxY;


            TranslateTransform transform = viewbox.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                viewbox.RenderTransform = transform;
            }

            transform.X = newX;
            transform.Y = newY;

        }

        // try to link neuron to queue after mouse up from neuron
        private void neuron_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Viewbox viewbox = (Viewbox)sender;
            Canvas canvas = parentWindow.dropCanvas;
            Neuron neuron = (Neuron)viewbox.Child;
            Console.WriteLine("In mouse up ");


            viewbox.ReleaseMouseCapture();
            viewbox.MouseMove -= neuron_MouseMove;
            viewbox.MouseUp -= neuron_MouseUp;

            Console.WriteLine(parentWindow.dropCanvas.Children.ToString());
            Console.WriteLine(viewbox.Parent);
            Console.WriteLine();
            

            //bool linked = this.linkNeuron(viewbox, neuron, e);
            //List<int> checkList = neuronQueueContainsCheck(viewbox);

            //if (!linked)
            //{
            //    //TODO: function to finish - it's finished probably
            //    this.checkIfNeuronLeaveQueue(viewbox);
            //}

            //double[] parameters = { Canvas.GetLeft(viewbox), Canvas.GetRight(viewbox), Canvas.GetTop(viewbox), Canvas.GetBottom(viewbox) };
            //canvasElements[(Viewbox)sender] = parameters;


            //Console.WriteLine("List count " + this.neuronQueue.Count());
            //Console.WriteLine(canvasElements[(Viewbox)sender][0]);
            //Console.WriteLine("Dictionary count !!!!! " + canvasElements.Count());

            //// Check to remove 
            //counter = 0;
            //foreach (List<Viewbox> el in this.neuronQueue)
            //{
            //    Console.WriteLine("List " + counter);
            //    Console.WriteLine(el.Count());
            //    counter++;
            //}
        }

        // remove ability to move neuron in neuron panel
        private void removeAbilityToMove()
        {
            // List<Viewbox> checkList
            //if (checkList.Count() > 1)
            //{
            //    Viewbox viewbox = checkList[checkList.Count() - 1];
            Console.WriteLine("Remove ability");
            this.viewbox.MouseDown -= this.neuron_MouseDown;
            this.viewbox.MouseMove -= this.neuron_MouseMove;
            this.viewbox.MouseUp -= this.neuron_MouseUp;
        }

        private void enableViewboxMoving()
        {
            this.viewbox.MouseDown -= this.neuron_MouseDown;
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

        private void blockNeuronsDendrites()
        {
            foreach (Dendrite dendrite in neuron.dendrites_list)
            {
                Console.WriteLine("Block DEN!");
                dendrite.isBlocked = true;
            }
        }

        private void resetNeuron()
        {
            neuron.reset();
            neuron.isFull = false;
        }

    }
}
