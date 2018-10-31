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
        private double diameter { get; set; }
        private double surface { get; set; }
        private bool dimension3D { get; set; }
        private double volume;
        private double liquidVolume = 0;
        private Rectangle[][] recDenArray;
        private System.Windows.Threading.DispatcherTimer timer;
        private int columnsCounter = 0;
        private int seconds = 0;
        private bool isFull = false;


        public Dendrite(bool dim3d)
        {
            InitializeComponent();
            Console.WriteLine("Dendrite");
            this.length = 31;
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

        public void newFlow(object sender, EventArgs e, double volumeIncrease, Soma soma, Axon axon)
        {
            
            if (this.liquidVolume < this.volume && this.volume <= (this.liquidVolume + volumeIncrease))
            {
                this.liquidVolume += volumeIncrease;
                int colToFillin1s = (int)((double)denrdriteRec.Width * volumeIncrease / this.volume);
                if (colToFillin1s > 0)
                {
                    this.fillRect(colToFillin1s);
                }
            }
            else
            {
                double volumeToPush = this.liquidVolume + volumeIncrease - this.volume;
                this.liquidVolume = this.volume;
                int colToFillin1s = (int)denrdriteRec.Width - this.columnsCounter;
                this.fillRect(colToFillin1s);

                soma.newFlow(volumeIncrease, axon);
            }
        }

        private void fillRect(int collToFill)
        {
            Console.WriteLine("In fill den");
            int colToFill = this.columnsCounter + collToFill;

            if (colToFill > recDenArray[0].Length)
            {
                colToFill = recDenArray[0].Length;
            }

            for (int i = this.columnsCounter; i < (colToFill); i++)
            {
                for (int j = 0; j < recDenArray.Length; j++)
                {
                    recDenArray[j][i].Fill = System.Windows.Media.Brushes.Blue;
                    recDenArray[j][i].Refresh();

                }
            }

            this.columnsCounter += collToFill;

            if (this.columnsCounter >= recDenArray[0].Length)
            {
                Console.WriteLine("In stop dend");
                isFull = true;
                this.columnsCounter = recDenArray[0].Length - 1;

            }
        }

        public double[] flow(double time, double speed)
        {
            double outFlowTime = this.volume / (this.surface * speed);
            Console.WriteLine("Out flow time dendrite: " + outFlowTime);
            if (outFlowTime >= time)
            {
                Console.WriteLine("Out flow bigger than time");
                double[] res0 = { 0, 0 };
                return res0;
            }

            int colToFillIn1s = (int)((double)denrdriteRec.Width / outFlowTime);

            if (colToFillIn1s == 0)
            {
                colToFillIn1s = 1;
            }

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => {fillFunc(sender, e, time, colToFillIn1s); };
            timer.Start();

            double flowedVolume = this.surface * speed * (time - outFlowTime);

            double[] res = { outFlowTime, flowedVolume };
            return res;
        }

        private void fillFunc(object sender, EventArgs e, double time, int colToFillIn1s)
        {
            if (!isFull)
            {
                Console.WriteLine("In fill den");
                int colToFill = this.columnsCounter + colToFillIn1s;

                if (colToFill > recDenArray[0].Length)
                {
                    colToFill = recDenArray[0].Length;
                }

                for (int i = this.columnsCounter; i < (colToFill); i++)
                {
                    for (int j = 0; j < recDenArray.Length; j++)
                    {
                        recDenArray[j][i].Fill = System.Windows.Media.Brushes.Blue;
                        recDenArray[j][i].Refresh();

                    }
                }

                this.columnsCounter += colToFillIn1s;
                seconds++;

                if (this.columnsCounter >= recDenArray[0].Length || seconds >= (int)time)
                {
                    Console.WriteLine("In stop");
                    isFull = true;
                    this.columnsCounter = recDenArray[0].Length - 1;
                    timer.Stop();

                    
                    //Thread.Sleep((int)time*10 - seconds);
                    //Console.WriteLine( "Time "  + ((int)time*10 - seconds));
                    //seconds = 0;

                    //timer.Start();

                }
            }

            //else
            //{
            //    this.unloadFunc(colToFillIn1s);
            //}


        }

        public void unloadFunc(int colToFillIn1s)
        {
            Console.WriteLine("I am here");
            Console.WriteLine(this.columnsCounter);
            bool stop = false;

            int colToUnfill = this.columnsCounter - colToFillIn1s;

            if (colToUnfill <= 0)
            {
                colToUnfill = 0;
                stop = true;
            }

            for (int i = this.columnsCounter; i >= (colToUnfill); i--)
            {
                for (int j = 0; j < recDenArray.Length; j++)
                {
                    recDenArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;
                    recDenArray[j][i].Refresh();

                }
            }

            this.columnsCounter -= colToFillIn1s;

            if (stop)
            {
                timer.Stop();
                Console.WriteLine("Stop timer 1");
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
