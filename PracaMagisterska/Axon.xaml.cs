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
    /// Interaction logic for Axon.xaml
    /// </summary>
    public partial class Axon : UserControl
    {
        public double length { get; set; }
        public double diameter { get; set; }
        public bool dimension3D { get; set; }
        public double surface { get; set; }
        public double flowedOutVolume { get; set; }
        private double volume;
        private double liquidVolume;
        private Rectangle[][] recAxonArray;
        private System.Windows.Threading.DispatcherTimer timer;
        private int columnsCounter = 0;
        private int seconds = 0;
        private bool isFull = false;

        public Axon(bool dim3d)
        {
            InitializeComponent();
            this.length = 31;
            this.diameter = 0.4;
            this.liquidVolume = 0;
            this.dimension3D = dim3d;
            this.flowedOutVolume = 0;
            this.calculateParameters();
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

        public void newFlow(object sender, EventArgs e, double volumeIncrease)
        {
            Console.WriteLine("In aaxon new flow");
            if (this.liquidVolume < this.volume && this.volume <= (this.liquidVolume + volumeIncrease))
            {
                this.liquidVolume += volumeIncrease;
                int colToFillin1s = (int)((double)axonRec.Width * volumeIncrease / this.volume);
                if (colToFillin1s == 0)
                {
                    colToFillin1s = 1;
                }
                this.fillRect(colToFillin1s);
            }
            else
            {
                double volumeToPush = this.liquidVolume + volumeIncrease - this.volume;
                this.liquidVolume = this.volume;
                int colToFillin1s = (int)axonRec.Width - this.columnsCounter;
                if (colToFillin1s > 0)
                {
                    this.fillRect(colToFillin1s);
                }
                this.flowedOutVolume += volumeToPush;

            }
        }

        private void fillRect(int collToFill)
        {
            Console.WriteLine("In fill den");
            int colToFill = this.columnsCounter + collToFill;

            if (colToFill > recAxonArray[0].Length)
            {
                colToFill = recAxonArray[0].Length;
            }

            for (int i = this.columnsCounter; i < (colToFill); i++)
            {
                for (int j = 0; j < recAxonArray.Length; j++)
                {
                    recAxonArray[j][i].Fill = System.Windows.Media.Brushes.Blue;
                    recAxonArray[j][i].Refresh();

                }
            }

            this.columnsCounter += collToFill;

            if (this.columnsCounter >= recAxonArray[0].Length)
            {
                Console.WriteLine("In stop");
                isFull = true;
                this.columnsCounter = recAxonArray[0].Length - 1;

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
