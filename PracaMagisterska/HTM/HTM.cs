using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class HTM
    {
        public int layer;
        public int inhibition_radius = HTM_parameters.INHIBITION_RADIUS; // Average connected receptive field size of the columns.
        public int cells_per_column;
        private int[][] input_cells;
        private InputCell[][] proxy_cells;
        public List<List<int>> data;
        private double input_compression;
        private Column[][] grid_columns;
        private int width;
        private int length;
        private UpdateSegments update_segments;
        private Random rnd;

        public HTM(int cells_in_column=HTM_parameters.CELLS_PER_COLUMN)
        {
            this.cells_per_column = cells_in_column;
            this.update_segments = new UpdateSegments();
            rnd = new Random();
        }

        public void initialize_input(List<List<int>> data, int layer, double cell_demage, double compression_factor=1.0)
        {
            // compression_fctor: the ratio of input elements to columns
            this.data = data;
            this.input_compression = compression_factor;
            this.layer = layer;

            int input_width = data.Count;
            int input_length = data[0].Count;

            // get width and length of input cells
            this.width = (int)(input_width / this.input_compression);
            this.length = (int)(input_length / this.input_compression);

            this.create_columns();

             if (this.width * this.length < 45)
            {
                Console.WriteLine("Increase size of input to at least 45 cells.");
            }

            this.wire_columns_to_input(input_width, input_length);

            if (cell_demage > 0)
            {
                this.demageCells(cell_demage);
            }
        }

        public void demageCells(double demage_cells)
        {
            int total_cell_num = this.width * this.length * this.cells_per_column - 1;
            int cell_to_demage = (int)(total_cell_num * demage_cells / 100);
            List<int> cells_list_to_demage = new List<int>();
            int index;
            for (int i = 0; i < cell_to_demage; i++)
            {
                do
                {
                    index = this.rnd.Next(0, total_cell_num);
                } while (cells_list_to_demage.Contains(index));
                
                cells_list_to_demage.Add(index);
            }

            int counter = 0;
            foreach( Cell cell in this.get_cells())
            {
                if (cells_list_to_demage.Contains(counter))
                {
                    cell.demage = true;
                }
                counter++;
            }
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

        public List<Cell> get_cells()
        {
            List<Cell> cells_list = new List<Cell>(); 
            foreach (Column column in this.get_columns())
            {
                foreach (Cell cell in column.get_cells())
                {
                    cells_list.Add(cell);
                    //yield return cell;
                }
            }
            return cells_list;
        }

        public Tuple<int, int> get_columns_grid_size()
        {
            return Tuple.Create(grid_columns.Length, grid_columns[0].Length);
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

                for (int i=0; i < HTM_parameters.SYNAPSES_PER_SEGMENT; i++)
                {
                    int inputx = this.rnd.Next(0, input_width - 1);
                    int inputy = this.rnd.Next(0, input_length - 1);
                    InputCell proxy_cell = this.proxy_cells[inputx][inputy];
                    double rand_permanence = this.SampleGaussian(this.rnd, HTM_parameters.CONNECTED_PERMANENCE, HTM_parameters.PERMANENCE_INCREMENT * 2);
                    double distance = col.calculate_distance(inputx, inputy, input_compression);
                    // bias permanence up toward column center as a gaussian distribution
                    double locality_bias = ((double)HTM_parameters.INPUT_BIAS_PEAK / 0.4) 
                        * Math.Exp(Math.Pow((distance / ((double)longer_side * HTM_parameters.INPUT_BIAS_STD_DEV)), 2) / (-2));
                    Synapse synapse = new Synapse(input_cell: proxy_cell, permanence: rand_permanence * locality_bias);
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

        public bool update_data(List<List<int>> new_data)
        {
            if (new_data.Count == this.data.Count && new_data[0].Count == this.data[0].Count)
            {
                this.data = new_data;
                return true;
            }
            return false;
        }

        public int lower_limit(int value)
        {
            return Math.Max(0, Math.Min(value - 1, value - inhibition_radius));
        }

        public int upper_limit(int value, int edge)
        {
            return Math.Min(edge, Math.Max(value + 1, value + inhibition_radius));
        }

        public List<Column> neighbors(Column col)
        {
            List<Column> neighbors_columns = new List<Column>();
            
            // calculate boundaries of neighbour
            int start_x = this.lower_limit(col.x);
            int start_y = this.lower_limit(col.y);

            int end_x = this.upper_limit(col.x, this.width);
            int end_y = this.upper_limit(col.y, this.length);

            for (int i = start_x; i < end_x; i++)
            {
                for (int j = start_y; j < end_y; j++)
                {
                    neighbors_columns.Add(this.grid_columns[i][j]);
                }
            }

            return neighbors_columns;
        }

        public List<Column> get_active_columns()
        {
            List<Column> active_columns = new List<Column>();
            foreach (Column column in this.get_columns())
            {
                if (column.active)
                    active_columns.Add(column);
            }

            return active_columns;
        }

        public void execute(bool learning = true)
        {
            foreach(Cell cell in this.get_cells())
            {
                cell.clock_tick();
            }
            SpatialPool spatial_pool = new SpatialPool();
            spatial_pool.perform(this);

            TemporalPool temporal_pool = new TemporalPool(this, learning, this.update_segments);
            UpdateSegments update_segments = temporal_pool.perform();

        }

        public double average_receptive_field_size()
        {
            // docs Numenta: The radius of the average connected receptive field size of all the columns. 
            // The connected receptive field size of a column includes only the connected synapses (those with permanence values >= connectedPerm).
            // This is used to determine the extent of lateral inhibition between columns
            List<double> radius_list = new List<Double>();
            foreach (Column col in this.get_columns())
            {
                foreach (Synapse synapse in col.get_connected_synapses())
                {
                    radius_list.Add(Math.Sqrt(Math.Pow((double)(col.x - synapse.input_cell.x), 2) + Math.Pow((double)(col.y - synapse.input_cell.y), 2)));
                }
            }

            return radius_list.Sum() / radius_list.Count();
        }

    }

    static class MyExtensions
    {
        private static Random rnd = new Random();
        public static void shuffle<T>(this IList<T> list)
        {
            int n = list.Count();
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
