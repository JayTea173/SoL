using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Actors
{
    [RequireComponent(typeof(BaseActor))]
    public class InitializeFacingBehaviour : MonoBehaviour
    {
        
        public BaseActor.Facing initialFacing;

        private void Start()
        {
            var a = GetComponent<BaseActor>();
            a.SetFacing(initialFacing);
            a.Move(Animation.SpriteAnimationBehaviour.FacingToVector(initialFacing));
            a.Move(Vector2.zero);
            Destroy(this);

        }


    }
}
