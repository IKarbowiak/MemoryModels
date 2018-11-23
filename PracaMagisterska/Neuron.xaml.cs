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
    /// Interaction logic for Neuron.xaml
    /// </summary>
    public partial class Neuron : UserControl
    {
        public List<Dendrite> dendrites_list { get; set; }
        public Soma soma { get; set; }
        public Axon axon { get; set; }
        public double neuronLength { get; set; }
        public double denDiam { get; set; }
        public double axDiam { get; set; }
        public double outFlowVolume { get; set; }
        public double volumeToPush { get; set; }
        public double flowVolume { get; set; }
        public bool isFlow { get; set; }
        public string model { get; set;  }

        public Neuron(int dendrideNum)
        {
            InitializeComponent();
            this.dendrites_list = new List<Dendrite>();
            if (dendrideNum == 0)
            {
                Console.WriteLine("0 den");
                axon = new Axon(recWidth: 380);
                axon.HorizontalAlignment = HorizontalAlignment.Left;
                neuronGrid.Children.Add(axon);
                this.model = "Model" + dendrideNum;
            }
            else if (dendrideNum > 0)
            {
                if (dendrideNum == 1)
                {
                    Dendrite dendrite = new Dendrite(false);
                    dendrite.HorizontalAlignment = HorizontalAlignment.Left;

                    neuronGrid.Children.Add(dendrite);
                    this.dendrites_list.Add(dendrite);
                }
                else
                {
                    for (int i = 0; i < dendrideNum; i++)
                    {
                        Dendrite dendrite = new Dendrite(false);
                        dendrite.isBlocked = true;
                        dendrite.HorizontalAlignment = HorizontalAlignment.Left;
                        if (i % 2 == 0)
                        {
                            dendrite.VerticalAlignment = VerticalAlignment.Top;
                            dendrite.Margin = new System.Windows.Thickness(0, 12, 0, 0);
                        }
                        else
                        {
                            dendrite.VerticalAlignment = VerticalAlignment.Bottom;
                            dendrite.Margin = new System.Windows.Thickness(0, 0, 0, 12);
                        }

                        neuronGrid.Children.Add(dendrite);
                        this.dendrites_list.Add(dendrite);
                    }
                }


                axon = new Axon(false);
                axon.HorizontalAlignment = HorizontalAlignment.Right;
                neuronGrid.Children.Add(axon);

                soma = new Soma(false);
                soma.HorizontalAlignment = HorizontalAlignment.Center;
                soma.Margin = new System.Windows.Thickness(0, 0, 190, 0);
                neuronGrid.Children.Add(soma);

                this.model = "Model" + dendrideNum;
                //this.setParameters(this.neuronLength); //
                outFlowVolume = 0;
                this.flowVolume = 0;
                this.isFlow = false;
            }
            else
            {
                Console.WriteLine("You must specify number of dendrites - 0 i sminimum");
            }

        }

        public void setParameters(double neuronLen = 40)
        {
            this.outFlowVolume = 0;
            foreach (Dendrite den in this.dendrites_list)
            {
                den.length = this.neuronLength / 26;
                den.diameter = denDiam;
                den.calculateParameters();
            }

            axon.length = this.neuronLength * 20 / 26;
            axon.diameter = this.axDiam;
            axon.calculateParameters();

            soma.diameter = this.neuronLength * 10 / 26;
            soma.axonDiameter = this.axDiam;
            soma.calculateParameters();
        }
    }

}
