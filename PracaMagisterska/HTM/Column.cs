using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    class Column
    {
        private int min_overlap = 5;

        private int x;
        private int y;
        public bool active;
        private HTM htm;
        public Segment segment { get; set; }
        public Cell[] cells { get; set; }
        private int boost = 1;

        public Column(HTM htm_obj, int x, int y, int cells_in_column)
        {
            // column localization
            this.x = x;
            this.y = y;
            this.htm = htm_obj;
            this.active = false;
            this.segment = new Segment(distal: false);

            this.cells = new Cell[cells_in_column];
            for (int i=0; i < cells_in_column; i++)
            {
                this.cells[i] = new Cell(this, i);
            }
        }

        public double calculate_distance(int inputx, int inputy, double input_compression)
        {
            double x_range = (double)this.x * input_compression;
            double y_range = (double)this.y * input_compression;
            // calculate distance between cell in localization x, y and this column
            return Math.Sqrt( Math.Pow((double)inputx - x_range, 2) + Math.Pow((double)inputy - y_range, 2));
        }

    }
}
