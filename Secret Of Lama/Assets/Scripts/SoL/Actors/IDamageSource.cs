﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoL.Actors
{
    public interface IDamageSource
    {
        void OnKill(IDamagable target);

        string[] killMessages
        {
            get;
        }
    }
}
