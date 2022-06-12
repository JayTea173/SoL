using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Pathing
{
    [Serializable]
    public class Path
    {
        public Vector3[] positions;
        public int currentTarget;

        public Vector3 Target
        {
            get
            {
                int c = positions.Length;
                if (c < 1)
                    return Vector3.zero;
                return positions[c - 1];
            }
        }

        public Vector3 GetDirection(Transform t)
        {
            return (positions[currentTarget] - t.position).normalized;
        }

        public Vector3 GetCurrentTargetPosition()
        {
            if (currentTarget >= 0 && currentTarget < positions.Length)
                return positions[currentTarget];
            else if (positions.Count() <= 0)
                return Vector3.zero;
            else
                return positions[0];
        }

        public void DebugDraw()
        {
            int c = positions.Length;
            for (int i = 0; i < c; i++)
            {
                Color col;
                if (i < currentTarget)
                    col = new Color(0f, 1f, 0f);
                else if (i == currentTarget)
                    col = new Color(1f, 1f, 0f);
                else
                    col = new Color(1f, 0f, 0f);
                Debug.DrawLine(positions[i], positions[i] + Vector3.up * 0.5f, col);
            }
               
        }

        public bool Update(Vector3 position, float pathingTargetLeeway)
        {
            Vector3 diff = GetCurrentTargetPosition() - position;
            if (Mathf.Abs(diff.x) < 0.5f && Mathf.Abs(diff.y) < pathingTargetLeeway)
            {
                int c = positions.Length;
                currentTarget++;
                if (currentTarget >= c)
                    return true;
            }

            return false;
        }
    }
}
