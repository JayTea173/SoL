using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Actors
{
    public class ProjectileActor : BaseActor
    {
        public int damageDealt;


        public override int Damage(int amount, IDamageSource damageSource)
        {
            return 0;
        }

        public override int GetDamageDealt()
        {
            return damageDealt;
        }

        public override bool CanMove
        {
            get
            {
                return true;
            }
        }


        protected override void Initialize()
        {
            base.Initialize();
            Animation.SetAnimation("Idle", true);
            
        }

        public override Vector2 TransformDirection(Vector2 originOffset)
        {
            return originOffset;
        }


        private void Start()
        {
            Move(Vector2.right);
            SetFacing(Facing.Right);
            PlayAttackAnimation("Attack");
        }
    }
}
