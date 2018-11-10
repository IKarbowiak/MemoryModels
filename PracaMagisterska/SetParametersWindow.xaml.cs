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
using System.Windows.Shapes;

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for SetParametersWindow.xaml
    /// </summary>
    public partial class SetParametersWindow : Window
    {
        private TextBlock info;
        private TextBox parBox;

        public SetParametersWindow()
        {
            InitializeComponent();

            //string[] parameters = { "Neuron Length", "Denrite Diameter", "Axon Diameter", "Flow ", "Flow time", "Max speed" };
            //string[] parametersM1BoxNames = {"neuronLenBoxM1", "denDiamBoxM1", "axonDiamBox1", "flowBoxM1", "timeBoxM1", "maxSpeedM1" };
            //for (int i = 0; i < parametersM1BoxNames.Length; i++)
            //{
            //    info = new TextBlock() { Height=16};
            //}

        }
        
    }
}
