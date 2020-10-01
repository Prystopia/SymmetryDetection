using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.DataTypes
{
    public class Graph<T>
    {
        public List<Node<T>> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
        public Graph()
        {
            Nodes = new List<Node<T>>();
            Edges = new List<Edge>();
        }
        public Graph(List<Node<T>> nodes)
        {
            Nodes = nodes;
            Edges = new List<Edge>();
        }
        public void AddEdge(int nodeFrom, int nodeTo)
        {
            Edges.Add(new Edge(nodeFrom, nodeTo));
        }
        public void AddNode(Node<T> node)
        {
            Nodes.Add(node);
        }
        public List<Node<T>> GetNodeNeighbours(Node<T> srcNode)
        {

        }
        public void RemoveNode(Node<T> node)
        {

        }
    }
}
