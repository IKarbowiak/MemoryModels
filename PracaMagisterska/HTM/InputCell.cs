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
        public List<List<int>> input_data;
        private bool predicted;
        public bool active;
        public bool learning;
        public int value;

        public InputCell(int x, int y, int value)
        {
            // x, y are location of cell in input data
            this.x = x;
            this.y = y;
            this.value = value;
            //this.input_data = input_data;
            this.predicted = false;
            this.learning = false;
        }

        public bool was_active()
        {
            return this.value == 1 ? true : false;
        }

    }
}
