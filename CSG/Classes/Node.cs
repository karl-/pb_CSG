using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Parabox.CSG
{
    sealed class Node
    {
        public List<Polygon> polygons;

        public Node front;
        public Node back;

        public Plane plane;

        public Node()
        {
            front = null;
            back = null;
        }

        public Node(List<Polygon> list)
        {
            Build(list);
        }

        public Node(List<Polygon> list, Plane plane, Node front, Node back)
        {
            this.polygons = list;
            this.plane = plane;
            this.front = front;
            this.back = back;
        }

        public Node Clone()
        {
            Node clone = new Node(this.polygons, this.plane, this.front, this.back);

            return clone;
        }

        // Remove all polygons in this BSP tree that are inside the other BSP tree
        // `bsp`.
        public void ClipTo(Node other)
        {
            this.polygons = other.ClipPolygons(this.polygons);

            if (this.front != null)
            {
                this.front.ClipTo(other);
            }

            if (this.back != null)
            {
                this.back.ClipTo(other);
            }
        }

        // Convert solid space to empty space and empty space to solid space.
        public void Invert()
        {
            for (int i = 0; i < this.polygons.Count; i++)
                this.polygons[i].Flip();

            this.plane.Flip();

            if (this.front != null)
            {
                this.front.Invert();
            }

            if (this.back != null)
            {
                this.back.Invert();
            }

            Node tmp = this.front;
            this.front = this.back;
            this.back = tmp;
        }

        // Build a BSP tree out of `polygons`. When called on an existing tree, the
        // new polygons are filtered down to the bottom of the tree and become new
        // nodes there. Each set of polygons is partitioned using the first polygon
        // (no heuristic is used to pick a good split).
        public void Build(List<Polygon> list)
        {
            Stack<(Node, List<Polygon>)> buildStack = new Stack<(Node, List<Polygon>)>();
            buildStack.Push((this, list));

            while (buildStack.Count > 0)
            {
                var current = buildStack.Pop();
                Node currentNode = current.Item1;
                List<Polygon> currentList = current.Item2;

                if (currentList.Count < 1)
                    continue;

                bool newNode = currentNode.plane == null || !currentNode.plane.Valid();

                if (newNode)
                {
                    currentNode.plane = new Plane();
                    currentNode.plane.normal = currentList[0].plane.normal;
                    currentNode.plane.w = currentList[0].plane.w;
                }

                if (currentNode.polygons == null)
                    currentNode.polygons = new List<Polygon>();

                var listFront = new List<Polygon>();
                var listBack = new List<Polygon>();

                for (int i = 0; i < currentList.Count; i++)
                    currentNode.plane.SplitPolygon(currentList[i], currentNode.polygons, currentNode.polygons, listFront, listBack);

                if (listFront.Count > 0)
                {
                    if (newNode && currentList.SequenceEqual(listFront))
                        currentNode.polygons.AddRange(listFront);
                    else
                    {
                        if (currentNode.front == null)
                            currentNode.front = new Node();
                        buildStack.Push((currentNode.front, listFront));
                    }
                }

                if (listBack.Count > 0)
                {
                    if (newNode && currentList.SequenceEqual(listBack))
                        currentNode.polygons.AddRange(listBack);
                    else
                    {
                        if (currentNode.back == null)
                            currentNode.back = new Node();
                        buildStack.Push((currentNode.back, listBack));
                    }
                }
            }
        }

        // Recursively remove all polygons in `polygons` that are inside this BSP tree.
        public List<Polygon> ClipPolygons(List<Polygon> list)
        {
            if (!this.plane.Valid())
            {
                return list;
            }

            List<Polygon> list_front = new List<Polygon>();
            List<Polygon> list_back = new List<Polygon>();

            for (int i = 0; i < list.Count; i++)
            {
                this.plane.SplitPolygon(list[i], list_front, list_back, list_front, list_back);
            }

            if (this.front != null)
            {
                list_front = this.front.ClipPolygons(list_front);
            }

            if (this.back != null)
            {
                list_back = this.back.ClipPolygons(list_back);
            }
            else
            {
                list_back.Clear();
            }

            // Position [First, Last]
            // list_front.insert(list_front.end(), list_back.begin(), list_back.end());
            list_front.AddRange(list_back);

            return list_front;
        }

        // Return a list of all polygons in this BSP tree.
        public List<Polygon> AllPolygons()
        {
            List<Polygon> list = this.polygons;
            List<Polygon> list_front = new List<Polygon>(), list_back = new List<Polygon>();

            if (this.front != null)
            {
                list_front = this.front.AllPolygons();
            }

            if (this.back != null)
            {
                list_back = this.back.AllPolygons();
            }

            list.AddRange(list_front);
            list.AddRange(list_back);

            return list;
        }

        #region STATIC OPERATIONS

        // Return a new CSG solid representing space in either this solid or in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static Node Union(Node a1, Node b1)
        {
            Node a = a1.Clone();
            Node b = b1.Clone();
        
            a.ClipTo(b);
            b.ClipTo(a);
            b.Invert();
            b.ClipTo(a);
            b.Invert();
        
            a.Build(b.AllPolygons());
        
            Node ret = new Node(a.AllPolygons());
        
            return ret;
        }
        
        // Return a new CSG solid representing space in this solid but not in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static Node Subtract(Node a1, Node b1)
        {
            Node a = a1.Clone();
            Node b = b1.Clone();
        
            a.Invert();
            a.ClipTo(b);
            b.ClipTo(a);
            b.Invert();
            b.ClipTo(a);
            b.Invert();
            a.Build(b.AllPolygons());
            a.Invert();
        
            Node ret = new Node(a.AllPolygons());
        
            return ret;
        }

        // Return a new CSG solid representing space both this solid and in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static Node Intersect(Node a1, Node b1)
        {
            Node a = a1.Clone();
            Node b = b1.Clone();

            a.Invert();
            b.ClipTo(a);
            b.Invert();
            a.ClipTo(b);
            b.ClipTo(a);

            a.Build(b.AllPolygons());
            a.Invert();

            Node ret = new Node(a.AllPolygons());

            return ret;
        }

        #endregion
    }
}
