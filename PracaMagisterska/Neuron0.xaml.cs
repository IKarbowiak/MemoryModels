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
    /// Interaction logic for Neuron0.xaml
    /// </summary>
    public partial class Neuron0 : UserControl
    {
        private Axon axon;
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        public double outFlowVolume { get; set; }

        public Neuron0()
        {
            InitializeComponent();
            axon = new Axon(recWidth: 400);
            //axon.changeRecSize(380, 11);
            axon.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetColumn(axon, 0);
            Grid.SetRow(axon, 0);
            neuronGrid.Children.Add(axon);
            outFlowVolume = 0;
        }

        public void flow(double time, double flow)
        {
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => {
                axon.newFlow(sender, e, flow);
            };
            timer.Start();

            timer2 = new System.Windows.Threading.DispatcherTimer();
            timer2.Interval = TimeSpan.FromSeconds(time);
            timer2.Tick += (sender, e) => { stopTimer(sender, e); };
            timer2.Start();

            //dendrite.flow(time, speed);
            //soma.flow(time, speed, this.axon);
        }

        public void reset()
        {
            axon.reset();
        }

        public void stopFlow()
        {
            timer.Stop();
            axon.stop();
            timer2.Stop();
        }

        public void continueFlow(int time)
        {
            timer.Start();
            timer2.Interval = TimeSpan.FromSeconds(time);
            timer2.Start();
        }

        public void stopTimer(object sender, EventArgs e)
        {
            timer.Stop();
            axon.stop();
            axon.unloadFunc();
            timer2.Stop();
            this.outFlowVolume = axon.flowedOutVolume;
            Console.WriteLine(axon.flowedOutVolume);
            Console.WriteLine("Stop timer in Neuron");
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject data = new DataObject();
                Neuron0 neurus = new Neuron0();
                data.SetData("Object", this);
                data.SetData("Model", "Model0");

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
