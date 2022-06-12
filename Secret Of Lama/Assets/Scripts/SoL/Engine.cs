using SoL.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using SoL.Actors;
using SoL.Items;
using SoL.Serialization;
using SoL.UI;
using UnityEngine.SceneManagement;

namespace SoL
{
    public class Engine : SerializedMonoBehaviour
    {
        public static readonly float pixelsPerUnit = 16f;

        private const string gameObjectName = "Engine";
        private static Engine instance;
        private static System.Random r;

        public static int actorLayer;
        public static int airborneActorLayer;

        public LevelManager levelManager;
        public PrefabRegistry<BaseActor> prefabRegistry;
        public PrefabRegistry<ItemBase> itemRegistry;

        public List<ActorHUD> actorHuds;

        private void Awake()
        {
            actorLayer = LayerMask.NameToLayer("Actor");
            airborneActorLayer = LayerMask.NameToLayer("AirborneActor");

            if (SceneManager.sceneCount <= 1)
                levelManager.NewGame();

            DontDestroyOnLoad(gameObject);

            instance = this;
            r = new System.Random();
            qt = new QuadTree(256, 16);
        }

        public static Engine Instance
        {
            get
            {
                return instance;
            }
        }

        private QuadTree qt;
        public static QuadTree QuadTree
        {
            get
            {
                return instance.qt;
            }
        }

        public static Canvas MainCanvas
        {
            get
            {
                return StaticData.Instance.mainCanvas;
            }
        }


        public static Vector3 AlignToPixelGrid(Vector3 v)
        {

            Vector2Int pixelPosition = new Vector2Int(Mathf.FloorToInt(v.x * pixelsPerUnit), Mathf.FloorToInt(v.y * pixelsPerUnit));

            return new Vector3(pixelPosition.x / Engine.pixelsPerUnit, pixelPosition.y / Engine.pixelsPerUnit, v.z);
        }

        #region Random
        public static double Gaussian(double center = 0d, double pioffset = 1d)
        {
            return NextGaussian() * pioffset + center;
        }

        public static float Gaussian(float center = 0f, float pioffset = 1f)
        {
            return (float)NextGaussian() * pioffset + center;
        }
        //this will go -pi to +pi with center at 0
        private static double NextGaussian(double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var rand_std_normal = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) *
                                System.Math.Sin(2.0 * System.Math.PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }

        public static float RandomFloat(float min = 0f, float max = 1f)
        {
            return (float)(r.NextDouble() * (max - min)) + min;
        }

        public static int RandomInt(int min = int.MinValue, int max = int.MaxValue)
        {
            return r.Next(min, max);
        }

        public static double RandomDouble()
        {
            return r.NextDouble();
        }
        #endregion
    }
}
