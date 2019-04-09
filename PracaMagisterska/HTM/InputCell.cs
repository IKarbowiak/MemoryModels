using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMagisterska.HTM
{
    class InputCell
    {
        private int x;
        private int y;
        private int[][] input_data;
        private bool predicted;

        public InputCell(int x, int y, int[][] input_data)
        {
            // x, y are location of cell in input data
            this.x = x;
            this.y = y;
            this.input_data = input_data;
            this.predicted = false;
        }

    }
}
