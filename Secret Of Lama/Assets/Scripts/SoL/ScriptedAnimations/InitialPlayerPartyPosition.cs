using SoL.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.ScriptedAnimations
{
    public class InitialPlayerPartyPosition : MonoBehaviour
    {
        public string initialAnimation = "Idle";
        public BaseActor.Facing initialFacing;
        public bool setCameraPosition;
        public Vector2 targetCameraPosition;

        private void Start()
        {
            var a = PlayerController.Instance.Actor;
            a.transform.position = transform.position;
            PlayerController.Instance.ForceCenter();
            a.SetFacing(initialFacing);
            a.Animation.SetAnimation(initialAnimation, true);
            if (setCameraPosition)
            {
                Camera.main.transform.position = new Vector3(targetCameraPosition.x, targetCameraPosition.y, Camera.main.transform.position.z);
            }

        }
    }
}
