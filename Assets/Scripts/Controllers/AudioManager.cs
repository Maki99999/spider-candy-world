using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource audioSourceVoice;
    public static AudioManager instance { get; private set; }

    private void Start()
    {
        instance = this;
    }

    public void PlayOneOff(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayOneOffVocal(AudioClip clip)
    {
        if (audioSourceVoice != null && clip != null)
        {
            audioSourceVoice.clip = clip;
            audioSourceVoice.Play();
        }
    }
}
