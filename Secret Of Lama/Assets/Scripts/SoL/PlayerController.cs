﻿using SoL.Actors;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoL
{
    [RequireComponent(typeof(CharacterActor))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterActor actor;
        public PlayerData playerData;
        public float tilesMovePerSecond = 4f;
        public float screenNumTilesWidth = 16f;
        public float screenNumTilesHeight = 14f;
        public int numTilesFromScreenEdgeTillCameraPan = 4;


        private static PlayerController instance;

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
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (actor.CanMove)
            {
                if (Input.GetButtonDown("Sprint"))
                {
                    if (!actor.sprinting && actor.AttackCharge <= 1f)
                    {
                        Debug.Log("SPRINT START");
                        actor.sprinting = true;
                        actor.AttackCharge /= 2f;
                    }
                }


                if (Input.GetButtonUp("Sprint"))
                    actor.sprinting = false;

                if (move.sqrMagnitude <= 0f && actor.sprinting)
                    move = actor.TransformDirection(Vector2.right);

                actor.Move(move);

                if (Input.GetButtonDown("Attack"))
                {
                    if (actor.AttackCharge <= 1f)
                    {
                        actor.weaponCharging = true;
                        actor.Attack();
                    }
                }

                if (Input.GetButtonUp("Attack"))
                {
                    if (actor.weaponCharging)
                    {
                        if (actor.AttackCharge > 1f)
                            actor.Attack();
                        actor.weaponCharging = false;
                    }
                }

                if (actor.weaponCharging)
                    actor.weaponCharging = Input.GetButton("Attack");



                actor.MoveToCheckingCollision(transform.position + (Vector3)(move * Time.deltaTime * tilesMovePerSecond * (actor.sprinting ? 2f : 1f) * (actor.AttackCharge > 1f ? 0.5f : 1f)));

            }
            var cam = Camera.main;

            Vector3 camDeltaPos = actor.transform.position - cam.transform.position;

            float cameraEdgeX = (screenNumTilesWidth / 2f) - numTilesFromScreenEdgeTillCameraPan;
            float cameraEdgeY = (screenNumTilesHeight / 2f) - numTilesFromScreenEdgeTillCameraPan;

            if (camDeltaPos.x < -cameraEdgeX)
                Camera.main.transform.position = new Vector3(transform.position.x + cameraEdgeX, Camera.main.transform.position.y, Camera.main.transform.position.z);
            else if (camDeltaPos.x > cameraEdgeX)
                Camera.main.transform.position = new Vector3(transform.position.x - cameraEdgeX, Camera.main.transform.position.y, Camera.main.transform.position.z);

            if (camDeltaPos.y < -cameraEdgeY)
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y + cameraEdgeY, Camera.main.transform.position.z);
            else if (camDeltaPos.y > cameraEdgeY)
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y - cameraEdgeY, Camera.main.transform.position.z);


        }
    }
}
