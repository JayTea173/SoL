using SoL.Actors;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoL.UI;
using TMPro;
using SoL.Audio;

namespace SoL
{
    [RequireComponent(typeof(CharacterActor))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterActor actor;
        public CharacterActor Actor
        {
            get
            {
                return actor;
            }
        }
        public CanvasGroup youDiedScreen;
        public TMP_Text youDiedTextField;

        public PlayerData playerData;
        public float tilesMovePerSecond = 4f;
        public float screenNumTilesWidth = 16f;
        public float screenNumTilesHeight = 14f;
        public int numTilesFromScreenEdgeTillCameraPan = 4;


        private static PlayerController instance;

        protected Vector2 cameraTargetPosition;
        protected bool aimCameraAtTargetPosition = false;

        public void SetCameraTargetTo(Vector2 pos)
        {
            aimCameraAtTargetPosition = true;
            cameraTargetPosition = pos;
        }

        public void ResetCameraToActor()
        {
            aimCameraAtTargetPosition = false;
        }


        public static PlayerController Instance
        {
            get
            {
                return instance;
            }
        }

        void Awake()
        {
            instance = this;
            actor = GetComponent<CharacterActor>();
            var p = GetComponent<DialogPartner>();
            p.displayName = System.Environment.UserName;

            actor.onKilledBy += OnActorDeath;
        }

        protected void OnActorDeath(IDamageSource killer)
        {
            StartCoroutine(OnActorDeathCoroutine(killer));

            

        }

        protected IEnumerator OnActorDeathCoroutine(IDamageSource killer)
        {
            World.Instance.SetColorDownsampling(0f, 0f);
            World.Instance.SetUVDownsampling(0f, 0f);
            youDiedTextField.text = "YOU DIED\n";
            if (MusicPlayer.IsPlaying)
                if (!MusicPlayer.Instance.song.presistThroughDeathAnimation)
                    MusicPlayer.Instance.Stop();
            if (killer is MonoBehaviour)
            {
                var go = (killer as MonoBehaviour).gameObject;
                if (go.GetComponent<ActorDamageRelay>() != null)
                    go = go.GetComponent<ActorDamageRelay>().owningActor.gameObject;
                youDiedTextField.text += go.name + " ";
                youDiedTextField.text += killer.killMessages[Engine.RandomInt(0, killer.killMessages.Length)];
            }
            youDiedScreen.alpha = 1f;

            yield return new WaitForSeconds(4f);
            BlackMask.Instance.FadeOut(2f);
            yield return new WaitForSeconds(2f);
            youDiedScreen.alpha = 0f;
            Engine.Instance.levelManager.NewGame(Engine.Instance.levelManager.Loaded.name);

            BlackMask.Instance.FadeIn(2f);
            yield return new WaitForSeconds(1f);
            actor.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            actor.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
            yield return null;
        }

        public void ForceCenter()
        {
            Camera.main.transform.position = new Vector3(actor.Animation.spriteRenderer.transform.position.x, actor.Animation.spriteRenderer.transform.position.y, Camera.main.transform.position.z);
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (move.sqrMagnitude < 0.2f)
                move = Vector2.zero;

            if (Input.GetButtonDown("Swap Weapon"))
            {

                actor.SwapWeapons();
            }

            if (actor.CanMove)
            {

                if (!DialogUI.Instance.visible)
                {
                    if (Input.GetButtonDown("Sprint"))
                    {
                        if (!actor.sprinting && actor.AttackCharge <= 1f)
                        {
                            actor.sprinting = true;
                            actor.AttackCharge /= 2f;
                        }
                    }


                    if (Input.GetButtonUp("Sprint"))
                        actor.sprinting = false;

                    if (move.sqrMagnitude <= 0f && actor.sprinting)
                        move = actor.TransformDirection(Vector2.right);

                    //Debug.Log("Moving towards" + actor.movementDirection + actor.IsMoving);
                    actor.Move(move);
                }


                if (Input.GetButtonDown("Attack"))
                {
                    if (UI.DialogUI.Instance.visible)
                    {
                        UI.DialogUI.Instance.OnNextDown();
                    }
                    else if (actor.AttackCharge <= 1f)
                    {
                        actor.weaponCharging = true;
                        actor.Attack();
                    }
                }

                if (Input.GetButtonUp("Attack"))
                {
                    if (UI.DialogUI.Instance.visible)
                    {
                        UI.DialogUI.Instance.OnNextUp();
                    }
                    else if (actor.weaponCharging)
                    {
                        if (actor.AttackCharge > 1f)
                            actor.Attack();
                        actor.weaponCharging = false;
                    }
                }

                if (actor.weaponCharging)
                    actor.weaponCharging = Input.GetButton("Attack");



                //actor.MoveToCheckingCollision(transform.position + (Vector3)(move * Time.deltaTime * tilesMovePerSecond * (actor.sprinting ? 2f : 1f) * (actor.AttackCharge > 1f ? 0.5f : 1f)));

            }
            var cam = Camera.main;

            if (!aimCameraAtTargetPosition)
            {
                var targetPos = actor.Animation.spriteRenderer.transform.position;
                Vector3 camDeltaPos = targetPos - cam.transform.position;

                float cameraEdgeX = (screenNumTilesWidth / 2f) - numTilesFromScreenEdgeTillCameraPan;
                float cameraEdgeY = (screenNumTilesHeight / 2f) - numTilesFromScreenEdgeTillCameraPan;

                if (camDeltaPos.x < -cameraEdgeX)
                    Camera.main.transform.position = new Vector3(targetPos.x + cameraEdgeX, Camera.main.transform.position.y, Camera.main.transform.position.z);
                else if (camDeltaPos.x > cameraEdgeX)
                    Camera.main.transform.position = new Vector3(targetPos.x - cameraEdgeX, Camera.main.transform.position.y, Camera.main.transform.position.z);

                if (camDeltaPos.y < -cameraEdgeY)
                    Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, targetPos.y + cameraEdgeY, Camera.main.transform.position.z);
                else if (camDeltaPos.y > cameraEdgeY)
                    Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, targetPos.y - cameraEdgeY, Camera.main.transform.position.z);

            } else
            {
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, new Vector3(cameraTargetPosition.x, cameraTargetPosition.y, cam.transform.position.z), Time.deltaTime * 4f);
                
                
            }
        }
    }
}
