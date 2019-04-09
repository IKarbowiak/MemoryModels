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
using PracaMagisterska.HTM;
using NumSharp.Core;

namespace PracaMagisterska
{
    /// <summary>
    /// Interaction logic for HTM_window.xaml
    /// </summary>
    public partial class HTM_window : Window
    {
        private PracaMagisterska.HTM.HTM htm;

        public HTM_window()
        {
            InitializeComponent();
            htm = new PracaMagisterska.HTM.HTM();
            initialize_model();
        }

        public void initialize_model()
        {
            NDArray data = new int[,] { {1, 0, 1, 0, 1, 0, 1, 0, 1, 0 },
                                        {0, 0, 0, 0, 1, 0, 1, 0, 1, 1 },
                                        {0, 0, 1, 0, 1, 0, 0, 0, 0, 1 },
                                        {0, 1, 0, 0, 1, 1, 0, 1, 0, 1 },
                                        {1, 0, 1, 0, 1, 0, 1, 0, 0, 1 },
                                        {0, 0, 0, 1, 1, 0, 0, 0, 1, 1 } };

            htm.initialize_input(data);
        }

    }
}
