using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Audio
{
    public class PlaySongTrigger : MonoBehaviour
    {
        public Song song;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            MusicPlayer.Instance.Play(song);
            Destroy(gameObject);
        }
    }
}
