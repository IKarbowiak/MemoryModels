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
    /// Interaction logic for Axon.xaml
    /// </summary>
    public partial class Axon : UserControl
    {
        public double length { get; set; }
        public double diameter { get; set; }
        public bool dimension3D;
        public double surface;
        public double flowedOutVolume { get; set; }
        public bool blockTheEnd { get; set; }
        private double volume;
        private double liquidVolume;
        private Rectangle[][] recAxonArray;
        private int columnsCounter = 0;
        public bool isFull { get; set; }

        public Axon(bool dim3d = false, int recWidth = 260, int recHeight = 11)
        {
            InitializeComponent();
            this.length = 31;
            this.diameter = 0.4;
            this.liquidVolume = 0;
            this.dimension3D = dim3d;
            this.flowedOutVolume = 0;
            this.calculateParameters();
            this.blockTheEnd = false;
            this.isFull = false;
            if (recWidth != 260 || recHeight != 11)
            {
                changeRecSize(recWidth, recHeight);
            }
            recAxonArray = this.splitRecModel(axonRec, axonGrid);
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

        public void changeRecSize(int width, int height)
        {

            mainGrid.Width = width;
            mainGrid.Height = height;

            axonRec.Height = height;
            axonRec.Width = width;
            axonGrid.Height = height;
            axonGrid.Width = width;
            //this.recAxonArray = this.splitRecModel(axonRec, axonGrid);
        }

        public Tuple<bool, double> newFlow(object sender, EventArgs e, double volumeIncrease)
        {
            double volumeToPush = 0;
            if (this.liquidVolume < this.volume && this.volume >= (this.liquidVolume + volumeIncrease))
            {
                this.liquidVolume += volumeIncrease;
                int colLevelToFill = (int)((double)axonRec.Width * volumeIncrease / this.volume);
                Console.WriteLine("Poniżej max");
                if (colLevelToFill == 0)
                {
                    colLevelToFill = 1;
                }
                if (this.columnsCounter < this.recAxonArray[0].Length - 1)
                {
                    this.fillRect(colLevelToFill);
                }
                //this.fillRect(sender, e, colToFillin1s);
            }
            else
            {
                Console.WriteLine("Powyżej!");
                volumeToPush = this.liquidVolume + volumeIncrease - this.volume;
                this.liquidVolume = this.volume;
                int colLevelToFill = (int)axonRec.Width;
                if (colLevelToFill > 0)
                {
                    if (this.columnsCounter < this.recAxonArray[0].Length - 1)
                    {
                        this.fillRect(colLevelToFill);
                    }
                    //this.fillRect(sender, e, colToFillin1s);
                }
                this.isFull = true;
                Console.WriteLine("Is full exon in powyzej: " + this.isFull);
                Console.WriteLine("Axon is block ath the ende: " + this.blockTheEnd);
                this.flowedOutVolume += volumeToPush;

            }
            return new Tuple<bool, double>(this.isFull, volumeToPush);
        }

        private void fillRect(int numberOfCol)
        {
            Console.WriteLine("In fill ax " + numberOfCol);
            int colLevel = this.columnsCounter + numberOfCol;

            if (colLevel > recAxonArray[0].Length)
            {
                colLevel = recAxonArray[0].Length;
            }

            for (int i = this.columnsCounter; i < (colLevel); i++)
            {
                for (int j = 0; j < recAxonArray.Length; j++)
                {
                    recAxonArray[j][i].Fill = System.Windows.Media.Brushes.DodgerBlue;

                }
            }

            this.columnsCounter = colLevel;

            if (this.columnsCounter >= recAxonArray[0].Length)
            {
                Console.WriteLine("In stop AXON");
                Console.WriteLine("block the end" + blockTheEnd);
                isFull = this.blockTheEnd == true ? true : false;
                this.columnsCounter = recAxonArray[0].Length - 1;

            }
        }


        public void unloadFunc()
       {
            Console.WriteLine("Axon unload");
            Console.WriteLine(this.columnsCounter);


            for (int i = this.recAxonArray[0].Length - 1; i >= 0; i--)
            {
                for (int j = 0; j < recAxonArray.Length; j++)
                {
                    recAxonArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;
            
                 }
             }

            this.columnsCounter = 0;

        }


        public void reset()
        {
            this.columnsCounter = 0;
            this.flowedOutVolume = 0;
            this.isFull = false;
            this.liquidVolume = 0;
            for (int i = 0; i < this.recAxonArray[0].Length; i++)
            {
                for (int j = 0; j < recAxonArray.Length; j++)
                {
                    recAxonArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;
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
