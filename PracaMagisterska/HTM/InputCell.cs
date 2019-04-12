using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    public class InputCell
    {
        public int x;
        public int y;
        private int[][] input_data;
        private bool predicted;
        public bool active;
        public bool learning;

        public InputCell(int x, int y, int[][] input_data)
        {
            // x, y are location of cell in input data
            this.x = x;
            this.y = y;
            this.input_data = input_data;
            this.predicted = false;
            this.learning = false;
        }

        // TODO: Fix was and i active
        public bool is_active()
        {
            return this.input_data[this.x][this.y] == 1 ? true : false;
        }

        public bool was_active()
        {
            return this.input_data[this.x][this.y] == 1 ? true : false;
        }

    }
}
