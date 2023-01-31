using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientManager : MonoBehaviour
{
    public static AmbientManager instance { get; private set; }

    public Color defaultColor;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;
    private Vector2 secondsBtwnAmbientClips = new Vector2(1000, 1000);

    private Coroutine ambientColorCoroutine;

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

    public void ChangeAmbientColor(Color color, bool instant = false)
    {
        if (ambientColorCoroutine != null)
            StopCoroutine(ambientColorCoroutine);

        if (instant)
            RenderSettings.ambientLight = color;
        else
            ambientColorCoroutine = StartCoroutine(AmbientColorTransition(color));
    }

    private IEnumerator AmbientColorTransition(Color color)
    {
        Color oldColor = RenderSettings.ambientLight;

        float rate = 1f / 0.8f;
        float fSmooth;
        for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
        {
            fSmooth = 1 - Mathf.Pow(1 - f, 3);
            RenderSettings.ambientLight = Color.Lerp(oldColor, color, f);

            yield return null;
        }
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
