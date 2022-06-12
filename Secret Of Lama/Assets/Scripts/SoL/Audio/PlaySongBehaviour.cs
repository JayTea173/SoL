using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Audio
{
    public class PlaySongBehaviour : MonoBehaviour
    {
        public Song song;
        public bool dontReplayOnLoad;
        public float delay;

        private void Start()
        {
            StartCoroutine(PlayDelayed());
        }

        private IEnumerator PlayDelayed()
        {
            yield return new WaitForSeconds(delay);
            if (dontReplayOnLoad && MusicPlayer.Instance.song == song)
            {

            }
            else if (MusicPlayer.Instance.song != song)
                MusicPlayer.Instance.Play(song);
            else if (!MusicPlayer.IsPlaying)
                MusicPlayer.Instance.Play(song);
        }
    }
}
