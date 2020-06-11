using SoL.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoL
{
    public class LevelManager : MonoBehaviour
    {
        public string newGameSceneName;

        protected Scene loaded;

        public Scene Loaded
        {
            get
            {
                return loaded;
            }
        }

        public void Load(string name, bool fadeInAndOut = false)
        {

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
                loaded = sc;
        }

        private void LoadAction(string name)
        {
            if (loaded != null)
                if (loaded.IsValid())
                    SceneManager.UnloadSceneAsync(loaded);

            SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
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
    }
}
