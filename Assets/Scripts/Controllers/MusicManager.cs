using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] songs;

    void Start()
    {
        StartCoroutine(StartMusic());
    }

    IEnumerator StartMusic()
    {
        audioSource.clip = songs[Random.Range(0, songs.Length)];
        audioSource.Play();

        yield return new WaitWhile(() => audioSource.isPlaying);
        StartCoroutine(StartMusic());
    }
}
