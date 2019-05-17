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


namespace PracaMagisterska.PersonalSolution
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
        public double threshold { get; set; }
        private int rowToReachTeshold;
        private double liquidVolume = 0;
        private Rectangle[][] recSomaArray;
        private int rowCounter;
        private bool isFull = false;

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

        // calculate basic Soma parameters as volume and trashold for leaking fluid to axon
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
            this.rowToReachTeshold = (int)(somaRec.Height / 2 - 1);
        }

        // fill soma or push fluid to axon
        public Tuple<bool, double> newFlow(object sender, EventArgs e, double volumeIncrease, bool axonIsFull, System.Windows.Media.SolidColorBrush color)
        {
            bool push = false;
            double increase = this.liquidVolume + volumeIncrease;
            double rowToFill;
            double volumeToPush = 0;

            if (this.isFull)
            {
                push = true;
                Tuple<bool, double> res = new Tuple<bool, double>(push, volumeIncrease);
                return res;
            }

            if (increase > this.threshold && !axonIsFull)
            {
                volumeToPush = increase - this.threshold;
                this.liquidVolume += (volumeIncrease - volumeToPush);
                rowToFill = this.rowToReachTeshold;
                this.fillRect(rowToFill, color);
                this.isFull = false;
                push = true;
                
            }

            else if (increase > this.volume)
            {
                volumeToPush = increase - this.volume;
                this.liquidVolume = this.volume;
                rowToFill = (double)somaRec.Height;
                this.fillRect(rowToFill, color);
                this.isFull = true;
                push = true;
            }

            else if (this.volume >= increase)
            {
                this.liquidVolume += volumeIncrease;
                rowToFill = ((double)somaRec.Height * this.liquidVolume / this.volume);
                this.fillRect(rowToFill, color);
                this.isFull = false;
                push = false;
            }
            Tuple<bool, double> result = new Tuple<bool, double>(push, volumeToPush);
            return result;
        }

        // fill rectangle to specific level
        private void fillRect(double rowLevel, System.Windows.Media.SolidColorBrush color)
        {
            int rowToFill = (int)(somaRec.Height - rowLevel);

            if (rowToFill < 0)
                rowToFill = 0;

            for (int j = rowCounter; j > (rowToFill); j--)
            {
                for (int i = 0; i < recSomaArray[0].Length; i++)
                {
                    recSomaArray[j][i].Fill = color;
                }
            }
            rowCounter = rowToFill > 0 ? rowToFill : 0; 
        }

        // reset parameters and empty soma to the level below treshold
        public bool partialEmpty(double remainingMemory)
        {
            bool empty = false;
            double restVolume = this.threshold * remainingMemory;
            if (restVolume > this.threshold)
                restVolume = this.threshold;
            this.liquidVolume = restVolume;
            double rowToEmpty = ((double)somaRec.Height * this.liquidVolume / this.volume);
            unloadFunc(false, rowToEmpty);
            if (this.liquidVolume <= 0)
            {
                this.resetParams();
                empty = true;
            }
            return empty;

        }

        // empty soma to specific level
        public void unloadFunc(bool unload, double toEmpty = 0)
        {
            isFull = false;
            toEmpty = toEmpty < 0 ? 0 : toEmpty;
            int toReach = unload == true ? this.rowToReachTeshold : (int)(somaRec.Height - toEmpty);

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

        // reset soma parameters
        private void resetParams()
        {
            this.liquidVolume = 0;
            this.isFull = false;
            this.rowCounter = recSomaArray.Length - 1;
        }

        // empty soma
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

        // create rectangles for soma which can be fill 
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
