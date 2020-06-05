using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoL.Items
{
    [Serializable]
    public class ItemStack<T> where T : ItemBase
    {
        protected T item;
        protected int count;
    }
}
