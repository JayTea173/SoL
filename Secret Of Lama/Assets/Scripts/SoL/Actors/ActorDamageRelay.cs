using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Actors
{
    public class ActorDamageRelay : MonoBehaviour, IDamagable
    {
        public BaseActor owningActor;

        public bool IsDead
        {
            get
            {
                return owningActor.IsDead;
            }
        }

        public EnumTeam Team
        {
            get
            {
                return owningActor.Team;
            }
            set
            {
                owningActor.Team = value;
            }
        }

        public int Damage(int amount, IDamageSource damageSource)
        {
            return owningActor.Damage(amount, damageSource);
        }
    }
}
