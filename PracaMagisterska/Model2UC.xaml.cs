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
    /// Interaction logic for Model2UC.xaml
    /// </summary>
    public partial class Model2UC : UserControl
    {
        public double length { get; set; }
        public double dendrite_diameter { get; set; }
        public double speed { get; set; }
        public double dendrite_length { get; set; }
        public double axon_length { get; set; }
        public double soma_diameter { get; set; }
        public double axon_diameter { get; set; }
        public double dendrite_surface;
        public double axon_surface;
        public double dendrite_volume;
        public double soma_volume;
        public double axon_volume;
        public double soma_threshold;
        public Rectangle[][] recDiamArray;
        public Rectangle[][] recSomaArray;
        public Rectangle[][] recAxonArray;
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        private int colToFillInDendriteAfter1s;
        private int rowToFillInSomaAfter1s;
        private int colToFillInAxonAfter1s;
        private bool dendriteIsFull = false;
        private bool somaIsFull = false;
        private bool axonIsFull = false;
        private int diamCountColumns = 0;
        private int somaCountRows = 0;
        private int axonCountColumns = 0;
        private bool isSomaTreshold = false;
        private int rowToReachTeshold;

        public Model2UC()
        {
            InitializeComponent();
            this.length = 40;
            this.speed = 200;
            this.dendrite_diameter = 0.4;

            calculate_parameters(dendrite_diameter);


            recDiamArray = splitRecModel(recInModel2UC, gridInModel2UC);
            recSomaArray = splitRecModel(recMiddleModel2UC, gridMiddleModel2UC);
            recAxonArray = splitRecModel(recOutModel2UC, gridOutputModel2UC);
        }

        public void calculate_parameters(double diameter)
        {
            this.dendrite_length = this.length / 26;
            this.soma_diameter = this.length * 10 / 26;
            this.axon_length = this.length * 20 / 26;
            this.dendrite_surface = Math.Pow((dendrite_diameter / 2), 2) * Math.PI;
            this.axon_surface = Math.Pow((axon_diameter / 2), 2) * Math.PI;
            this.dendrite_volume = this.dendrite_length * this.dendrite_surface;
            this.axon_volume = this.axon_length * this.axon_surface;
            this.soma_volume = Math.Pow(this.soma_diameter / 2, 2) * Math.PI * this.soma_diameter;
            this.soma_threshold = (this.soma_diameter / 2 - diameter / 2) * this.soma_volume / this.soma_diameter;
        }

        public Rectangle[][] splitRecModel(Rectangle modelElement, Grid modelGrid)
        {
            //object MyAttribute =  
            //(object)Attribute.GetCustomAttribute(mode, typeof(DeveloperAttribute));
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

        public double[] Flow(double time)
        {
            Console.WriteLine("**************");
            double timeToReachSoma = this.dendrite_length / this.speed;
            //double time_to_reach_soma2 = this.dendrite_volume / (this.input_surface * this.speed); ta sama wartość co wyżej wychodzi więc można zostawić ławtiejszą wersję
            double timeToFillSoma = this.soma_threshold / (this.dendrite_surface * this.speed);
            Console.WriteLine(timeToReachSoma);
            Console.WriteLine("Time For Fill some");
            Console.WriteLine(timeToFillSoma);

            // Time after which liquid will begin to flow out
            double out_flow_time = this.length / this.speed + timeToFillSoma; // duże zaokrąglenie

            if (out_flow_time > time)
            {
                Console.WriteLine("Out flow bigger than time");
                double[] res0 = { 0, 0 };
                return res0;
            }

            
            colToFillInDendriteAfter1s = (int)((double)recInModel2UC.Width / timeToReachSoma);
            rowToFillInSomaAfter1s = (int)((double)recMiddleModel2UC.Height / timeToFillSoma);
            colToFillInAxonAfter1s = (int)((double)recOutModel2UC.Width / (this.dendrite_length/this.speed));
            rowToReachTeshold = (int)(soma_threshold * recMiddleModel2UC.Height / soma_volume);

            if (colToFillInAxonAfter1s == 0)
            {
                colToFillInAxonAfter1s = 1;
            }

            if (colToFillInDendriteAfter1s == 0)
            {
                colToFillInDendriteAfter1s = 1;
            }

            if (rowToFillInSomaAfter1s == 0)
            {
                rowToFillInSomaAfter1s = 1;
            } 

            //ref int[] colAndRowToFill = { colToFillInDendriteAfter1s, rowToFillInSomaAfter1s, colToFillInAxonAfter1s };

            //for (int i = 0; i < colAndRowToFill.Length; i++)
            //{
            //    if (colAndRowToFill[i] == 0)
            //    {
            //        colAndRowToFill[i] = 1;
            //    }
            //}

            Console.WriteLine(rowToFillInSomaAfter1s);

            dendriteIsFull = false;
            somaIsFull = false;
            axonIsFull = false;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => { fillfunc(sender, e, time); };
            timer.Start();

            // The volume of liquid which flowed up
            double flowedVolume = this.dendrite_surface * this.speed * (time - out_flow_time);
            double[] res = { out_flow_time, flowedVolume };
            return res;

        }

        public void fillfunc(object sender, EventArgs e, double time)
        {
            Console.WriteLine("In fill function");
            Console.WriteLine(dendriteIsFull);
            Console.WriteLine(somaIsFull);
            Console.WriteLine(axonIsFull);
            if (!dendriteIsFull)
            {
                fillDendriteOrAxon(ref diamCountColumns, ref colToFillInDendriteAfter1s, recDiamArray, ref dendriteIsFull);
                
            }
            else if (!somaIsFull && dendriteIsFull)
            {
                Console.WriteLine(rowToFillInSomaAfter1s);
                Console.WriteLine("In soma");
                int rowToFill = somaCountRows + rowToFillInSomaAfter1s;

                if (rowToFill > recSomaArray.Length)
                {
                    rowToFill = recSomaArray.Length;
                }
                for (int j = somaCountRows; j < (rowToFill); j++)
                {
                    for (int i = 0; i < recSomaArray[0].Length; i++)
                    {
                        recSomaArray[j][i].Fill = System.Windows.Media.Brushes.Blue;
                        recSomaArray[j][i].Refresh();

                    }
                }

                somaCountRows += rowToFillInSomaAfter1s;

                if (somaCountRows >= rowToReachTeshold)
                {
                    somaCountRows = recSomaArray.Length - 1;
                    somaIsFull = true;
                }

                //if (somaCountRows >= recSomaArray.Length)
                //{
                //    somaCountRows = recSomaArray.Length - 1;
                //    somaIsFull = true;

                //}
            }

            else if (somaIsFull && !axonIsFull)
            {
                fillDendriteOrAxon(ref axonCountColumns, ref colToFillInAxonAfter1s, recAxonArray, ref axonIsFull);

            }
            else
            {
                timer.Stop();
                Console.WriteLine("Stop timer 1");
            }
        }

        private void fillDendriteOrAxon(ref int countColumns, ref int colToFillIn1s, Rectangle[][] recArray, ref bool isFull)
        {
            Console.WriteLine("In fill den or axon");
            int colToFill = countColumns + colToFillIn1s;

            if (colToFill > recArray[0].Length)
            {
                colToFill = recArray[0].Length;
            }

            for (int i = countColumns; i < (colToFill); i++)
            {
                for (int j = 0; j < recArray.Length; j++)
                {
                    recArray[j][i].Fill = System.Windows.Media.Brushes.Blue;
                    recArray[j][i].Refresh();

                }
            }

            countColumns += colToFillIn1s;


            if (countColumns >= recArray[0].Length)
            {
                countColumns = recArray[0].Length - 1;
                isFull = true;
            }
        }


        //public void unloadFunc(object sender, EventArgs e)
        //{
        //    Console.WriteLine("I am here");
        //    Console.WriteLine(countColumns);
        //    bool stop = false;

        //    int colToUnfill = countColumns - colToFilAfter1s;

        //    if (colToUnfill <= 0)
        //    {
        //        colToUnfill = 0;
        //        stop = true;
        //    }

        //    for (int i = countColumns; i >= (colToUnfill); i--)
        //    {
        //        for (int j = 0; j < recArray.Length; j++)
        //        {
        //            recArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;
        //            recArray[j][i].Refresh();

        //        }
        //    }

        //    countColumns -= colToFilAfter1s;

        //    if (stop)
        //    {
        //        Console.WriteLine("Stop timer2");
        //        timer2.Stop();
        //    }

        //}

    }
}
