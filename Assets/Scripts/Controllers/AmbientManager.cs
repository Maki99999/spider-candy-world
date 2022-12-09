using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientManager : MonoBehaviour
{
    public static AmbientManager instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;
    private Vector2 secondsBtwnAmbientClips = new Vector2(1000, 1000);

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        StartCoroutine(RandomSound());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void ChangeAmbientClips(AudioClip[] clips, Vector2 secondsBtwnAmbientClips)
    {
        StopAllCoroutines();
        this.clips = clips;
        this.secondsBtwnAmbientClips = secondsBtwnAmbientClips;
        StartCoroutine(RandomSound());
    }

    public void ChangeAmbientColor(Color color)
    {
        RenderSettings.ambientLight = color;
    }

    private IEnumerator RandomSound()
    {
        while (clips.Length > 0 && isActiveAndEnabled)
        {
            yield return new WaitForSeconds(Random.Range(secondsBtwnAmbientClips.x, secondsBtwnAmbientClips.y));
            audioSource.clip = clips[Random.Range(0, clips.Length)];
            audioSource.Play();
        }
    }
}
