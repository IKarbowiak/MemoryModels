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
    /// Interaction logic for Neuron2.xaml
    /// </summary>
    public partial class Neuron2 : UserControl
    {
        private Dendrite dendrite1;
        private Dendrite dendrite2;
        private Soma soma;
        private Axon axon;
        public double neuronLength { get; set; }
        public double denDiam { get; set; }
        public double axDiam { get; set; }
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        public double outFlowVolume { get; set; }
        public double volumeToPush { get; set; }
        public double flowVolume { get; set; }
        public bool isFlow { get; set; }

        public Neuron2()
        {
            InitializeComponent();
            this.outFlowVolume = 0;
            this.flowVolume = 0;
            this.isFlow = false;

            dendrite1 = new Dendrite(false);
            dendrite1.HorizontalAlignment = HorizontalAlignment.Left;
            dendrite1.VerticalAlignment = VerticalAlignment.Top;
            dendrite1.Margin = new System.Windows.Thickness(0, 12, 0, 0);
            Grid.SetColumn(dendrite1, 1);
            Grid.SetRow(dendrite1, 1);
            neuronGrid.Children.Add(dendrite1);

            InitializeComponent();
            dendrite2 = new Dendrite(false);
            dendrite2.HorizontalAlignment = HorizontalAlignment.Left;
            dendrite2.VerticalAlignment = VerticalAlignment.Bottom;
            dendrite2.Margin = new System.Windows.Thickness(0, 0, 0,12);
            Grid.SetColumn(dendrite2, 1);
            Grid.SetRow(dendrite2, 1);
            neuronGrid.Children.Add(dendrite2);

            axon = new Axon(false);
            axon.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(axon, 1);
            Grid.SetRow(axon, 1);
            neuronGrid.Children.Add(axon);

            soma = new Soma(false);
            soma.HorizontalAlignment = HorizontalAlignment.Center;
            soma.Margin = new System.Windows.Thickness(0, 0, 190, 0);
            Grid.SetColumn(soma, 1);
            Grid.SetRow(soma, 1);
            neuronGrid.Children.Add(soma);
        }

        public void flow(double time, double flow = 0)
        {
            if (flow == 0)
            {
                flow = this.flowVolume;
            }
            this.outFlowVolume = 0;
            dendrite1.length = this.neuronLength / 26;
            dendrite1.diameter = denDiam;

            dendrite2.length = this.neuronLength / 26;
            dendrite2.diameter = denDiam;

            axon.length = this.neuronLength * 20 / 26;
            axon.diameter = this.axDiam;

            soma.diameter = this.neuronLength * 10 / 26;
            soma.axonDiameter = this.axDiam;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) =>
            {
                Tuple<bool, double> dendriteRes1 = dendrite1.newFlow(sender, e, flow, soma, axon);
                Tuple<bool, double> dendriteRes2 = dendrite2.newFlow(sender, e, flow, soma, axon);
                if (dendriteRes1.Item1 | dendriteRes2.Item1)
                {
                    Tuple<bool, double> somaRes = soma.newFlow(sender, e, dendriteRes1.Item2 + dendriteRes2.Item2);
                    if (somaRes.Item1)
                    {
                        this.volumeToPush = axon.newFlow(sender, e, somaRes.Item2);
                    }
                }
            };
            timer.Start();

            timer2 = new System.Windows.Threading.DispatcherTimer();
            timer2.Interval = TimeSpan.FromSeconds(time);
            timer2.Tick += (sender, e) => { stopTimer(); };
            timer2.Start();
            this.isFlow = true;
            //dendrite.flow(time, speed);
            //soma.flow(time, speed, this.axon);
        }

        public void reset()
        {
            axon.reset();
            soma.reset();
            dendrite1.reset();
            dendrite2.reset();
        }


        public void stopFlow()
        {
            timer.Stop();
            axon.stop();
            dendrite1.stop();
            dendrite2.stop();
            timer2.Stop();
            this.isFlow = false;
        }

        public void continueFlow(int time)
        {
            timer.Start();
            timer2.Interval = TimeSpan.FromSeconds(time);
            timer2.Start();
        }

        public void stopTimer()
        {
            timer.Stop();
            Dispatcher.Invoke(() => { axon.unloadFunc(); });
            Dispatcher.Invoke(() => { soma.unloadFunc(); });
            Dispatcher.Invoke(() => { dendrite1.unloadFunc(); });
            Dispatcher.Invoke(() => { dendrite2.unloadFunc(); });
            timer2.Stop();
            this.outFlowVolume = axon.flowedOutVolume;
            this.isFlow = false;

        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject data = new DataObject();
                Neuron2 neurus = new Neuron2();
                data.SetData("Object", this);
                data.SetData("Model", "Model2");

                // Inititate the drag-and-drop operation.
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            Console.WriteLine("On drop");
            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent("Model"))
            {
                string dataString = (string)e.Data.GetData("Model");
                Console.WriteLine(dataString);
                // If the string can be converted into a Brush, 
                // convert it and apply it to the ellipse.

                // Set Effects to notify the drag source what effect
                // the drag-and-drop operation had.
                // (Copy if CTRL is pressed; otherwise, move.)
                //if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                //{
                //    e.Effects = DragDropEffects.Copy;
                //}
                
                e.Effects = DragDropEffects.Move;
                

            }
            e.Handled = true;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            // Save the current Fill brush so that you can revert back to this value in DragLeave.
            Console.WriteLine("On drag enter");
            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent("Model"))
            {
                string dataString = (string)e.Data.GetData("Model");

                Console.WriteLine(dataString);
            }
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            // Undo the preview that was applied in OnDragEnter.
            Console.WriteLine("Leave");
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            e.Effects = DragDropEffects.None;

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

                // If the string can be converted into a Brush, allow copying or moving.
                BrushConverter converter = new BrushConverter();
                if (converter.IsValid(dataString))
                {
                    // Set Effects to notify the drag source what effect
                    // the drag-and-drop operation will have. These values are 
                    // used by the drag source's GiveFeedback event handler.
                    // (Copy if CTRL is pressed; otherwise, move.)
                    if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                }
            }
            e.Handled = true;
        }
    }
}
