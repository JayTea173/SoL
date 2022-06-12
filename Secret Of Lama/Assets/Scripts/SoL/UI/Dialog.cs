using SoL.Actors;
using SoL.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace SoL.UI
{
    [CreateAssetMenu(fileName = "Dialog", menuName = "Dialog")]
    public class Dialog : ScriptableObject
    {
        [NonSerialized]
        public GameObject[] referencedSceneObjects;

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

            public void RunActions(MonoBehaviour behaviour, Dialog dlg)
            {
                if (actions != null)
                {
                    foreach (var action in actions)
                    {
                        action.Run(behaviour, dlg);
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
            public int actionIntValue, actionIntValueOther;
            public float actionFloatValue;
            public string actionStringValue;
            public UnityEngine.Object actionObjectValue;
            public float delay = 0f;
            public int actionDeliverer = 0;

            public void ExecuteAction(Dialog dlg, DialogPartner p, DialogPartner otherP)
            {
                BaseActor actor = null;
                if (p != null)
                    actor = p.GetComponent<BaseActor>();
                BaseActor other = null;
                if (otherP != null)
                    other = otherP.GetComponent<BaseActor>();


                switch (action)
                {
                    case DialogActionEnum.CHANGE_FACING:
                        Debug.Log("Setfacing of " + actor.gameObject.name + " to " + ((BaseActor.Facing)actionIntValue).ToString());
                        actor.SetFacing((BaseActor.Facing)actionIntValue);
                        break;

                    case DialogActionEnum.DESTROY_ACTOR:
                        Destroy(p.gameObject);

                        break;
                    case DialogActionEnum.HIDE_ACTOR:
                        p.gameObject.SetActive(false);

                        break;
                    case DialogActionEnum.FORCE_FACING_OTHER:
                        DialogUI.Instance.forceFacing = actionIntValue != 0;
                        break;
                    case DialogActionEnum.GAIN_WEAPON:
                        if (actor is CharacterActor)
                        {
                            
                            var ch = actor as CharacterActor;
                            var w = Items.WeaponDatabase.Instance.weapons[actionIntValue];
                            if (ch.inventory.weapons == null)
                                ch.inventory.weapons = new List<Items.Weapon>();
                            if (!ch.inventory.weapons.Contains(w))
                                ch.inventory.weapons.Add(w);
                            ch.weapon = w;
                            
                        }
                        break;
                    case DialogActionEnum.ATTACK:
                        actor.PlayAttackAnimation("Attack");
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
                        actor.Animation.SetAnimation(0, true);
                        break;
                    case DialogActionEnum.FALL:
                        actor.GetComponent<Rigidbody2D>().velocity = new Vector3(0f, -8f, 0f);
                        if (actor is CharacterActor)
                        {
                            (actor as CharacterActor).RootForSeconds(5f, false);
                        }
                        break;
                    case DialogActionEnum.CAMERA_SHAKE:
                        CameraController.Instance.Shake(actionIntValue);
                        break;
                    case DialogActionEnum.CENTER_CAMERA_ON_CONVERSATION:
                        PlayerController.Instance.SetCameraTargetTo((Vector2)(p.transform.position + otherP.transform.position) / 2f + Vector2.up * 2f);
                        break;
                    case DialogActionEnum.RESET_CAMERA_TO_PLAYER_CHARACTER:
                        PlayerController.Instance.ResetCameraToActor();
                        break;
                    case DialogActionEnum.CHANGE_OTHER_ACTOR_TEAM_ID:



                        var target = dlg.referencedSceneObjects[actionIntValue].GetComponent<IDamagable>();

                        if (target != null)
                        {

                            target.Team = (EnumTeam)actionIntValueOther;
                        }
                        else
                            Debug.LogError("Dialog tried to access referenced gameobject #" + actionIntValue + " - " + dlg.referencedSceneObjects[actionIntValue].name + ", but it didnt have an IDamagable component!");

                        break;
                    case DialogActionEnum.FORCE_NEXT_DIALOG_PAGE:
                        DialogUI.Instance.NextPage();
                        break;

                    case DialogActionEnum.CHANGE_WORLD_TILE:
                        string layerName = actionStringValue;
                        Vector2Int localPos = new Vector2Int(actionIntValue, actionIntValueOther);
                        Vector2Int pos = new Vector2Int(localPos.x + Mathf.FloorToInt(actor.transform.position.x), localPos.y + Mathf.FloorToInt(actor.transform.position.y));
                        var l = World.Instance.FindLayerByName(layerName);
                        l.SetTile(new Vector3Int(pos.x, pos.y, 0), actionObjectValue as TileBase);
                        break;
                    case DialogActionEnum.PLAY_SONG:
                        Debug.Log("Dialog play song");
                        MusicPlayer.Instance.Play(actionObjectValue as Song);
                        break;
                    case DialogActionEnum.SET_PROGRAMMER_ART_COLOR_DOWNSAMPLING_SMOOTH:
                        World.Instance.SetColorDownsampling(actionFloatValue, (actionIntValue) / 1000f);
                        break;
                    case DialogActionEnum.SET_PROGRAMMER_ART_UV_DOWNSAMPLING_SMOOTH:
                        World.Instance.SetUVDownsampling(actionFloatValue, (actionIntValue) / 1000f);
                        break;
                    case DialogActionEnum.INSTANTIATE_GAMEOBJECT:
                        var go = Instantiate(actionObjectValue as GameObject, actor.position + new Vector3(actionIntValue, actionIntValueOther, 0f), Quaternion.identity);
                        break;

                    case DialogActionEnum.SET_CG_ALPHA_BY_GAMEOBJECT_NAME:
                        var cg = GameObject.Find(actionStringValue).GetComponent<CanvasGroup>();
                        cg.alpha = actionFloatValue;
                        break;

                }
            }


            public IEnumerator RunCo(Dialog dlg, DialogPartner p, DialogPartner otherP)
            {
                if (delay > 0f)
                    yield return new WaitForSeconds(delay);
                ExecuteAction(dlg, p, otherP);
            }

            public void Run(MonoBehaviour behaviour, Dialog dlg)
            {
                if (delay > 0f)
                    behaviour.StartCoroutine(RunCo(dlg, DialogUI.Instance.GetInvolved(actionDeliverer), DialogUI.Instance.GetInvolved(actionDeliverer == 0 ? 1 : 0)));
                else
                    ExecuteAction(dlg, DialogUI.Instance.GetInvolved(actionDeliverer), DialogUI.Instance.GetInvolved(actionDeliverer == 0 ? 1 : 0));
            }


            public void Run(MonoBehaviour behaviour, DialogPartner p, DialogPartner otherP)
            {
                if (delay > 0f)
                    behaviour.StartCoroutine(RunCo(null, p, otherP));
                else
                    ExecuteAction(null, p, otherP);
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
            DESTROY_ACTOR,
            CENTER_CAMERA_ON_CONVERSATION,
            RESET_CAMERA_TO_PLAYER_CHARACTER,
            CHANGE_OTHER_ACTOR_TEAM_ID,
            FORCE_NEXT_DIALOG_PAGE,
            CHANGE_WORLD_TILE,
            PLAY_SONG,
            SET_PROGRAMMER_ART_COLOR_DOWNSAMPLING_SMOOTH,
            SET_PROGRAMMER_ART_UV_DOWNSAMPLING_SMOOTH,
            INSTANTIATE_GAMEOBJECT,
            HIDE_ACTOR,
            SET_CG_ALPHA_BY_GAMEOBJECT_NAME

        }

        public List<Page> pages;

    }
}
