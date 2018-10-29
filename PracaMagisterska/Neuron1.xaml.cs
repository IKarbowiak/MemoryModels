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
    /// Interaction logic for Neuron1.xaml
    /// </summary>
    public partial class Neuron1 : UserControl
    {

        private Dendrite dendrite;
        private Soma soma;
        private Axon axon;

        public Neuron1()
        {
            InitializeComponent();
            dendrite = new Dendrite();
            dendrite.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetColumn(dendrite, 1);
            Grid.SetRow(dendrite, 1);
            neuronGrid.Children.Add(dendrite);

            axon = new Axon();
            axon.HorizontalAlignment = HorizontalAlignment.Right;
            //soma.Margin = new System.Windows.Thickness(0, 0, 84, 0);
            Grid.SetColumn(axon, 1);
            Grid.SetRow(axon, 1);
            neuronGrid.Children.Add(axon);

            soma = new Soma();
            soma.HorizontalAlignment = HorizontalAlignment.Center;
            soma.Margin = new System.Windows.Thickness(0, 0, 84, 0);
            Grid.SetColumn(soma, 1);
            Grid.SetRow(soma, 1);
            neuronGrid.Children.Add(soma);
        }
    }
}
