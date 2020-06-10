using SoL.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SoL.UI
{
    [CreateAssetMenu(fileName = "Dialog", menuName = "Dialog")]
    public class Dialog : ScriptableObject
    {
        [Serializable]
        public class Page
        {
            public string text;
            [Tooltip("0 is the one talked to, 1 is player char")]
            public int delivererId = 0;

            protected string processedText;
            public string ProcessedText
            {
                get
                {
                    return processedText;
                }
            }

            public List<DialogAction> actions;

            public void RunActions(MonoBehaviour behaviour)
            {
                if (actions != null)
                {
                    foreach (var action in actions)
                    {
                        behaviour.StartCoroutine(action.Run());
                    }
                }

            }

            private static readonly char[] varSeperators = new char[]
            {
                ' ',
                '.',
                ',',
                '!',
                '?',
                ':',
                ';'
            };

            public void PostProcess(params DialogPartner[] partners)
            {
                processedText = text;


                int i = 0;
                int lasti = 0;
                StringBuilder sb = new StringBuilder();
                while ((i = processedText.IndexOf("$", i)) >= 0)
                {
                    sb.Append(processedText.Substring(lasti, i - lasti));

                    int indexEnd = processedText.IndexOfAny(varSeperators, i);
                    string varName = processedText.Substring(i + 1, indexEnd - i - 1).ToLower();
                    char last = varName[varName.Length - 1];
                    int arrIndex = 0;
                    if (char.IsNumber(last))
                    {
                        int.TryParse(new string(last, 1), out arrIndex);
                        varName = varName.Substring(0, varName.Length - 1);
                    }

                    
                    if (varName == "name")
                    {
                        sb.Append(partners[arrIndex].displayName);
                    }

                    
                    i = indexEnd;
                    lasti = i;
                }

                sb.Append(processedText.Substring(lasti));

                processedText = sb.ToString();
               
            }


        }

        [Serializable]
        public class DialogAction
        {
            public DialogActionEnum action;
            public int actionIntValue;
            public string actionStringValue;
            public UnityEngine.Object actionObjectValue;
            public float delay = 0f;
            public int actionDeliverer = 0;


            public IEnumerator Run()
            {

                var p = DialogUI.Instance.GetInvolved(actionDeliverer);
                var actor = p.GetComponent<BaseActor>();
                var otherP = DialogUI.Instance.GetInvolved(actionDeliverer == 0 ? 1 : 0);
                BaseActor other = null;
                if (otherP != null)
                    other = otherP.GetComponent<BaseActor>();

                if (delay > 0f)
                    yield return new WaitForSeconds(delay);


                switch (action)
                {
                    case DialogActionEnum.CHANGE_FACING:
                        Debug.Log("Setfacing of " + actor.gameObject.name + " to " + ((BaseActor.Facing)actionIntValue).ToString());
                        actor.SetFacing((BaseActor.Facing)actionIntValue);
                        break;
                    case DialogActionEnum.FORCE_FACING_OTHER:
                        DialogUI.Instance.forceFacing = actionIntValue != 0;
                        break;
                    case DialogActionEnum.GAIN_WEAPON:
                        if (actor is CharacterActor)
                        {
                            var ch = actor as CharacterActor;
                            ch.weapon = Items.WeaponDatabase.Instance.weapons[actionIntValue];
                        }
                        break;
                    case DialogActionEnum.ATTACK:
                        actor.PlayAttackAnimation();
                        break;

                    case DialogActionEnum.SET_SPRITE:
                        actor.Animation.spriteRenderer.sprite = actionObjectValue as Sprite;
                        actor.Animation.spriteRenderer.flipX = actionIntValue != 0;

                        break;
                    case DialogActionEnum.PLAY_ANIMATION:
                        actor.Animation.SetAnimation(actionObjectValue as Animation.AnimationCollection, actionIntValue, true);
                        break;
                    case DialogActionEnum.MOVE_TOWARDS_PARTNER:
                        var diff = (other.transform.position - actor.transform.position);
                        actor.Move(((Vector2)diff).normalized);
                        break;
                    case DialogActionEnum.STOP_MOVING:
                        actor.Move(Vector2.zero);
                        break;
                    case DialogActionEnum.FALL:
                        actor.GetComponent<Rigidbody2D>().velocity = new Vector3(0f, -8f, 0f);
                        break;
                    case DialogActionEnum.CAMERA_SHAKE:
                        CameraController.Instance.Shake(actionIntValue);
                        break;
                    case DialogActionEnum.ROOT_ACTOR:
                        if (actor is CharacterActor)
                            (actor as CharacterActor).RootForSeconds((actionIntValue) / 60f);
                        break;
                    
                    
                }

            }
  
        }

        public enum DialogActionEnum
        {
            NOTHING,
            CHANGE_FACING,
            FORCE_FACING_OTHER,
            GAIN_WEAPON,
            ATTACK,
            SET_SPRITE,
            PLAY_ANIMATION,
            MOVE_TOWARDS_PARTNER,
            STOP_MOVING,
            FALL,
            CAMERA_SHAKE,
            ROOT_ACTOR
        }

        public bool repeatable;
        public List<Page> pages;


    }
}
