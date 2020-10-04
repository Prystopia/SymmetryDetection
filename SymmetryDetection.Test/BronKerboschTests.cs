using System;
using SymmetryDetection.Clustering;
using SymmetryDetection.DataTypes;
using Xunit;

namespace SymmetryDetection.Test
{
    public class BronKerboschTests
    {
        public BronKerboschTests()
        {
        }

        [Fact]
        public void Test_Bron_Kerbosch()
        {
            //example taken from wikipedia
            Graph graph = new Graph(6);
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 4);
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 4);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 5);
            graph.AddEdge(3, 4);
            BronKerbosch algo = new BronKerbosch(graph);
            var cliques = algo.RunAlgorithm(1);

            Assert.Equal(5, cliques.Count);

            Assert.Contains(cliques, c => c.Contains(5) && c.Contains(3));
            Assert.Contains(cliques, c => c.Contains(3) && c.Contains(2));
            Assert.Contains(cliques, c => c.Contains(3) && c.Contains(4));
            Assert.Contains(cliques, c => c.Contains(2) && c.Contains(1));
            Assert.Contains(cliques, c => c.Contains(0) && c.Contains(1) && c.Contains(4));
        }
    }
}
