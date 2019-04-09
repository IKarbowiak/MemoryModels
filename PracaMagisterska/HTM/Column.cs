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
        private bool active;
        private HTM htm;
        private Segment segment;
        private Cell[] cells;
        private int boost = 1;

        public Column(HTM htm_obj, int x, int y, int cells_in_column)
        {
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
    }
}
