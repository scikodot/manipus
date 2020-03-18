﻿using System;
using System.Collections.Generic;
using Graphics;

namespace Logic.PathPlanning
{
    public class Tree
    {
        public class Node
        {
            public Entity Entity;

            public Node Parent;
            public List<Node> Childs;
            public int Layer;
            public Point p;
            public double[] q;

            public Node(Node parent, Point p, double[] q)
            {
                Parent = parent;
                Childs = new List<Node>();
                if (parent == null)
                    Layer = 0;
                else
                    Layer = parent.Layer + 1;
                this.p = p;
                this.q = Misc.CopyArray(q);
            }
        }

        public List<List<Node>> Layers;  // TODO: create indexer, make layers private
        public List<Node> Buffer;
        public int Count, LayersAdded;

        public Tree(Node root)
        {
            Layers = new List<List<Node>>
            {
                new List<Node>()
            };
            Buffer = new List<Node>();

            Layers[0].Add(root);
            Count = 1;
            LayersAdded = 0;
        }

        public Tree(Tree tree)
        {

        }

        public Node Root
        {
            get { return Layers[0][0]; }
        }

        public void AddLayer()
        {
            Layers.Add(new List<Node>());
            LayersAdded++;
        }

        public void RemoveLayer()
        {
            int LayerCount = Layers[Layers.Count - 1].Count;
            Layers.RemoveAt(Layers.Count - 1);
            Count -= LayerCount;
        }

        public void RemoveNode(Node n)
        {
            // delete node from layer
            Layers[n.Layer].Remove(n);

            // remove all childs of this node
            foreach (var node in n.Childs)
                RemoveNode(node);

            Count--;
        }

        public void AddNode(Node n)
        {
            Dispatcher.ActionsQueue.Enqueue(() =>
            {
                n.Entity = Window.CreateTreeBranch(n.p, n.Parent.p);
            });

            if (n.Layer == Layers.Count)
                AddLayer();

            n.Parent.Childs.Add(n);

            Layers[n.Layer].Add(n);
            Count++;

            //Buffer.Add(n);
        }

        public Node Min(Point p)
        {
            Node min_node = null;
            double min = double.PositiveInfinity;
            foreach (var layer in Layers)
            {
                foreach (var node in layer)
                {
                    double curr = p.DistanceTo(node.p);
                    if (curr < min)
                    {
                        min = curr;
                        min_node = node;
                    }
                }
            }
            return min_node;
        }

        public void Rectify(Node start)
        {
            Node node_curr = start.Parent;
            List<Node> nodes = new List<Node>();
            while (node_curr != null && node_curr.Parent != null && node_curr.Childs.Count == 1)
            {
                nodes.Add(node_curr);
                node_curr = node_curr.Parent;
            }

            if (nodes.Count > 0)
            {
                foreach (var node in nodes)
                {
                    RemoveNode(node);
                }
                start.Parent = node_curr;
            }
        }

        public void RectifyWhole()
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < Layers[i].Count; j++)
                {
                    Rectify(Layers[i][j]);
                }
            }
        }

        public static Node[] Discretize(Node start, Node end, int pointNum)
        {
            Segment seg = new Segment(start.p, end.p);
            Point[] points = new Point[pointNum];
            Array.Copy(seg.Discretize(pointNum + 1), 1, points, 0, pointNum);

            Node parent, child;
            if (start.Layer > end.Layer)
            {
                parent = end;
                child = start;
            }
            else
            {
                parent = start;
                child = end;
            }

            double[][] configs = new double[pointNum][];
            for (int i = 0; i < pointNum; i++)
            {
                double[] config = new double[start.q.Length];
                for (int j = 0; j < start.q.Length; j++)
                {
                    config[j] = start.q[j] + (i + 1) * (end.q[j] - start.q[j]) / (pointNum + 1);
                }
                configs[i] = config;
            }

            Node[] nodes = new Node[pointNum];
            for (int i = 0; i < pointNum; i++)
            {
                nodes[i] = new Node(null, points[i], configs[i]);
            }
            return nodes;
        }
    }
}
