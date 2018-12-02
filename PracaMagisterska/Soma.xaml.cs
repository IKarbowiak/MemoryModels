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
        private double collect_rows = 0;
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
            void fillRowOrNot(double rowNum)
            {
                Console.WriteLine("Row num 1 !!!" + rowNum);
                Console.WriteLine(collect_rows);
                if (rowNum < 1 && this.collect_rows < 1)
                {
                    collect_rows += rowNum;
                    rowNum = 0;
                }
                else if (rowNum >= 1 || this.collect_rows >= 1)
                {
                    collect_rows += rowNum;
                    Console.WriteLine(collect_rows);
                    rowNum = (int)collect_rows;
                    collect_rows = (double)collect_rows - (double)rowNum;
                }
                Console.WriteLine("Row num!!!" + rowNum);
                if (rowNum > 0) this.fillRect((int)rowNum);
            }

            bool push = false;
            Console.WriteLine("In new Soma " + volumeIncrease);
            double increase = this.liquidVolume + volumeIncrease;
            double rowToFill;
            double volumeToPush = 0;
            Console.WriteLine("volume" + volume + "    threshold: " + this.threshold + "       increase: " + increase);
            if (this.threshold < (increase) && !axonIsFull)
            {
                Console.WriteLine("Reach treshold");
                if (this.liquidVolume < this.volume && this.volume >= (increase))
                {
                    volumeToPush = increase - this.threshold;
                    this.liquidVolume += (volumeIncrease - volumeToPush);
                    //rowToFill = ((double)somaRec.Height * (volumeIncrease - volumeToPush) / this.volume);
                    rowToFill = (double)somaRec.Height * 0.5 - this.rowCounter;
                    Console.WriteLine("Row to fills" + rowToFill);
                    fillRowOrNot(rowToFill);
                    this.isFull = false;
                    push = true;

                }
                else
                {
                    volumeToPush = increase - this.volume;
                    this.liquidVolume = this.volume;
                    rowToFill = (double)somaRec.Height - this.rowCounter;
                    fillRowOrNot(rowToFill);
                    this.isFull = true;
                    push = true;
                }
                
                //axon.newFlow(volumeToPush);
            }
            else if (axonIsFull)
            {
                Console.WriteLine("In full soma axon");
                if (increase > this.volume)
                {
                    volumeToPush = increase - this.volume;
                    this.liquidVolume = this.volume;
                    rowToFill = (double)somaRec.Height - this.rowCounter;
                    fillRowOrNot(rowToFill);
                    this.isFull = true;
                    push = true;
                }
                else
                {
                    this.liquidVolume += volumeIncrease;
                    rowToFill = ((double)somaRec.Height * volumeIncrease / this.volume);
                    fillRowOrNot(rowToFill);
                    this.isFull = false;
                }
            }
            else if (this.volume >= (this.liquidVolume + volumeIncrease))
            {
                this.liquidVolume += volumeIncrease;
                rowToFill = ((double)somaRec.Height * volumeIncrease / this.volume);
                fillRowOrNot(rowToFill);
                this.isFull = false;
            }
            Tuple<bool, double> result = new Tuple<bool, double>(push, volumeToPush);
            return result;
        }

        private void fillRect(int rowlToFill)
        {
            Console.WriteLine("In soma fill REC !!! Row to fill: " + rowlToFill);
            int rowToFill = rowCounter - rowlToFill;

            if (rowToFill < 0)
            {
                rowToFill = 0;
            }
            for (int j = rowCounter; j > (rowToFill); j--)
            {
                for (int i = 0; i < recSomaArray[0].Length; i++)
                {
                    recSomaArray[j][i].Fill = System.Windows.Media.Brushes.DodgerBlue;
                    recSomaArray[j][i].Refresh();

                }
            }

            if (rowToFill > 0)
            {
                rowCounter -= rowlToFill;
            }
            else
            {
                rowCounter =  0;
            }
        }

        public void unloadFunc()
        {
            this.liquidVolume = this.threshold;

            //for (int j = this.rowToReachTeshold; j < this.rowCounter; j++)
            for (int j = this.rowCounter; j < this.rowToReachTeshold + 2; j++)
            {
                for (int i = 0; i < recSomaArray[0].Length; i++)
                {
                    recSomaArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;

                }
            }


        }

        public void reset()
        {
            this.liquidVolume = 0;
            this.rowCounter = recSomaArray.Length - 1;
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
