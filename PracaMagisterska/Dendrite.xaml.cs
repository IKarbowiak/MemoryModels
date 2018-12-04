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
    /// Interaction logic for Dendrite.xaml
    /// </summary>
    public partial class Dendrite : UserControl
    {
        public double length { get; set; }
        public double diameter { get; set; }
        private double surface;
        private bool dimension3D;
        private double volume;
        private double liquidVolume = 0;
        private Rectangle[][] recDenArray;
        private int columnsCounter = 0;
        private bool isFull = false;
        public bool isBlocked {get; set;}


        public Dendrite(bool dim3d)
        {
            InitializeComponent();
            this.isBlocked = false;
            this.length = 1.5;
            this.diameter = 0.4;
            this.dimension3D = dim3d;
            this.calculateParameters();
            recDenArray = this.splitRecModel(denrdriteRec, dendriteGrid);
        }

        public void calculateParameters()
        {
            if (dimension3D)
            {
                this.surface = Math.Pow((this.diameter / 2), 2) * Math.PI;
                this.volume = this.length * this.surface;
            }
            else
            {
                this.volume = this.length * this.diameter;
            }

        }


        public Tuple<bool, double> newFlow(object sender, EventArgs e, double volumeIncrease)
        {
            double volumeToPush = 0;
            if (this.liquidVolume < this.volume && this.volume >= (this.liquidVolume + volumeIncrease))
            {
                this.liquidVolume += volumeIncrease;
                int colLevelToFill = (int)((double)denrdriteRec.Width * volumeIncrease / this.volume);
                Console.WriteLine("Col counter: " + columnsCounter + ", recDenArray " + recDenArray[0].Length);
                if (colLevelToFill > 0 && this.columnsCounter < this.recDenArray[0].Length - 1)
                {
                    this.fillRect(colLevelToFill);
                }
                this.isFull = false;
            }
            else
            {
                volumeToPush = this.liquidVolume + volumeIncrease - this.volume;
                this.liquidVolume = this.volume;
                int colToFillin1s = (int)denrdriteRec.Width;
                Console.WriteLine("Col counter: " + columnsCounter + ", recDenArray " + recDenArray[0].Length);
                if (this.columnsCounter < this.recDenArray[0].Length - 1)
                {
                    this.fillRect(colToFillin1s);
                }

                this.isFull = true;
            }

            Console.WriteLine("In dendrite " + volumeToPush);
            Tuple<bool, double> result = new Tuple<bool, double>(this.isFull, volumeToPush);
            return result;
        }

        private void fillRect(int numberOfCol)
        {
            int colLevel = this.columnsCounter + numberOfCol;

            if (colLevel > recDenArray[0].Length)
            {
                colLevel = recDenArray[0].Length;
            }

            for (int i = this.columnsCounter; i < (colLevel); i++)
            {
                for (int j = 0; j < recDenArray.Length; j++)
                {
                    recDenArray[j][i].Fill = System.Windows.Media.Brushes.DodgerBlue;

                }
            }

            this.columnsCounter = colLevel;

            if (this.columnsCounter >= recDenArray[0].Length)
            {
                Console.WriteLine("In stop dend");
                isFull = true;
                this.columnsCounter = recDenArray[0].Length - 1;

            }
        }

        public void unloadFunc()
        {
            Console.WriteLine("Dendirte unload");
            Console.WriteLine(this.columnsCounter);

            for (int i = this.recDenArray[0].Length - 1; i >= 0; i--)
            {
                for (int j = 0; j < recDenArray.Length; j++)
                {
                    recDenArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;
                    recDenArray[j][i].Refresh();

                }
            }

            this.columnsCounter = 0;

        }

        public void reset()
        {
            this.columnsCounter = 0;
            this.isFull = false;
            this.liquidVolume = 0;
            for (int i = 0; i < this.recDenArray[0].Length; i++)
            {
                for (int j = 0; j < recDenArray.Length; j++)
                {
                    recDenArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;
                }
            }

        }


        public Rectangle[][] splitRecModel(Rectangle modelElement, Grid modelGrid)
        {
            int rowNumber = Convert.ToInt32(modelElement.Height);
            int columnNumber = Convert.ToInt32(modelElement.Width);

            Rectangle rec;


            Rectangle[][] recArray = new Rectangle[rowNumber][];


            for (int i = 0; i < rowNumber; i++)
            {
                recArray[i] = new Rectangle[columnNumber];
                modelGrid.RowDefinitions.Add(new RowDefinition());

                for (int j = 0; j < columnNumber; j++)
                {
                    ColumnDefinition c1 = new ColumnDefinition();
                    c1.Width = new GridLength(1);
                    modelGrid.ColumnDefinitions.Add(c1);

                    rec = new Rectangle()
                    {
                        Width = 1,
                        Height = 1,
                        //Stroke = Brushes.Red,
                        StrokeThickness = 1,

                        Name = "rec_" + i + "_" + j
                    };

                    recArray[i][j] = rec;

                    modelGrid.Children.Add(rec);
                    Grid.SetColumn(rec, j);
                    Grid.SetRow(rec, i);

                }
            }
            return recArray;
        }
    }


}
