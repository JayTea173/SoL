using SoL.Actors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SoL.UI;

namespace SoL.ScriptedAnimations
{
    public class FallAnimation : MonoBehaviour, IDamageSource
    {
        public CharacterActor player;
        public GameObject splashPrefab;
        public Vector3 fallFrom;
        public Vector3 fallTo;
        public Vector3 speed;
        public Dialog dialogAfterFall;

        // Start is called before the first frame update
        void Start()
        {
            if (player == null)
                player = PlayerController.Instance.Actor;
            StartCoroutine(Play());

        }

        protected IEnumerator Play()
        {
            player.RootForSeconds(2f);
            player.weapon = null;
            player.Rigidbody.isKinematic = true;

            player.transform.position = fallTo;
            player.Animation.SetAnimation("Death", true);
            player.Animation.enabled = false;
            player.Animation.spriteRenderer.transform.position = fallFrom;
            var diff = (player.transform.position - player.Animation.spriteRenderer.transform.position);

            
            PlayerController.Instance.ForceCenter();
            
            while (diff.magnitude > 0.5f && diff.y < 0f) {
                player.transform.position = fallTo;

                speed += diff.normalized * Time.deltaTime * 4f;
                player.Animation.spriteRenderer.transform.Translate(speed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
                player.RootForSeconds(2f);
                diff = (player.transform.position - player.Animation.spriteRenderer.transform.position);
            }
            
            GameObject splish = Instantiate(splashPrefab, player.transform.position + Vector3.down * 0.5f, Quaternion.identity);

            player.Animation.transform.position = fallTo;
            player.Rigidbody.isKinematic = false;
            player.Damage(30, this);
            player.Animation.enabled = true;
            player.Animation.SetAnimation("Death", true);
            CameraController.Instance.Shake(18);

            yield return new WaitForEndOfFrame();

            player.RootForSeconds(4f, false);
            yield return new WaitForSeconds(4f);

            if (dialogAfterFall != null)
            {
                var dlgPartner = player.GetComponent<DialogPartner>();
                DialogUI.Instance.Display(dialogAfterFall, dlgPartner, dlgPartner);
            }

    
            yield return null;


        }

        public void OnKill(IDamagable target)
        {
            
        }
    }
}
