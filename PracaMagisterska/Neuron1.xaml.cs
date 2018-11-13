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
        public double neuronLength { get; set; }
        public double denDiam { get; set; }
        public double axDiam { get; set; }
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        public double outFlowVolume { get; set; }
        Point startPoint;

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
            soma.Margin = new System.Windows.Thickness(0, 0, 190, 0);
            Grid.SetColumn(soma, 1);
            Grid.SetRow(soma, 1);
            neuronGrid.Children.Add(soma);

            this.outFlowVolume = 0;
        }


        public void flow(double time, double flow)
        {
            this.outFlowVolume = 0;
            dendrite.length = this.neuronLength / 26;
            dendrite.diameter = denDiam;

            axon.length = this.neuronLength * 20 / 26;
            axon.diameter = this.axDiam;

            soma.diameter = this.neuronLength * 10 / 26;
            soma.axonDiameter = this.axDiam;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => {Tuple<bool, double> dendriteRes = dendrite.newFlow(sender, e, flow, soma, axon);
                if (dendriteRes.Item1)
                {
                    Tuple<bool, double> somaRes = soma.newFlow(sender, e, dendriteRes.Item2);
                    if (somaRes.Item1)
                    {
                        axon.newFlow(sender, e, somaRes.Item2);
                    }
                }
            };
            timer.Start();

            timer2 = new System.Windows.Threading.DispatcherTimer();
            timer2.Interval = TimeSpan.FromSeconds(time);
            timer2.Tick += (sender, e) => { stopTimer(sender, e); };
            timer2.Start();

            //dendrite.flow(time, speed);
            //soma.flow(time, speed, this.axon);
        }
        public void stopFlow()
        {
            timer.Stop();
            axon.stop();
            dendrite.stop();
            timer2.Stop();
        }

        public void reset()
        {
            axon.reset();
            soma.reset();
            dendrite.reset();
        }

        public void continueFlow(int time)
        {
            Console.WriteLine("In neuron 0 continue");
            timer.Start();
            timer2.Interval = TimeSpan.FromSeconds(time);
            timer2.Start();
        }

        public void stopTimer(object sender, EventArgs e)
        {
            timer.Stop();
            Dispatcher.Invoke(() => { axon.unloadFunc(); });
            Dispatcher.Invoke(() => { soma.unloadFunc(); });
            Dispatcher.Invoke(() => { dendrite.unloadFunc(); });
            timer2.Stop();
            outFlowVolume = axon.flowedOutVolume;
            Console.WriteLine("Stop timer in Neuron1");
        }

     
    }
}
