using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Tetris
{
    class Figure
    {
        private static Random rnd = new Random();

        int x, y;

        int[,] mx;

        public Figure(int _x)
        {
            x = _x;
            y = 0;
        }

        public Figure()
       {
            int figType = rnd.Next(7);

           x = 4;

            switch (figType)
            {
                //square
                case 0:
                    mx = new int[,]{{1,1},{1,1}};
                    break;
                //stright line
                case 1:
                    mx = new int[,]{{1},{1},{1},{1}};
                    break;
                //L
                case 2:
                    mx = new int[,]{{1,0}, {1,0}, {1,1}};
                    break;
                //L-m
                case 3:
                    mx = new int[,]{{0,1}, {0,1}, {1,1}};
                    break;
                //S
                case 4:
                    mx = new int[,]{{0,1},{1,1},{1,0}};
                    break;
                //S
                case 5:
                    mx = new int[,]{{1,0},{1,1},{0,1}};
                    break;
                //T
                case 6:
                    mx = new int[,]{{1,0},{1,1},{1,0}};
                    break;
                //T
                case 7:
                    mx = new int[,]{{0,1},{1,1},{0,1}};
                    break;
            }

        }

        public int X {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public int[,] Matrix
        {
            get { return mx;  }
            set { mx = value; }
        }

    }
}
