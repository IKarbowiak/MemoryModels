using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumSharp.Core;

namespace PracaMagisterska.HTM
{
    class HTM
    {
        public int layer;
        private int inhibition_radius = 5;
        public int cells_per_column;
        private int[][] input_cells;
        private InputCell[][] proxy_cells;
        private int[][] data;
        private double input_compression;
        private Column[][] grid_columns;
        private int width;
        private int length;

        public HTM(int cells_in_column=HTM_parameters.CELLS_PER_COLUMN)
        {
            this.cells_per_column = cells_in_column;
            //this.input_cells = new NDArray(typeof(int), );
            
        }

        public void initialize_input(int[][] data, int layer, double comprssion_factor=1.0)
        {
            // compression_fctor: the ratio of input elements to columns
            this.data = data;
            this.input_compression = comprssion_factor;
            this.layer = layer;

            int input_width = data.Length;
            int input_length = data[0].Length;

            // get width and length of input cells
            this.width = (int)(input_width / this.input_compression);
            this.length = (int)(input_length / this.input_compression);

            this.create_columns();

            if (this.width * this.length < 45)
            {
                Console.WriteLine("Increase size of input to at least 45 cells.");
            }

            this.wire_columns_to_input(input_width, input_length);

        }

        private void create_columns()
        {
            this.grid_columns = new Column[this.width][];
            for (int i = 0; i < this.width; i++)
            {
                Column[] x_columns = new Column[this.length];
                for (int j = 0; j < this.length; j++)
                {
                    x_columns[j] = new Column(this, i, j, this.cells_per_column);
                }
                this.grid_columns[i] = x_columns;
            }
        }

        public IEnumerable<Column> get_columns()
        {
            for (int i=0; i < this.width; i++)
            {
                for (int j=0; j < this.length; j++)
                {
                    yield return this.grid_columns[i][j];
                }
            } 
        }

        private void wire_columns_to_input(int input_width, int input_length)
        {
            this.proxy_cells = new InputCell[input_width][];
            for (int i=0; i < input_width; i++)
            {
                this.proxy_cells[i] = new InputCell[input_length];
                for (int j=0; j < input_length; j++)
                {
                    this.proxy_cells[i][j] = new InputCell(i, j, this.data);
                }
            }

            int longer_side = input_width > input_length ? input_width : input_length;

            foreach (Column col in this.get_columns())
            {
                Random r = new Random();
                for (int i=0; i < HTM_parameters.SYNAPSES_PER_SEGMENT; i++)
                {
                    int inputx = r.Next(0, input_width - 1);
                    int inputy = r.Next(0, input_length - 1);
                    InputCell proxy_cell = this.proxy_cells[inputx][inputy];
                    double rand_permanence = this.SampleGaussian(r, HTM_parameters.CONNECTED_PERMANENCE, HTM_parameters.PERMANENCE_INCREMENT * 2);
                    double distance = col.calculate_distance(inputx, inputy, input_compression);
                    // bias permanence up toward column center as a gaussian distribution
                    // TODO: Figure out why it is desribe by this equation, maybe try with other
                    double locality_bias = ((double)HTM_parameters.INPUT_BIAS_PEAK / 0.4) 
                        * Math.Exp(Math.Pow((distance / ((double)longer_side * HTM_parameters.INPUT_BIAS_STD_DEV)), 2) / (-2));
                    Synapse synapse = new Synapse(proxy_cell, rand_permanence * locality_bias);
                    col.segment.add_synapse(synapse);
                }
            }

            // set starting synapses permanence value near treshold
        }

        public double SampleGaussian(Random random, double mean, double stddev)
        {
            // The method requires sampling from a uniform random of (0,1]
            // but Random.NextDouble() returns a sample of [0,1).
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1 * stddev + mean;
        }

        public IEnumerable<Cell> get_cells()
        {
            foreach (Column column in this.get_columns())
            {
                foreach (Cell cell in column.cells)
                {
                    yield return cell;
                }
            }
        }

        public Tuple<int, int> get_columns_grid_size()
        {
            return Tuple.Create(grid_columns.Length, grid_columns[0].Length);
        }
    }
}
