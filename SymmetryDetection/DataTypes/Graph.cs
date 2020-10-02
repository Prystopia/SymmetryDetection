using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.DataTypes
{
    public class Graph
    {
        public bool[,] GraphDetails { get; set; }
        public int Size { get; set; }
        public Graph(int size)
        {
            Size = size;
            GraphDetails = new bool[size, size];
        }

        public void AddEdge(int from, int to)
        {
            GraphDetails[from - 1, to - 1] = true;
            GraphDetails[to - 1, from - 1] = true;
        }

        public bool[] GetCol(int colIndex)
        {
            bool[] column = new bool[Size];
            for (int i = 0; i < Size; i++)
            {
                column[i] = GraphDetails[i, colIndex];
            }
            return column;
        }

        public string Export()
        {
            StringBuilder sb = new StringBuilder();



            return sb.ToString();
        }
    }
}
