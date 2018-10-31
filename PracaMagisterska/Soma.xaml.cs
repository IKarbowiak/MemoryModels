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
        private bool dimension3D { get; set; }
        public double dendriteDiameter;
        public double dendriteSurface;
        private double surface;
        private double volume;
        private double threshold;
        private double liquidVolume = 0;
        private Rectangle[][] recSomaArray;
        private System.Windows.Threading.DispatcherTimer timer;
        private int rowCounter = 0;
        private int seconds = 0;
        private bool isFull = false;
        //public bool IsEnabled { get; set; }

        public Soma(bool dim3d)
        {
            InitializeComponent();
            this.diameter = 15;
            this.dendriteDiameter = 0.4;
            this.dimension3D = dim3d;
            this.calculateParameters();
            recSomaArray = this.splitRecModel(somaRec, somaGrid);
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
            this.dendriteSurface = Math.Pow((this.dendriteDiameter / 2), 2) * Math.PI;
            Console.WriteLine("Dendite surface " + this.dendriteSurface);
            this.threshold = (this.diameter / 2 - this.axonDiameter / 2) * this.volume /  (this.diameter);
        }

        public void newFlow(double volumeIncrease, Axon axon)
        {
            Console.WriteLine("In new Soma");
            double increase = this.liquidVolume + volumeIncrease;
            int rowToFillin1s;
            double volumeToPush;
            Console.WriteLine("volume" + volume + "    threshold: " + this.threshold + "       increase: " + increase);
            if (this.threshold < (increase))
            {
                Console.WriteLine("Reach treshold");
                if (this.volume >= (increase))
                {
                    volumeToPush = increase - this.threshold;
                    this.liquidVolume += (volumeIncrease - volumeToPush);
                    rowToFillin1s = (int)((double)somaRec.Width * (volumeIncrease - volumeToPush) / this.volume);
                    Console.WriteLine(rowToFillin1s);
                    if (rowToFillin1s > 0)
                    {
                        this.fillRect(rowToFillin1s);
                    }

                }
                else
                {
                    volumeToPush = this.liquidVolume + volumeIncrease - this.volume;
                    this.liquidVolume = this.volume;
                    rowToFillin1s = (int)somaRec.Width - this.rowCounter;
                    if (rowToFillin1s > 0 )
                    {
                        this.fillRect(rowToFillin1s);
                    }
                }
                
                axon.newFlow(volumeToPush);
            }
            else if (this.volume >= (this.liquidVolume + volumeIncrease))
            {
                this.liquidVolume += volumeIncrease;
                rowToFillin1s = (int)((double)somaRec.Width * volumeIncrease / this.volume);
                if (rowToFillin1s == 0)
                {
                    rowToFillin1s = 1;
                }
                this.fillRect(rowToFillin1s);
            }
        }

        private void fillRect(int rowlToFill)
        {
            Console.WriteLine("In soma fill REC");
            int rowToFill = rowCounter + rowlToFill;

            if (rowToFill > recSomaArray.Length)
            {
                rowToFill = recSomaArray.Length;
            }
            for (int j = rowCounter; j < (rowToFill); j++)
            {
                for (int i = 0; i < recSomaArray[0].Length; i++)
                {
                    recSomaArray[recSomaArray.Length - 1 - j][i].Fill = System.Windows.Media.Brushes.Blue;
                    recSomaArray[recSomaArray.Length - 1 - j][i].Refresh();

                }
            }

            rowCounter += rowlToFill;

            //if (rowCounter >= treshold)
            //{
            //    rowCounter = recSomaArray.Length - 1;
            //    this.isFull = true;
            //}
        }

        public double[] flow(double time, double speed, Axon axon)
        {
            Console.WriteLine(this.threshold);
            Console.WriteLine(this.dendriteSurface);
            Console.WriteLine(speed);
            double timeToFillSoma = this.threshold / (this.dendriteSurface * speed);
            
            if (timeToFillSoma >= time)
            {
                Console.WriteLine("Out flow in soma bigger than time " + timeToFillSoma);
                double[] res0 = { 0, 0 };
                return res0;
            }

            int rowsToFillIn1s = (int)((double)somaRec.Width / timeToFillSoma);
            int rowToReachTeshold = (int)(this.threshold * somaRec.Height / this.volume);

            if (rowsToFillIn1s == 0)
            {
                rowsToFillIn1s = 1;
            }

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => { fillFunc(sender, e, time, rowsToFillIn1s, rowToReachTeshold, axon, speed); };
            timer.Start();

            double[] res = { timeToFillSoma };
            return res;
        }

        public void fillFunc(object sender, EventArgs e, double time, int rowToFill1s, int treshold, Axon axon, double speed)
        {
            Console.WriteLine("In soma");
            int rowToFill = rowCounter + rowToFill1s;

            if (rowToFill > recSomaArray.Length)
            {
                rowToFill = recSomaArray.Length;
            }
            for (int j = rowCounter; j < (rowToFill); j++)
            {
                for (int i = 0; i < recSomaArray[0].Length; i++)
                {
                    recSomaArray[recSomaArray.Length - 1 -j][i].Fill = System.Windows.Media.Brushes.Blue;
                    recSomaArray[recSomaArray.Length - 1 -j][i].Refresh();

                }
            }

            rowCounter += rowToFill1s;

            if (rowCounter >= treshold)
            {
                this.timer.Stop();
                rowCounter = recSomaArray.Length - 1;
                this.isFull = true;
                axon.flow(time, speed);
            }

        }

        public Rectangle[][] splitRecModel(Rectangle modelElement, Grid modelGrid)
        {
            int rowNumber = Convert.ToInt32(modelElement.Height);
            int columnNumber = Convert.ToInt32(modelElement.Width);

            Rectangle rec;
            Console.WriteLine(rowNumber);
            Console.WriteLine(columnNumber);

            Console.WriteLine(modelGrid.Width);
            Console.WriteLine(modelGrid.Height);


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

                    Console.WriteLine(rec.Name);
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
