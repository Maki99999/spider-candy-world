using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Default
{
    public class SoundOnUse : MonoBehaviour, Useable
    {
        new public AudioSource audio;

        public void Use()
        {
            audio.Play();
        }
    }
}
