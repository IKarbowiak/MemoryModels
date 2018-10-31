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
    /// Interaction logic for Neuron1.xaml
    /// </summary>
    public partial class Neuron1 : UserControl
    {

        private Dendrite dendrite;
        private Soma soma;
        private Axon axon;
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        private double outFlowVolume = 0;

        public Neuron1()
        {
            InitializeComponent();
            dendrite = new Dendrite(false);
            dendrite.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetColumn(dendrite, 1);
            Grid.SetRow(dendrite, 1);
            neuronGrid.Children.Add(dendrite);

            axon = new Axon(false);
            axon.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(axon, 1);
            Grid.SetRow(axon, 1);
            neuronGrid.Children.Add(axon);

            soma = new Soma(false);
            soma.HorizontalAlignment = HorizontalAlignment.Center;
            soma.Margin = new System.Windows.Thickness(0, 0, 84, 0);
            Grid.SetColumn(soma, 1);
            Grid.SetRow(soma, 1);
            neuronGrid.Children.Add(soma);
        }

        public void flow(double time, double speed)
        {
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => {Tuple<bool, double> dendriteRes = dendrite.newFlow(sender, e, 8, soma, axon);
                if (dendriteRes.Item1)
                {
                    Console.WriteLine(dendriteRes);
                    Console.WriteLine(dendriteRes.Item1);
                    Console.WriteLine(dendriteRes.Item2);
                    Tuple<bool, double> somaRes = soma.newFlow(sender, e, dendriteRes.Item2);
                    if (somaRes.Item1)
                    {
                        Console.WriteLine(somaRes);
                        axon.newFlow(sender, e, somaRes.Item2);
                    }
                }
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
            soma.unloadFunc();
            dendrite.unloadFunc();
            timer2.Stop();
            Console.WriteLine(axon.flowedOutVolume);
            Console.WriteLine("Stop timer in Neuron");
        }
    }
}
