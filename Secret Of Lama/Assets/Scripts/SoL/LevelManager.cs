using SoL.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codice.Client.Common.Connection;
using SoL.Actors;
using SoL.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoL
{
    public class LevelManager : MonoBehaviour, ISerializable
    {
        public string newGameSceneName;

        protected Scene loaded;

        protected bool loading = false;

        public Scene Loaded
        {
            get
            {
                return loaded;
            }
        }

        public void Load(string name, bool fadeInAndOut = false)
        {
            if (loading)
                return;
            loading = true;
            if (DialogUI.Instance.visible)
            {
                DialogUI.Instance.visible = false;
            }

            if (fadeInAndOut)
                StartCoroutine(TransitionedLoad(name));
            else
                LoadAction(name);
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene sc, LoadSceneMode m)
        {
            if (sc.name != "Game")
            {
                loaded = sc;
                SceneManager.SetActiveScene(loaded);
            }
        }

        private void LoadAction(string name)
        {
            if (loaded != null)
                if (loaded.IsValid())
                    SceneManager.UnloadSceneAsync(loaded);

            SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            loading = false;
        }

        public IEnumerator TransitionedLoad(string name)
        {
            yield return StartCoroutine(BlackMask.Instance.FadeOutCoroutine(2f));

            LoadAction(name);

            yield return StartCoroutine(BlackMask.Instance.FadeInCoroutine(2f));

        }

        public void NewGame()
        {

            Load(newGameSceneName);
        }

        public void NewGame(string startingScene)
        {
            PlayerController.Instance.Actor.Revive();
            Load(startingScene);
        }

        public void Save()
        {
            string path = Path.Combine(Application.persistentDataPath, "save.sol");
            using (var bw = SerializationExtensions.OpenWriteToFile(path))
            {
                Serialize(bw);
            }

        }

        public void Load()
        {
            string path = Path.Combine(Application.persistentDataPath, "save.sol");
            using (var br = SerializationExtensions.OpenReadFromFile(path))
            {
                Deserialize(br);
            }

        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(SceneManager.GetActiveScene().name);
            var pc = PlayerController.Instance;
            pc.playerData.Serialize(bw);
            //pc.Actor.Serialize(bw);


            World.Instance.Serialize(bw);
            

            var allAgents = Engine.QuadTree.GetAllAgents();
            int numWritten = 0;
            var numWrittenPos = bw.BaseStream.Position;
            
            bw.Write(allAgents.Count);
            foreach (var agent in allAgents)
            {
                var actor = agent.ab.GetComponent<BaseActor>();
                if (actor != null)
                {
                    
                    var prefabRef = actor.GetComponent<IIdentifyable>();
                    if (prefabRef == null)
                    {
                        Debug.LogError(
                            $"{actor.gameObject.name} does not have a {nameof(IIdentifyable)} component. That component is required to save the actor to disk.");
                        continue;
                    }

                    bw.Write(actor.PrefabId);
                    bw.Write(actor.Guid);
                    actor.Serialize(bw);
                    
                    numWritten++;
                }

            }

            var p0 = bw.BaseStream.Position;
            bw.BaseStream.Position = numWrittenPos;
            bw.Write(numWritten);
            bw.BaseStream.Position = p0;
            
        }

        public void Deserialize(BinaryReader br)
        {
            DialogUI.Instance.visible = false;
            var sceneName = br.ReadString();

            if (!SceneManager.GetActiveScene().name.Equals(sceneName))
            {
                this.Load(sceneName);
            }
            
            var pc = PlayerController.Instance;
            pc.playerData.Deserialize(br);
            //pc.Actor.Deserialize(br);

            
            World.Instance.Deserialize(br);
            
            int numActors = br.ReadInt32();

            var existingActors = new Dictionary<string, BaseActor>();
            foreach (var agent in Engine.QuadTree.GetAllAgents())
            {
                
                var prefabRef = agent.ab.GetComponent<IIdentifyable>();
                if (prefabRef == null)
                    continue;

                var actor = agent.ab.GetComponent<BaseActor>();
                if (existingActors.ContainsKey(actor.guid))
                {
                    Debug.LogError($"Duplicate actor guid: {actor.gameObject.name} and {existingActors[actor.guid].gameObject.name}: {actor.guid} in {string.Join(", ", existingActors.Select(a => a.Value.gameObject.name))}");
                    continue;
                }
                existingActors.Add(actor.guid, actor);
                
            }

            for (int i = 0; i < numActors; i++)
            {
                ulong prefabId = br.ReadUInt64();
                string guid = br.ReadString();

                if (existingActors.TryGetValue(guid, out var actor))
                {
                    actor.Deserialize(br);
                }
                else
                {

                    if (!Engine.Instance.prefabRegistry.TryGet(prefabId, out var prefab))
                    {
                        Debug.LogError("Failed to locate prefab with id " + prefabId + " go was " + i + " / " + numActors);
                        return;
                    }

                    var inst = GameObject.Instantiate(prefab.gameObject);
                    var a0 = inst.GetComponent<BaseActor>();
                    a0.Deserialize(br);


                }
            }

            foreach (var actorHud in Engine.Instance.actorHuds)
            {
                actorHud.ForceUpdate();
            }
            

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Save();
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                Load();
            }
        }
    }
}
