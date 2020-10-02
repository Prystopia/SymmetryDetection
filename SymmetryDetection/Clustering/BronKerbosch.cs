using SymmetryDetection.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SymmetryDetection.Clustering
{
    public class BronKerbosch
    {
        public Graph GraphDetails { get; set; }
        private List<IList<int>> Cliques { get; set; }
        public BronKerbosch(Graph graph)
        {
            GraphDetails = graph;
            Cliques = new List<IList<int>>();
        }

        public List<IList<int>> RunAlgorithm(int minCliqueSize)
        {
            int vertices = GraphDetails.Size;
            List<int> existingVerts = new List<int>();
            for (int i = 0; i < vertices; i++)
            {
                existingVerts.Add(i);
            }
            RecursiveCliques(new List<int>(), existingVerts, new List<int>());
            return Cliques;
        }

        private void RecursiveCliques(IList<int> R, IList<int> P, IList<int> X)
        {
            if (P.Count() == 0 && X.Count() == 0)
            {
                if (!Cliques.Any(p => p.All(R.Contains)))
                {
                    Cliques.Add(R);
                }
            }
            else
            {

                foreach (int vertex in P)
                {
                    var newR = new List<int>(R);
                    newR.Add(vertex);
                    var neighbours = GetNeighbours(vertex);
                    var newP = P.Except(new List<int>(vertex)).Intersect(neighbours).ToList();
                    var newX = X.Intersect(neighbours).ToList();
                    RecursiveCliques(newR, newP, newX);
                    X.Add(vertex);
                }
            }
        }
        private List<int> GetNeighbours(int vertex)
        {
            List<int> neighbours = new List<int>();
            var column = GraphDetails.GetCol(vertex);
            for (int i = 0; i < column.Length; i++)
            {
                if (column[i])
                {
                    neighbours.Add(i);
                }
            }
            return neighbours;
        }
    }
}
