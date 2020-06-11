using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Physics
{


    public class QuadTree
    {
        public struct Position
        {
            public int x;
            public int y;
            public int depth;

            public override string ToString()
            {
                return "(" + x + ", " + y + ", " + depth + ")";
            }
        }

        public class Agent
        {
            public Transform t;
            public QuadTreeAgentBehaviour ab;
            public Position p;
            public Bounds b;
            public byte[] extents;
            // 0 - l, 1 r, 2, down, 3 up


            public Agent(Transform t, Bounds b, QuadTree qt)
            {
                this.t = t;
                extents = new byte[4];
                ab = t.gameObject.GetComponent<QuadTreeAgentBehaviour>();
                this.b = b;
                CalculateExtents(qt);
            }

            public override int GetHashCode()
            {
                return t.GetHashCode();
            }

            public void CalculateExtents(QuadTree qt)
            {
                Vector2 local = qt.GetLocalNodePosition(this);

                int e = Mathf.FloorToInt((local.x - b.extents.x) / qt.LeafSize);
                extents[0] = (e < 0) ? (byte)-e : (byte)0;

                e = Mathf.FloorToInt((local.x + b.extents.x) / qt.LeafSize);
                extents[1] = (e > 0) ? (byte)e : (byte)0;

                e = Mathf.FloorToInt((local.y - b.extents.y) / qt.LeafSize);
                extents[2] = (e < 0) ? (byte)-e : (byte)0;

                e = Mathf.FloorToInt((local.y + b.extents.y) / qt.LeafSize);
                extents[3] = (e > 0) ? (byte)e : (byte)0;

            }
        }

        private uint sizeHalf;
        private uint[] sizeAtDepth;

        protected int maxDepth;
        public uint LeafSize
        {
            get
            {
                return sizeAtDepth[maxDepth];
            }
        }

        private int[] startIndecesForDepth;
        private int numLeaves;

        private List<Agent>[] nodeInhabitants;


        public QuadTree(uint size, uint leafSize)
        {
            maxDepth = (int)Math.FloorLog2(size / leafSize);
            sizeAtDepth = new uint[maxDepth + 1];
            if (size % 2 == 1)
                throw new Exception("size must be a multiple of 2.");

            sizeAtDepth[0] = size;
            sizeHalf = size / 2;
            if (size % leafSize != 0)
            {
                throw new Exception("Leaf size must be a divisor of the total area covered! " + size + " is not dividable by " + leafSize);
            }

            startIndecesForDepth = new int[maxDepth + 1];

            startIndecesForDepth[0] = 0;
            for (int i = 1; i < maxDepth + 1; i++)
            {
                startIndecesForDepth[i] = startIndecesForDepth[i - 1] + (1 << ((i - 1) * 2));
                sizeAtDepth[i] = sizeAtDepth[i - 1] / 2;
            }

            numLeaves = (1 << maxDepth) * (1 << maxDepth);
            nodeInhabitants = new List<Agent>[numLeaves];
            for (int i = 0; i < numLeaves; i++)
                nodeInhabitants[i] = new List<Agent>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leafNodeId">index of leaf node, starting at 0</param>
        /// <returns></returns>
        public IEnumerable<Agent> GetAgents(Vector3 p)
        {
            foreach (var agent in nodeInhabitants[GetLeafContaining(p)])
            {
                p.z = agent.t.position.z;
                if (agent.b.Contains(p - agent.t.position))
                {
                    yield return agent;
                }
            }

            yield break;
        }

        public IEnumerable<Agent> GetAgentsInRange(Vector3 p, float radius)
        {
            var originLeaf = GetLeafContaining(p);

            foreach (var a in GetAgentsInNodeInRange(originLeaf, p, radius))
                yield return a;

            int extents = Mathf.RoundToInt(radius / LeafSize);
            if (extents > 0)
            {
                var pos = GetLeafPosition(originLeaf);

                for (int i = 0; i < extents; i++)
                {
                    foreach (var a in GetAgentsInNodeInRange(GetLeafIndex(pos.x - i, pos.y), p, radius))
                        yield return a;
                    foreach (var a in GetAgentsInNodeInRange(GetLeafIndex(pos.x + i, pos.y), p, radius))
                        yield return a;
                    foreach (var a in GetAgentsInNodeInRange(GetLeafIndex(pos.x, pos.y - i), p, radius))
                        yield return a;
                    foreach (var a in GetAgentsInNodeInRange(GetLeafIndex(pos.x, pos.y + i), p, radius))
                        yield return a;
                    foreach (var a in GetAgentsInNodeInRange(GetLeafIndex(pos.x - i, pos.y - i), p, radius))
                        yield return a;
                    foreach (var a in GetAgentsInNodeInRange(GetLeafIndex(pos.x + i, pos.y - i), p, radius))
                        yield return a;
                    foreach (var a in GetAgentsInNodeInRange(GetLeafIndex(pos.x - i, pos.y + i), p, radius))
                        yield return a;
                    foreach (var a in GetAgentsInNodeInRange(GetLeafIndex(pos.x + i, pos.y + i), p, radius))
                        yield return a;

                }
            }
        }

        public bool RemoveAgent(Agent physicsAgent)
        {
            return nodeInhabitants[physicsAgent.ab.node].Remove(physicsAgent);
        }

        private int RemoveNullAgents(int nodeId)
        {
            return nodeInhabitants[nodeId].RemoveAll((agent) => agent.t == null);
        }

        private IEnumerable<Agent> GetAgentsInNodeInRange(int nodeId, Vector3 p, float radius)
        {
            RemoveNullAgents(nodeId);
            foreach (var agent in nodeInhabitants[nodeId])
            {

                Vector3 diff = (agent.t.position + agent.b.center) - p;
                float d = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
                if (d < radius)
                    yield return agent;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="leafNodeId">index of leaf node, starting at 0</param>
        /// <returns></returns>
        public List<Agent> GetAgents(int leafNodeId)
        {
            return nodeInhabitants[leafNodeId];
        }

        public Position GetPosition(int index)
        {
            int d = 0;
            for (int i = maxDepth - 1; i >= 0; i--)
            {
                if (index >= startIndecesForDepth[i])
                {
                    d = i;
                    break;
                }
            }

            int posIndex = index - startIndecesForDepth[d];
            int sDepth = 1 << d;
            int y = posIndex / sDepth;
            int x = posIndex % sDepth;

            return new Position()
            {
                x = x,
                y = y,
                depth = d
            };
        }

        public Position GetLeafPosition(int leafIndex)
        {

            int sDepth = 1 << maxDepth;
            int y = leafIndex / sDepth;
            int x = leafIndex % sDepth;

            return new Position()
            {
                x = x,
                y = y,
                depth = maxDepth
            };
        }



        public void PlaceAgent(Agent a, int nodeId)
        {
            if (nodeId != a.ab.node)
            {
                if (a.ab.node >= 0)
                {
                    nodeInhabitants[a.ab.node].Remove(a);
                }
                nodeInhabitants[nodeId].Add(a);
                a.ab.node = nodeId;
            }
        }

        public Position PlaceAgent(Transform t, Bounds b)
        {
            return PlaceAgent(new Agent(t, b, this));
        }

        public Position PlaceAgent(Agent a)
        {
            Vector2 p = (Vector2)(a.t.position + a.b.center) + new Vector2(sizeHalf, sizeHalf);
            int d = maxDepth;
            uint s = LeafSize;
            a.p = new Position()
            {
                depth = d,
                x = Mathf.FloorToInt(p.x / s),
                y = Mathf.FloorToInt(p.y / s)
            };
            PlaceAgent(a, GetIndex(a.p) - startIndecesForDepth[d]);
            a.CalculateExtents(this);
            return a.p;
        }

        protected int GetLeafContaining(Vector2 v)
        {
            Vector2 p = v + new Vector2(sizeHalf, sizeHalf);
            uint s = LeafSize;
            int x = Mathf.FloorToInt(p.x / s);
            int y = Mathf.FloorToInt(p.y / s);
            return GetLeafIndex(x, y);
        }

        public int GetIndex(int x, int y, int depth)
        {
            int sDepth = 1 << depth;
            return x + y * sDepth + startIndecesForDepth[depth];
        }

        public int GetLeafIndex(int x, int y)
        {
            int sDepth = 1 << maxDepth;
            return x + y * sDepth;
        }

        public int GetIndex(Position p)
        {
            return GetIndex(p.x, p.y, p.depth);
        }

        public Vector2 GetLocalNodePosition(Agent a)
        {
            uint s = sizeAtDepth[maxDepth];
            return -new Vector2(a.p.x * s - (a.t.position.x + a.b.center.x) - sizeHalf, a.p.y * s - (a.t.position.y + a.b.center.y) - sizeHalf);
        }

    }

}
