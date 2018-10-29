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
using System.Timers;
using System.Threading;

namespace PracaMagisterska
{

    /// <summary>
    /// Interaction logic for Model1UC.xaml
    /// </summary>
    public partial class Model1UC : UserControl
    {
        public double length { get; set; }
        public double diameter { get; set; }
        public double speed { get; set; }
        public double input_surface { get; set; }
        public Rectangle[][] recArray;
        public int colToFilAfter1s;
        public int countColumns = 0;
        private int seconds = 0;
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer timer2;
        private bool unload = false;
        Color color = (Color)ColorConverter.ConvertFromString("#FFF4F4F5");

        public Model1UC()
        {
            InitializeComponent();
            this.length = 40;
            this.speed = 200;
            this.diameter = 0.4;
            this.input_surface = Math.Pow((diameter / 2), 2) * Math.PI;
            double volume = this.input_surface * length;

            this.splitRecModel();

        }

        public Model1UC(Model1UC m1uc)
        {
            InitializeComponent();

            this.model1UI.Height = m1uc.model1UI.Height;
            this.model1UI.Width = m1uc.model1UI.Height;
            this.model1UI.Fill = m1uc.model1UI.Fill;

            this.length = m1uc.length;
            double diameter = m1uc.diameter;
            this.speed = m1uc.speed;
            this.input_surface = Math.Pow((diameter / 2), 2) * Math.PI;
            double volume = this.input_surface * length;

            splitRecModel();


        }

        public void splitRecModel()
        {
            int rowNumber = Convert.ToInt32(this.model1UI.Height);
            int columnNumber = Convert.ToInt32(model1UI.Width);

            Rectangle rec;
            Console.WriteLine(rowNumber);
            Console.WriteLine(columnNumber);

            Console.WriteLine(gridModel1UC.Width);
            Console.WriteLine(gridModel1UC.Height);


            recArray = new Rectangle[rowNumber][];


            for (int i = 0; i < rowNumber; i++)
            {
                recArray[i] = new Rectangle[columnNumber];
                gridModel1UC.RowDefinitions.Add(new RowDefinition());

                for (int j = 0; j < columnNumber; j++)
                {
                    ColumnDefinition c1 = new ColumnDefinition();
                    c1.Width = new GridLength(1);
                    gridModel1UC.ColumnDefinitions.Add(c1);

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

                    gridModel1UC.Children.Add(rec);
                    Grid.SetColumn(rec, j);
                    Grid.SetRow(rec, i);

                }
            }



        }

        public double[] Flow(double time)
        {

            // Time after which liquid will begin to flow out
            double outFlowTime = this.length / this.speed;

            if (outFlowTime > time)
            {
                double[] res0 = { 0, 0 };
                return res0;
            }

            Console.WriteLine(recArray[0].Length);
            Console.WriteLine(outFlowTime);
            colToFilAfter1s = (int)((double)model1UI.Width / outFlowTime);

            Console.WriteLine("how much col");
            Console.WriteLine(colToFilAfter1s);

            if (colToFilAfter1s == 0)
            {
                colToFilAfter1s = 1;
            }
            //else if (colToFilAfter1s > 100) {
            //    colToFilAfter1s = 100;
            //}

            

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => { fillfunc(sender, e, time); };
            timer.Start();


            // The volume of liquid which flowed up
            double flowedVolume = this.input_surface * this.speed * (time - outFlowTime);
            double[] res = { outFlowTime, flowedVolume };
            return res;

        }

        public void fillfunc(object sender, EventArgs e, double time)
        {
            Console.WriteLine("I am in fill");
            if (!unload) {
                int colToFill = countColumns + colToFilAfter1s;

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

                countColumns += colToFilAfter1s;
                seconds++;


                if (seconds == (int)time || countColumns >= recArray[0].Length)
                {
                    countColumns = recArray[0].Length - 1;
                    unload = true;
                    seconds = 0;
                    timer.Stop();
                    Console.WriteLine("Stop timer 1");

                    Thread.Sleep((int)time - seconds);
                    timer2 = new System.Windows.Threading.DispatcherTimer();
                    timer2.Interval = TimeSpan.FromSeconds((int)time - seconds);
                    timer2.Tick += (sender2, e2) => {unloadFunc(sender2, e2); };
                    timer2.Start();
                }
            }

        }

        public void unloadFunc(object sender, EventArgs e)
        {
            Console.WriteLine("I am here");
            Console.WriteLine(countColumns);
            bool stop = false;

            int colToUnfill = countColumns - colToFilAfter1s;

            if (colToUnfill <= 0)
            {
                colToUnfill = 0;
                stop = true;
            }

            for (int i = countColumns; i >= (colToUnfill); i--)
            {
                for (int j = 0; j < recArray.Length; j++)
                {
                    recArray[j][i].Fill = System.Windows.Media.Brushes.Transparent;
                    recArray[j][i].Refresh();

                }
            }

            countColumns -= colToFilAfter1s;

            if (stop)
            {
                Console.WriteLine("Stop timer2");
                timer2.Stop();
            }
            
        }

    
    }


}
