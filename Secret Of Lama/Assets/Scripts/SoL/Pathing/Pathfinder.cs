using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Pathing
{

    public class Pathfinder
    {
        public static PathingGrid grid;


        public const byte cameFromLeft = 0;
        public const byte cameFromDown = 1;
        public const byte cameFromRight = 2;
        public const byte cameFromUp = 3;
        public const byte cameFromDownLeft = 4;
        public const byte cameFromDownRight = 5;
        public const byte cameFromUpRight = 6;
        public const byte cameFromUpLeft = 7;

        private class PathingNode : IEquatable<PathingNode>
        {
            public Vector2Int p;
            public int hCost, fCost;
            protected int _gCost;
            public bool open;
            public PathingNode parent;

            public int gCost
            {
                get
                {
                    return _gCost;
                }
                set
                {
                    _gCost = value;
                    fCost = hCost + _gCost;
                }
            }
            public byte cameFrom;




            public PathingNode(Vector2Int position, byte cameFrom, int gCost, PathingNode parent = null)
            {
                p = position;
                this.cameFrom = cameFrom;
                this.gCost = gCost;
                open = true;
                this.parent = parent;
            }

            public override int GetHashCode()
            {
                return p.GetHashCode();
            }

            public bool Equals(PathingNode other)
            {
                return p.x == other.p.x && p.y == other.p.y;
            }

            public void CalcHeuristicCost(Vector2Int target)
            {
                hCost = System.Math.Abs(target.x - p.x) + System.Math.Abs(target.y - p.y);
                fCost = hCost + _gCost;
            }
        }


        public static Path GetPath(Vector3 from, Vector3 to, int maxAllowedCycles = 1000)
        {
            Vector2Int start = new Vector2Int(Mathf.FloorToInt(from.x), Mathf.FloorToInt(from.y));
            Vector2Int target = new Vector2Int(Mathf.FloorToInt(to.x), Mathf.FloorToInt(to.y));

            PathingNode targetNode = new PathingNode(target, 0, int.MaxValue);
            PathingNode lastNode = null;
            //TODO if performance sucks, implement chunk portals and check paths in chunks first.

            List<PathingNode> open = new List<PathingNode>();
            List<PathingNode> closed = new List<PathingNode>();
            open.Add(new PathingNode(start, 0, 0));

            int cycles = 0;

            while (open.Count > 0)
            {
                cycles++;
                if (cycles > maxAllowedCycles)
                    return null;
                var n = open[0];

                if (n.Equals(targetNode))
                {
                    int numClosed = closed.Count;
                    if (numClosed <= 0)
                        return null;
                   
                    targetNode.parent = lastNode;
                    break;
                }

                var t = n.p.MoveUp();
                var l = n.p.MoveLeft();
                var r = n.p.MoveRight();
                var b = n.p.MoveDown();
                var tl = n.p.MoveUpLeft();
                var tr = n.p.MoveUpRight();
                var bl = n.p.MoveDownLeft();
                var br = n.p.MoveDownRight();


                bool added = false;
                int gCost = n.gCost + 1;

                n.open = false;
                closed.Add(n);
                open.RemoveAt(0);


                bool lAllowed = !grid[t.x, t.y];
                bool rAllowed = !grid[r.x, r.y];
                bool bAllowed = !grid[b.x, b.y];
                bool tAllowed = !grid[tl.x, tl.y];

                if (tAllowed)
                {
                    var n0 = new PathingNode(t, cameFromDown, gCost, n);
                    var existing = open.Find((searchNode) => searchNode.Equals(n0));
                    if (existing == null)
                    {
                        n0.CalcHeuristicCost(target);
                        open.Add(n0);
                        added = true;
                    }
                    else if (existing.open)
                    {
                        existing.gCost = gCost;
                        existing.cameFrom = n0.cameFrom;
                        existing.parent = n;
                    }
                }
                if (bAllowed)
                {
                    var n0 = new PathingNode(b, cameFromUp, gCost, n);
                    var existing = open.Find((searchNode) => searchNode.Equals(n0));
                    if (existing == null)
                    {
                        n0.CalcHeuristicCost(target);
                        open.Add(n0);
                        added = true;
                    }
                    else if (existing.open)
                    {
                        existing.gCost = gCost;
                        existing.cameFrom = n0.cameFrom;
                        existing.parent = n;
                    }
                }
                if (rAllowed)
                {
                    var n0 = new PathingNode(r, cameFromLeft, gCost, n);
                    var existing = open.Find((searchNode) => searchNode.Equals(n0));
                    if (existing == null)
                    {
                        n0.CalcHeuristicCost(target);
                        open.Add(n0);
                        added = true;
                    }
                    else if (existing.open)
                    {
                        existing.gCost = gCost;
                        existing.cameFrom = n0.cameFrom;
                        existing.parent = n;
                    }
                }
                if (lAllowed)
                {
                    var n0 = new PathingNode(l, cameFromRight, gCost, n);

                    var existing = open.Find((searchNode) => searchNode.Equals(n0));
                    if (existing == null)
                    {
                        n0.CalcHeuristicCost(target);
                        open.Add(n0);
                        added = true;
                    }
                    else if (existing.open)
                    {
                        existing.gCost = gCost;
                        existing.cameFrom = n0.cameFrom;
                    }
                }

                if (tAllowed && lAllowed)
                    if (!grid[tl.x, tl.y])
                    {
                        var n0 = new PathingNode(tl, cameFromDownRight, gCost, n);
                        var existing = open.Find((searchNode) => searchNode.Equals(n0));
                        if (existing == null)
                        {
                            n0.CalcHeuristicCost(target);
                            open.Add(n0);
                            added = true;
                        }
                        else if (existing.open)
                        {
                            existing.gCost = gCost;
                            existing.cameFrom = n0.cameFrom;
                        }
                    }
                if (tAllowed && rAllowed)
                    if (!grid[tr.x, tr.y])
                    {
                        var n0 = new PathingNode(tr, cameFromDownLeft, gCost, n);
                        var existing = open.Find((searchNode) => searchNode.Equals(n0));
                        if (existing == null)
                        {
                            n0.CalcHeuristicCost(target);
                            open.Add(n0);
                            added = true;
                        }
                        else if (existing.open)
                        {
                            existing.gCost = gCost;
                            existing.cameFrom = n0.cameFrom;
                        }
                    }
                if (bAllowed && lAllowed)
                    if (!grid[bl.x, bl.y])
                    {
                        var n0 = new PathingNode(bl, cameFromUpRight, gCost, n);
                        var existing = open.Find((searchNode) => searchNode.Equals(n0));
                        if (existing == null)
                        {
                            n0.CalcHeuristicCost(target);
                            open.Add(n0);
                            added = true;
                        }
                        else if (existing.open)
                        {
                            existing.gCost = gCost;
                            existing.cameFrom = n0.cameFrom;
                        }
                    }
                if (bAllowed && rAllowed)
                    if (!grid[br.x, br.y])
                    {
                        var n0 = new PathingNode(br, cameFromUpLeft, gCost, n);
                        var existing = open.Find((searchNode) => searchNode.Equals(n0));
                        if (existing == null)
                        {
                            n0.CalcHeuristicCost(target);
                            open.Add(n0);
                            added = true;
                        }
                        else if (existing.open)
                        {
                            existing.gCost = gCost;
                            existing.cameFrom = n0.cameFrom;
                        }
                    }

                if (added)
                    open.Sort((node0, node1) =>
                    {
                        return node0.fCost.CompareTo(node1.fCost);
                    });

                lastNode = n;

            }

            List<Vector3> points = new List<Vector3>();
            PathingNode nBacktrack = targetNode;
            while (nBacktrack != null)
            {
                points.Add(new Vector3(nBacktrack.p.x + 0.5f, nBacktrack.p.y + 0.5f, 0f));
                nBacktrack = nBacktrack.parent;
            }
            points.Reverse();
            points.RemoveAt(0);

            Path p = new Path();
            p.positions = points.ToArray();

            return p;
        }

    }
}
