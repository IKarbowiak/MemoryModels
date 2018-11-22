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
        public Dendrite dendrite1 { get; set; }
        public Dendrite dendrite2 { get; set; }
        public Soma soma { get; set; }
        public Axon axon { get; set; }
        public double neuronLength { get; set; }
        public double denDiam { get; set; }
        public double axDiam { get; set; }
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
            this.isFlow = false;
        }

        public void continueFlow(int time)
        {

        }

        public void stopTimer()
        {
            Dispatcher.Invoke(() => { axon.unloadFunc(); });
            Dispatcher.Invoke(() => { soma.unloadFunc(); });
            Dispatcher.Invoke(() => { dendrite1.unloadFunc(); });
            Dispatcher.Invoke(() => { dendrite2.unloadFunc(); });
            this.outFlowVolume = axon.flowedOutVolume;
            this.isFlow = false;

        }

    }
}
