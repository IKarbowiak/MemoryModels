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
    /// Interaction logic for Soma.xaml
    /// </summary>
    public partial class Soma : UserControl
    {
        public double diameter { get; set; }
        public double axonDiameter {get; set;}
        private bool dimension3D;
        private double surface;
        private double volume;
        private double threshold;
        private int rowToReachTeshold;
        private double liquidVolume = 0;
        private Rectangle[][] recSomaArray;
        private int rowCounter;
        private bool isFull = false;
        //public bool IsEnabled { get; set; }

        public Soma(bool dim3d)
        {
            InitializeComponent();
            this.diameter = 15;
            this.dimension3D = dim3d;
            this.axonDiameter = 0.4;
            this.calculateParameters();
            recSomaArray = this.splitRecModel(somaRec, somaGrid);
            this.rowCounter = recSomaArray.Length - 1;

        }

        public void calculateParameters()
        {
            if (dimension3D)
            {
                this.surface = Math.Pow((this.diameter / 2), 2) * Math.PI;
                this.volume = this.diameter * this.surface;

            }
            else
            {
                this.volume = this.diameter * this.diameter;
            }
            this.threshold = (this.diameter / 2 - this.axonDiameter / 2) * this.volume /  (this.diameter);
            Console.WriteLine("Calculate thrwshold " + this.threshold);
            this.rowToReachTeshold = (int)(this.threshold * somaRec.Height / this.volume);
        }

        public Tuple<bool, double> newFlow(object sender, EventArgs e, double volumeIncrease, bool axonIsFull)
        {
            bool push = false;
            Console.WriteLine("In new Soma " + volumeIncrease);
            double increase = this.liquidVolume + volumeIncrease;
            double rowToFill;
            double volumeToPush = 0;
            Console.WriteLine("volume" + volume + "    threshold: " + this.threshold + "       increase: " + increase);

            if (this.isFull)
            {
                push = true;
                Tuple<bool, double> res = new Tuple<bool, double>(push, volumeToPush);
                return res;
            }

            if (increase > this.threshold && this.volume >= increase && !axonIsFull)
            {
                Console.WriteLine("Reach treshold");

                volumeToPush = increase - this.threshold;
                this.liquidVolume += (volumeIncrease - volumeToPush);
                rowToFill = this.rowToReachTeshold;
                Console.WriteLine("Row to fills" + rowToFill);
                this.fillRect(rowToFill);
                this.isFull = false;
                push = true;
                
            }

            else if (increase > this.volume)
            {
                Console.WriteLine("In full soma axon");

                volumeToPush = increase - this.volume;
                this.liquidVolume = this.volume;
                rowToFill = (double)somaRec.Height;
                this.fillRect(rowToFill);
                this.isFull = true;
                push = true;
            }

            else if (this.volume >= increase)
            {
                this.liquidVolume += volumeIncrease;
                rowToFill = ((double)somaRec.Height * this.liquidVolume / this.volume);
                this.fillRect(rowToFill);
                this.isFull = false;
                push = false;
            }
            Tuple<bool, double> result = new Tuple<bool, double>(push, volumeToPush);
            return result;
        }

        private void fillRect(double rowLevel)
        {
            Console.WriteLine("In soma fill REC !!! Row to fill: " + rowLevel);
            int rowToFill = (int)(somaRec.Height - rowLevel);

            if (rowToFill < 0)
                rowToFill = 0;

            for (int j = rowCounter; j > (rowToFill); j--)
            {
                for (int i = 0; i < recSomaArray[0].Length; i++)
                {
                    recSomaArray[j][i].Fill = System.Windows.Media.Brushes.DodgerBlue;
                    recSomaArray[j][i].Refresh();
                }
            }

            rowCounter = rowToFill > 0 ? rowToFill : 0; 

        }

        public bool partialEmpty(double volumeToEmpty)
        {
            bool empty = false;
            this.liquidVolume -= volumeToEmpty;
            double rowToEmpty = ((double)somaRec.Height * this.liquidVolume / this.volume);
            new_unloadFunc(false, rowToEmpty);
            if (this.liquidVolume <= 0)
            {
                this.resetParams();
                empty = true;
            }
            return empty;

        }

        public void unloadFunc()
        {

            //for (int j = this.rowToReachTeshold; j < this.rowCounter; j++)
            for (int j = this.rowCounter; j < this.rowToReachTeshold; j++)
            {
                for (int i = 0; i < recSomaArray[0].Length; i++)
                {
                    recSomaArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;

                }
            }

            if (this.liquidVolume > this.threshold)
            {
                this.liquidVolume = this.threshold;
                this.rowCounter = this.rowToReachTeshold;
            }
        }

        public void new_unloadFunc(bool unload, double toEmpty = 0)
        {
            int toReach = unload == true ? this.rowToReachTeshold : (int)(somaRec.Height - toEmpty);

            //for (int j = this.rowToReachTeshold; j < this.rowCounter; j++)
            for (int j = this.rowCounter; j < toReach; j++)
            {
                for (int i = 0; i < recSomaArray[0].Length; i++)
                {
                    recSomaArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;

                }
            }

            if (unload)
            {
                if (this.liquidVolume > this.threshold)
                {
                    this.liquidVolume = this.threshold;
                    this.rowCounter = this.rowToReachTeshold;
                }
                return;
            }

            rowCounter = toReach;

        }

        private void resetParams()
        {
            this.liquidVolume = 0;
            this.isFull = false;
            this.rowCounter = recSomaArray.Length - 1;
        }

        public void reset()
        {
            this.resetParams();
            for (int i = 0; i < this.recSomaArray[0].Length; i++)
            {
                for (int j = 0; j < recSomaArray.Length; j++)
                {
                    recSomaArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;
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
