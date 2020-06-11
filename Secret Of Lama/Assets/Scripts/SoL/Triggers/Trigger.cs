using SoL.Actors;
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
        public bool allowNonPlayers;


        protected virtual void ExecuteAction(Collider2D collider, Collision2D collision = null)
        {
            if (action != null)
                action();

        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (cause.HasFlag(TriggerCause.Enter))
            {
                if (!allowNonPlayers)
                {
                    var a = collision.collider.GetComponent<CharacterActor>();
                    if (a == null)
                        return;
                }
                ExecuteAction(collision.collider, collision);
                if (removeAfterExecute)
                    Destroy(gameObject);
            }
        }

        public void OnCollisionExit2D(Collision2D collision)
        {
            if (cause.HasFlag(TriggerCause.Exit))
            {
                if (!allowNonPlayers)
                {
                    var a = collision.collider.GetComponent<CharacterActor>();
                    if (a == null)
                        return;
                }
                ExecuteAction(collision.collider, collision);
                if (removeAfterExecute)
                    Destroy(gameObject);
            }
        }

        public void OnCollisionStay2D(Collision2D collision)
        {
            if (cause.HasFlag(TriggerCause.Stay))
            {
                if (!allowNonPlayers)
                {
                    var a = collision.collider.GetComponent<CharacterActor>();
                    if (a == null)
                        return;
                }
                ExecuteAction(collision.collider, collision);
                if (removeAfterExecute)
                    Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (cause.HasFlag(TriggerCause.Enter))
            {
                if (!allowNonPlayers)
                {
                    var a = collision.GetComponent<CharacterActor>();
                    if (a == null)
                        return;
                }
                ExecuteAction(collision);
                if (removeAfterExecute)
                    Destroy(gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (cause.HasFlag(TriggerCause.Exit))
            {
                if (!allowNonPlayers)
                {
                    var a = collision.GetComponent<CharacterActor>();
                    if (a == null)
                        return;
                }
                ExecuteAction(collision);
                if (removeAfterExecute)
                    Destroy(gameObject);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (cause.HasFlag(TriggerCause.Stay))
            {
                if (!allowNonPlayers)
                {
                    var a = collision.GetComponent<CharacterActor>();
                    if (a == null)
                        return;
                }
                ExecuteAction(collision);
                if (removeAfterExecute)
                    Destroy(gameObject);
            }
        }
    }
}
