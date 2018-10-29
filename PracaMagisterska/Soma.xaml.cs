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
        public double dendriteDiameter;
        public double dendriteSurface;
        private double surface;
        private double volume;
        private double threshold;
        private Rectangle[][] recSomaArray;
        private System.Windows.Threading.DispatcherTimer timer;
        private int rowCounter = 0;
        private int seconds = 0;
        private bool isFull = false;

        public Soma()
        {
            InitializeComponent();
            this.diameter = 15;
            this.dendriteDiameter = 0.4;
            this.calculateParameters();
            recSomaArray = this.splitRecModel(somaRec, somaGrid);
        }

        public void calculateParameters()
        {
            Console.WriteLine("I am in calculate soma");
            this.surface = Math.Pow((this.diameter / 2), 2) * Math.PI;
            this.volume = this.diameter * this.surface;
            this.dendriteSurface = Math.Pow((this.dendriteDiameter / 2), 2) * Math.PI;
            Console.WriteLine("Dendite surface " + this.dendriteSurface);
            this.threshold = (this.diameter / 2 - this.axonDiameter / 2) * this.volume /  (this.diameter);
        }

        public double[] flow(double time, double speed)
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
            timer.Tick += (sender, e) => { fillFunc(sender, e, time, rowsToFillIn1s, rowToReachTeshold); };
            timer.Start();

            double[] res = { timeToFillSoma };
            return res;
        }

        public void fillFunc(object sender, EventArgs e, double time, int rowToFill1s, int treshold)
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
                rowCounter = recSomaArray.Length - 1;
                this.isFull = true;
                this.timer.Stop();
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
