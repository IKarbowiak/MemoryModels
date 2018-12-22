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
    /// Interaction logic for Neuron.xaml
    /// </summary>
    public partial class Neuron : UserControl
    {
        public List<Dendrite> dendrites_list { get; set; }
        public Soma soma { get; set; }
        public Axon axon { get; set; }
        public double neuronLength { get; set; }
        public double denDiam { get; set; }
        public double dendriteLength { get; set; }
        private double somaDiam;
        public double axDiam { get; set; }
        public double axonLength { get; set; }
        public double outFlowVolume { get; set; }
        public double totalOutFlowVolume { get; set; }
        public double volumeToPush { get; set; }
        public bool isFlow { get; set; }
        public string model { get; set;  }
        public bool isFull { get; set; }
        public double minVolumeToOutflow { get; set; }
        public double volumeToOutFlowWhenNeuronFull { get; set; }

        public Neuron(int dendrideNum)
        {
            InitializeComponent();
            this.isFull = false;
            this.denDiam = 0.4;
            this.dendriteLength = 1.5;
            this.axDiam = 0.4;
            this.axonLength = 25;
            this.somaDiam = 13.5;
            
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
                            dendrite.Margin = new System.Windows.Thickness(0, 7, 0, 0);
                        }
                        else
                        {
                            dendrite.VerticalAlignment = VerticalAlignment.Bottom;
                            dendrite.Margin = new System.Windows.Thickness(0, 0, 0, 7);
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
                this.outFlowVolume = 0;
                this.totalOutFlowVolume = 0;
                this.volumeToOutFlowWhenNeuronFull = 0;
                this.isFlow = false;
                this.calculateVolumeToOutFlow();
            }
            else
            {
                Console.WriteLine("You must specify number of dendrites - 0 i sminimum");
            }

        }

        private void calculateVolumeToOutFlow()
        {
            this.minVolumeToOutflow = 0;
            this.volumeToOutFlowWhenNeuronFull = 0;
            this.minVolumeToOutflow += axon.volume;
            this.volumeToOutFlowWhenNeuronFull += axon.volume;
            if (this.soma != null)
            {
                this.minVolumeToOutflow += this.soma.threshold;
            }
            if (this.dendrites_list.Count() > 0)
            {
                this.minVolumeToOutflow += this.dendrites_list[0].volume;
                this.volumeToOutFlowWhenNeuronFull += this.dendrites_list[0].volume;
            }
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%Total volume " + this.minVolumeToOutflow);
        }


        public void unload(bool reminder)
        {
            this.axon.unloadFunc();
            if (this.soma != null)
            {
                Console.WriteLine("Soma unload");
                this.soma.unloadFunc(true);

            }
            foreach (Dendrite den in this.dendrites_list)
            {
                den.unloadFunc();
            }
            Console.WriteLine("In neuron onload");
            Console.WriteLine(this.outFlowVolume);
            Console.WriteLine(this.totalOutFlowVolume);
            Thread.Sleep(100);
            if (!reminder)
            {
                double value = this.totalOutFlowVolume + this.outFlowVolume;
                this.totalOutFlowVolume = value;
            }
            Console.WriteLine("After" + this.totalOutFlowVolume);
        }

        public void SetParameters(List<Tuple<double, double>> dendriteLenAndDiam_List, double somaDiam, double axonDiam, double axonLen, bool blockAxon)
        {
            if (dendriteLenAndDiam_List != null && dendriteLenAndDiam_List.Count() == this.dendrites_list.Count())
            {
                Console.WriteLine("Change den params");
                for (int i = 0; i < this.dendrites_list.Count(); i++)
                {
                    Dendrite den = this.dendrites_list[i];
                    den.diameter = dendriteLenAndDiam_List[i].Item1 == 0 ? this.denDiam : dendriteLenAndDiam_List[i].Item1;
                    den.length = dendriteLenAndDiam_List[i].Item2 == 0 ? this.dendriteLength : dendriteLenAndDiam_List[i].Item2;
                    den.calculateParameters();

                }
            }
            else if (dendriteLenAndDiam_List != null && dendriteLenAndDiam_List.Count() == 1)
            {
                Console.WriteLine("Change den params");
                foreach (Dendrite den in this.dendrites_list)
                {
                    den.diameter = dendriteLenAndDiam_List[0].Item1 == 0 ? this.denDiam : dendriteLenAndDiam_List[0].Item1;
                    den.length = dendriteLenAndDiam_List[0].Item2 == 0 ? this.dendriteLength : dendriteLenAndDiam_List[0].Item2;
                    den.calculateParameters();
                }
            }
            else
            {
                Console.WriteLine("Change den params");
                foreach (Dendrite den in this.dendrites_list)
                {
                    den.diameter = this.denDiam;
                    den.length = this.dendriteLength;
                    den.calculateParameters();
                }
            }

            if (soma != null)
            {
                Console.WriteLine("change som");
                this.soma.diameter = somaDiam == 0 ? this.somaDiam : somaDiam;
                this.soma.axonDiameter = axonDiam == 0 ? this.axDiam : axonDiam;
                this.soma.calculateParameters();
            }
            Console.WriteLine("Change axon");
            this.axon.length = axonLen == 0 ? this.axonLength : axonLen;
            this.axon.diameter = axonDiam == 0 ? this.axDiam : axDiam;
            this.axon.blockTheEnd = blockAxon;
            axon.calculateParameters();

            this.calculateVolumeToOutFlow();
            
        }

        public void reset()
        {
            this.axon.reset();
            this.isFull = false;
            this.volumeToPush = 0;
            if (this.soma != null)
            {
                Console.WriteLine("Reset soma");
                this.soma.reset();
            }
            foreach (Dendrite den in this.dendrites_list)
                den.reset();
        }

        public bool draining(double volumeToEmpty)
        {
            Console.WriteLine("In Neron draining " + volumeToEmpty);
            bool empty = true;
            if (this.soma != null)
            {
                empty = this.soma.partialEmpty(volumeToEmpty);
            }
            Console.WriteLine("Neron draining return: " + empty);
            return empty;
        }

    }

}
