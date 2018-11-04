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

        public Neuron2()
        {
            InitializeComponent();
            this.outFlowVolume = 0;

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

        public void flow(double time, double flow)
        {
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
                    Console.WriteLine("Den 1" + dendriteRes1);
                    Console.WriteLine("Den 2" + dendriteRes2);
                    Tuple<bool, double> somaRes = soma.newFlow(sender, e, dendriteRes1.Item2 + dendriteRes2.Item2);
                    if (somaRes.Item1)
                    {
                        Console.WriteLine(somaRes);
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
        }

        public void stopTimer(object sender, EventArgs e)
        {
            timer.Stop();
            Dispatcher.Invoke(() => { axon.unloadFunc(); });
            Dispatcher.Invoke(() => { soma.unloadFunc(); });
            Dispatcher.Invoke(() => { dendrite1.unloadFunc(); });
            Dispatcher.Invoke(() => { dendrite2.unloadFunc(); });
            timer2.Stop();
            this.outFlowVolume = axon.flowedOutVolume;
            Console.WriteLine(axon.flowedOutVolume);
            Console.WriteLine("Stop timer in Neuron2");
        }
    }
}
