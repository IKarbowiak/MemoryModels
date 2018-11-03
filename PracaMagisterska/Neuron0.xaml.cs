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
        private double outFlowVolume = 0;

        public Neuron0()
        {
            InitializeComponent();
            axon = new Axon(false);
            axon.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(axon, 0);
            Grid.SetRow(axon, 0);
            neuronGrid.Children.Add(axon);
        }

        public void flow(double time, double speed)
        {
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => {
                axon.newFlow(sender, e, 8);
            };
            timer.Start();

            timer2 = new System.Windows.Threading.DispatcherTimer();
            timer2.Interval = TimeSpan.FromSeconds(20);
            timer2.Tick += (sender, e) => { stopTimer(sender, e); };
            timer2.Start();

            //dendrite.flow(time, speed);
            //soma.flow(time, speed, this.axon);
        }

        public void stopTimer(object sender, EventArgs e)
        {
            timer.Stop();
            axon.unloadFunc();
            timer2.Stop();
            Console.WriteLine(axon.flowedOutVolume);
            Console.WriteLine("Stop timer in Neuron");
        }
    }
}
