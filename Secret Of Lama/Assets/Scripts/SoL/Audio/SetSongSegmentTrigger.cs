using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoL.Audio
{
    public class SetSongSegmentTrigger : MonoBehaviour
    {
        public int targetSegment;
        public bool playToNext;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!playToNext)
                MusicPlayer.Instance.song.SetSegment(targetSegment);
            else
                MusicPlayer.Instance.bypassNextSegmentLoop = true;

            
            Destroy(gameObject);
        }
    }
}
