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
        private int inhibition_radius = 5;
        private int cells_per_column;
        private int[][] input_cells;
        private InputCell[][] proxy_cells;
        private int[][] data;
        private double input_compression;
        private int[][] grid_columns;
        private int width;
        private int length;

        public HTM(int cells_in_column=4)
        {
            this.cells_per_column = cells_in_column;
            //this.input_cells = new NDArray(typeof(int), );
            
        }

        public void initialize_input(int[][] data, double comprssion_factor=1.0)
        {
            // compression_fctor: the ratio of input elements to columns
            this.data = data;
            this.input_compression = comprssion_factor;

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
            for (int i = 0; i < this.width; i++)
            {
                Column[] x_columns = new Column[this.length];
                for (int j = 0; j < this.length; j++)
                {
                    x_columns[j] = new Column(this, i, j, this.cells_per_column);
                }
            }
        }

        private void wire_columns_to_input(int input_width, int input_length)
        {
            for (int i=0; i < input_width; i++)
            {
                this.proxy_cells[i] = new InputCell[input_length];
                for (int j=0; j < input_length; j++)
                {
                    this.proxy_cells[i][j] = new InputCell(i, j, this.data);
                }
            }

            // set starting synapses permanence value near treshold
        }
    }
}
