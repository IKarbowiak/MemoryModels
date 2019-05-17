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
using PracaMagisterska.PersonalSolution;

namespace PracaMagisterska.PersonalSolution
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
        public bool damage;

        // set main neuron paramaters, create dendrite, soma and axon objects
        public Neuron(int dendrideNum)
        {
            InitializeComponent();
            this.isFull = false;
            this.denDiam = 0.4;
            this.dendriteLength = 1.5;
            this.axDiam = 0.4;
            this.axonLength = 25;
            this.somaDiam = 13.5;
            this.damage = false;
            
            this.dendrites_list = new List<Dendrite>();
            if (dendrideNum == 0)
            {
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
                Console.WriteLine("You must specify number of dendrites - 0 is minimum");
            }

        }

        // claculate volume when flow will out flow
        private void calculateVolumeToOutFlow()
        {
            this.minVolumeToOutflow = axon.volume;
            this.volumeToOutFlowWhenNeuronFull = axon.volume;
            if (this.soma != null)
            {
                this.minVolumeToOutflow += this.soma.threshold;
            }
            if (this.dendrites_list.Count() > 0)
            {
                this.minVolumeToOutflow += this.dendrites_list[0].volume;
                this.volumeToOutFlowWhenNeuronFull += this.dendrites_list[0].volume;
            }
        }

        // unload neuron - empty axon, soma, dendrite
        public void unload(bool reminder)
        {
            this.axon.unloadFunc();
            if (this.soma != null)
            {
                this.soma.unloadFunc(true);
            }
            foreach (Dendrite den in this.dendrites_list)
            {
                den.unloadFunc();
            }
            Thread.Sleep(100);
            if (!reminder)
            {
                double value = this.totalOutFlowVolume + this.outFlowVolume;
                this.totalOutFlowVolume = value;
            }
        }

        // set neuron parameter - update axon, soma, dendrite parameters
        public void SetParameters(List<Tuple<double, double>> dendriteLenAndDiam_List, double somaDiam, double axonDiam, double axonLen, bool blockAxon, double axonMaxSpeed, bool neuron_damage = false)
        {
            this.damage = neuron_damage;

            if (dendriteLenAndDiam_List != null && dendriteLenAndDiam_List.Count() == this.dendrites_list.Count())
            {
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
                foreach (Dendrite den in this.dendrites_list)
                {
                    den.diameter = dendriteLenAndDiam_List[0].Item1 == 0 ? this.denDiam : dendriteLenAndDiam_List[0].Item1;
                    den.length = dendriteLenAndDiam_List[0].Item2 == 0 ? this.dendriteLength : dendriteLenAndDiam_List[0].Item2;
                    den.calculateParameters();
                }
            }
            else
            {
                foreach (Dendrite den in this.dendrites_list)
                {
                    den.diameter = this.denDiam;
                    den.length = this.dendriteLength;
                    den.calculateParameters();
                }
            }

            if (soma != null)
            {
                this.soma.diameter = somaDiam == 0 ? this.somaDiam : somaDiam;
                this.soma.axonDiameter = axonDiam == 0 ? this.axDiam : axonDiam;
                this.soma.calculateParameters();
            }
            this.axon.length = axonLen == 0 ? this.axonLength : axonLen * 1000;
            this.axon.diameter = axonDiam == 0 ? this.axDiam : axonDiam;
            this.axon.blockTheEnd = blockAxon;
            this.axon.maxSpeed = axonMaxSpeed;
            axon.calculateParameters();

            this.calculateVolumeToOutFlow();
            
        }

        // reset neuron parameters
        public void reset()
        {
            this.axon.reset();
            this.isFull = false;
            this.volumeToPush = 0;
            this.outFlowVolume = 0;
            if (this.soma != null)
            {
                this.soma.reset();
            }
            foreach (Dendrite den in this.dendrites_list)
                den.reset();
        }

        // partial empty the soma of neuron
        public bool draining(double remainingMemory)
        {
            bool empty = true;
            if (this.soma != null)
            {
                empty = this.soma.partialEmpty(remainingMemory);
            }
            return empty;
        }

    }

}
