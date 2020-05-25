using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Actors
{
    public class BaseActor : MonoBehaviour, IDamagable, IDamageSource
    {
        [SerializeField]
        protected int hp;

        [SerializeField]
        protected int hpMax;

        protected virtual bool IsDead
        {
            get
            {
                return hp <= 0;
            }
        }

        public int Damage(int amount, IDamageSource damageSource)
        {
            if (amount > hpMax)
                amount = hpMax;
            else if (amount > hp)
                amount = hp;

            hp -= amount;
            return amount;
        }

        public void SortZ()
        {
             transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.01f);
        }

        private void Update()
        {
            SortZ();
        }
    }
}
