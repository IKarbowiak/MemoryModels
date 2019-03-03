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
using System.Threading;

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
        private Point startPosition;
        private DragAndDropPanel parentWindow;
        public double[] lastPosition { get; set; }

        public NeuronViewbox(int den_number)
        {
            Console.WriteLine("Here");
            InitializeComponent();
            this.neuron = new Neuron(den_number);
            this.viewbox.Child = this.neuron;
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

            this.startPosition = e.GetPosition(parentWindow.dropCanvas);
            viewbox.CaptureMouse();
            Console.WriteLine("In Mouse Down");
            viewbox.MouseMove += neuron_MouseMove;
            viewbox.MouseUp += neuron_MouseUp;
        }

        private double[] adjustPosition(Point position)
        {
            var newX = position.X - (viewbox.Width / 2);
            var newY = position.Y - (viewbox.Height / 2);

            double[] res = this.changePositionIfLeftCanvas(newX, newY);

            return res;
        }

        private double[] changePositionIfLeftCanvas(double newX, double newY)
        {
            if (newX < 0) newX = 0;
            if (newX > maxX) newX = maxX;

            if (newY < 0) newY = 0;
            if (newY > maxY) newY = maxY;

            double[] res = { newX, newY };
            return res;
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
            transform = new TranslateTransform();
            viewbox.RenderTransform = transform;

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

            Point position = e.GetPosition(this.parentWindow.dropCanvas);
            this.lastPosition = this.adjustPosition(position);

            this.parentWindow.after_mouseUp(this);

        }

        // remove ability to move neuron in neuron panel
        public void removeViewboxAbilityToMove()
        {
            Console.WriteLine("Remove ability");
            this.viewbox.MouseDown -= this.neuron_MouseDown;
            this.viewbox.MouseMove -= this.neuron_MouseMove;
            this.viewbox.MouseUp -= this.neuron_MouseUp;
        }

        public void enableViewboxMoving()
        {
            this.viewbox.MouseDown += this.neuron_MouseDown;
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
        public void backToPreviousPosition()
        {
            TranslateTransform transform = viewbox.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                viewbox.RenderTransform = transform;
            }

            transform.X = this.startPosition.X;
            transform.Y = this.startPosition.Y;

            //viewbox.SetValue(Canvas.LeftProperty, this.startPosition[0]);
            //viewbox.SetValue(Canvas.RightProperty, this.startPosition[1]);
            //viewbox.SetValue(Canvas.TopProperty, this.startPosition[2]);
            //viewbox.SetValue(Canvas.BottomProperty, this.startPosition[3]);
        }

        // check if neuron quit border of neuron panel
        public bool checkIfQuitBorder()
        {   
            double newX = lastPosition[0] - (viewbox.Width / 2);
            double newY = lastPosition[1] - (viewbox.Height / 2);

            bool quit = false;
            if (newX < 0) { newX = 0; quit = true; }
            if (newX > maxX) { newX = maxX; quit = true; }

            if (newY < 0) { newY = 0; quit = true; }
            if (newY > maxY) { newY = maxY; quit = true; }

            TranslateTransform transform = viewbox.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                viewbox.RenderTransform = transform;
            }
            if (quit)
            {
                transform.X = newX;
                transform.Y = newY;
            }

            return quit;

        }

        // set params from xml to neuron, function is used by loadParams()
        public void setNeuronParams(List<double> params_list)
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

        // put fluid to neuron
        public double neuronFlow(object sender, EventArgs e, List<double> flowList, System.Windows.Media.SolidColorBrush color)
        {

            double toPush = 0;
            double volumeToPushNext = 0;
            Console.WriteLine("In neuron flow");
            // if there is no dendrite in neuron
            if (neuron.dendrites_list.Count() == 0)
            {
                Tuple<bool, double> axonRes = neuron.axon.newFlow(sender, e, flowList[0], color);
                volumeToPushNext = axonRes.Item2;
                this.parentWindow.setOutFlowParameters(this, axonRes.Item2);
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
                    this.parentWindow.setOutFlowParameters(this, axonRes.Item2);
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

        public void changePosition(double X, double Y, string site)
        {
            double newX;
            if (site == "left")
                newX = X - viewbox.Width;
            else
                newX = X;

            TranslateTransform transform = viewbox.RenderTransform as TranslateTransform;

            transform = new TranslateTransform();
            viewbox.RenderTransform = transform;

            transform.X = newX;
            transform.Y = Y;

        }


        public void resetNeuron()
        {
            neuron.reset();
            neuron.isFull = false;
        }

        public double getNumberOfDendrites()
        {
            return neuron.dendrites_list.Count();
        }

        public void openDendrites()
        {
            foreach (Dendrite den in this.neuron.dendrites_list)
                den.isBlocked = false;
        }

        public void openOneDendrite(string side)
        {
            if (side == "up")
                this.neuron.dendrites_list[0].isBlocked = false;
            else
                this.neuron.dendrites_list[1].isBlocked = false;
        }

        public void closeDendrites()
        {
            foreach (Dendrite den in this.neuron.dendrites_list)
            {
                den.isBlocked = true;
                Console.WriteLine("Block DEN!");
            }
        }

        public void ublockUpOrDownDendrite()
        {
            if (viewbox.Name == "up")
            {
                Console.WriteLine("Unblock Den");
                (neuron.dendrites_list[0]).isBlocked = false;
            }
            else if (viewbox.Name == "down")
            {
                Console.WriteLine("Unblock Den");
                (neuron.dendrites_list[1]).isBlocked = false;
            }

        }

        public double[] getCanvasParameters()
        {
            double[] parameters = { lastPosition[0], lastPosition[0] + viewbox.Width, lastPosition[1], lastPosition[1] + viewbox.Height };
            return parameters;
        }

        public bool drain(double drainingVolume)
        {
            return neuron.draining(drainingVolume);
        }

        public void blockAxonEnd()
        {
            neuron.axon.blockTheEnd = true;
            Console.WriteLine("set axon to block");
        }

        public void unblockAxonEnd()
        {
            neuron.axon.blockTheEnd = false;
            Console.WriteLine("set axon to unblock");
        }

        public void unloadNeuron(bool remindStarted)
        {
            neuron.unload(remindStarted);
            neuron.isFull = false;
            neuron.volumeToPush = 0;
        }

        public double getVolumeToOutflowWhenNeuronIsFull()
        {
            return neuron.volumeToOutFlowWhenNeuronFull;
        }

        public double getSomaThreshold()
        {
            return neuron.soma.threshold;
        }

        public bool somaIsNull()
        {
            return neuron.soma == null;
        }

        public double getVolumeToPush()
        {
            return neuron.volumeToPush;
        }

        public void setVolumeToPush(double volume)
        {
            neuron.volumeToPush = volume;
        }

        public bool neuronIsFull()
        {
            return neuron.isFull;
        }
    }
}
