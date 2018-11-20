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
        public Axon axon { get; set; }
        public double outFlowVolume { get; set; }
        public double volumeToPush { get; set; }
        public double flowVolume { get; set; }
        public bool isFlow { get; set; }

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
            this.flowVolume = 0;
            this.isFlow = false;
        }

        public void flow(double time, double flow = 0)
        {
            if (flow == 0)
            {
                flow = this.flowVolume;
            }
            this.isFlow = true;

            //dendrite.flow(time, speed);
            //soma.flow(time, speed, this.axon);
        }

        public void reset()
        {
            axon.reset();
            this.isFlow = false;
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
            axon.unloadFunc();
            this.outFlowVolume = axon.flowedOutVolume;
            Console.WriteLine(axon.flowedOutVolume);
            Console.WriteLine("Stop timer in Neuron");
            this.isFlow = false;
        }

    }
}
