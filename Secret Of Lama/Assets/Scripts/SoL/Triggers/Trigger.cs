using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Triggers
{
    public class Trigger : MonoBehaviour
    {
        [Flags]
        public enum TriggerCause
        {
            Enter = 1,
            Exit = 2,
            Stay = 4
        }

        public TriggerCause cause;
        public UnityEngine.Events.UnityAction action;
        public bool removeAfterExecute;


        protected virtual void ExecuteAction(Collision2D collision)
        {
            if (action != null)
                action();

        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (cause.HasFlag(TriggerCause.Enter))
            {
                ExecuteAction(collision);
                if (removeAfterExecute)
                    Destroy(gameObject);
            }
        }

        public void OnCollisionExit2D(Collision2D collision)
        {
            if (cause.HasFlag(TriggerCause.Exit))
            {
                ExecuteAction(collision);
                if (removeAfterExecute)
                    Destroy(gameObject);
            }
        }

        public void OnCollisionStay2D(Collision2D collision)
        {
            if (cause.HasFlag(TriggerCause.Stay))
            {
                ExecuteAction(collision);
                if (removeAfterExecute)
                    Destroy(gameObject);
            }
        }
    }
}
