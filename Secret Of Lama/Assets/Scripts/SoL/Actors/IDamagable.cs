using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoL.Actors
{
    public interface IDamagable
    {
        int Damage(int amount, IDamageSource damageSource);

        EnumTeam Team
        {
            get;
        }

        bool IsDead
        {
            get;
        }
    }

    public enum EnumTeam
    {
        WILD,
        PLAYER
    }

}
