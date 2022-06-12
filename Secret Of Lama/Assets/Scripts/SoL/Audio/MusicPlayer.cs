using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        public Song song;

        private static MusicPlayer instance;

        public bool bypassNextSegmentLoop;

        private float volume;
        public static MusicPlayer Instance
        {
            get
            {
                return instance;
            }
        }

        private AudioSource src;
        public AudioSource AudioSource
        {
            get
            {
                return src;
            }
        }
        private void Awake()
        {
            instance = this;
            src = GetComponent<AudioSource>();
            volume = src.volume;
            if (song != null)
                Play(song);
        }

        public void FadeOut(float time)
        {
            StartCoroutine(FadeOutCoroutine(time));
        }

        private IEnumerator FadeOutCoroutine(float time)
        {
            float v = src.volume;
            float t = 0f;
            while (t < time)
            {
                yield return new WaitForEndOfFrame();
                src.volume = v * (1f - t / time);
                t += Time.deltaTime;
            }
            src.volume = 0f;
            src.Stop();
            src.volume = v;
        }

        public static bool IsPlaying
        {
            get
            {
                return Instance.src.isPlaying;
            }
        }

        public void FadeIn(float time)
        {
            StartCoroutine(FadeInCoroutine(time));
        }

        private IEnumerator FadeInCoroutine(float time)
        {
            src.volume = 0f;
            float t = 0f;
            while (t < time)
            {
                yield return new WaitForEndOfFrame();
                src.volume = volume * (t / time);
                t += Time.deltaTime;
            }
            src.volume = volume;
        }

        public void Play(Song s)
        {
            if (s != null)
                Debug.Log("playing " + s.name);
            else
                Debug.Log("Stopping song");
            song = s;
            StartCoroutine(PlayCoroutine());

        }

        private IEnumerator PlayCoroutine()
        {
            if (IsPlaying)
            {
                FadeOut(2f);
                yield return new WaitForSeconds(2f);
            }
            src.volume = volume;
            if (song != null)
            {
                song.Play(src);
            }
        }

        public void Stop()
        {
            FadeOut(2f);
        }

        private void Update()
        {
            if (song != null)
                song.UpdateSegmentedPlay(src);
        }

    }
}
