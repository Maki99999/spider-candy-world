using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    private AudioClip[] songs = new AudioClip[0];
    [SerializeField] private float maxVolume = 0.7f;
    [SerializeField] private float fadeTime = 2f;

    private bool inTransition = false;
    private int currentClipNum = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (!inTransition && !audioSource.isPlaying && songs.Length > 0)
        {
            ChangeMusic(songs, true);
        }
    }

    public void ChangeMusic(AudioClip[] newSongs, bool nextClip = false)
    {
        bool songsAreEqual = newSongs.Length == songs.Length;
        foreach (AudioClip song in newSongs)
            if (!songs.Contains(song))
            {
                songsAreEqual = false;
                break;
            }
        if (!nextClip && songsAreEqual)
            return;

        StopAllCoroutines();
        songs = newSongs;
        currentClipNum = nextClip ? (currentClipNum + 1) % songs.Length : Random.Range(0, songs.Length);
        StartCoroutine(MusicFade(fadeTime));
    }

    private IEnumerator MusicFade(float duration)
    {
        inTransition = true;
        float start = audioSource.volume;

        float rate = 1f / (duration / 2f);
        float fSmooth;
        if (audioSource.isPlaying)
        {
            for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
            {
                fSmooth = Mathf.SmoothStep(0f, 1f, f);
                audioSource.volume = Mathf.Lerp(start, 0, fSmooth);
                yield return null;
            }
        }

        if (songs.Length > 0)
        {
            audioSource.clip = songs[currentClipNum];
            audioSource.Play();

            for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
            {
                fSmooth = Mathf.SmoothStep(0f, 1f, f);
                audioSource.volume = Mathf.Lerp(0, maxVolume, fSmooth);
                yield return null;
            }
        }
        inTransition = false;
    }
}
