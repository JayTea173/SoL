using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Actors
{
    public class CreatureActor : BaseActor
    {
        [Header("Rewards")]
        [SerializeField]
        protected uint exp = 1;

        public uint ExpAward
        {
            get
            {
                return exp;
            }
        }
        [SerializeField]
        protected uint gp = 2;

        public uint GpAward
        {
            get
            {
                return gp;
            }
        }

        [SerializeField]
        protected ManaElement element;
        [SerializeField]
        protected CreatureFlags typeFlags;

        [SerializeField]
        protected int attack = 20;

        public override int GetDamageDealt()
        {
            return attack;
        }
    }

    

    [Flags]
    public enum CreatureFlags
    {
        NoStunLock = 1

    }

    [Flags]
    public enum ManaElement
    {
        Undine = 1,
        Gnome = 2,
        Sylphid = 4,
        Salamando = 8,
        Lumina = 16,
        Shade = 32,
        Luna = 64,
        Dryad = 128
    }
}
