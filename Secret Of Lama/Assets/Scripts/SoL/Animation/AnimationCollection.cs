using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Animation
{
    [CreateAssetMenu(fileName = "AnimationCollection", menuName = "Animation/Collection")]
    public class AnimationCollection : ScriptableObject, IEnumerable
    {
        [SerializeField]
        public SpriteAnimation[] animations;


        public IEnumerator GetEnumerator()
        {
            return animations.GetEnumerator();
        }

        public int Count
        {
            get
            {
                if (animations == null)
                    return 0;
                return animations.Length;
            }
            
        }

        public SpriteAnimation this[int index]
        {
            get
            {
                return animations[index];
            }
            set
            {
                animations[index] = value;
            }
        }

        public int GetIndexByName(string animationName)
        {
            int hc = animationName.GetHashCode();
            int c = animations.Length;

            for (int i = 0; i < c; i++)
            {
                if (animations[i].name.GetHashCode() == hc)
                    return i;
            }
            return -1;
        }

    }
}
