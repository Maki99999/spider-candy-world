using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance { get; private set; }

    public PlayerController player;

    public Animator anim;
    public DialogueText text;
    public GameObject audioSourceOriginal;
    public int audioSourcesCount = 25;

    [Space(10)]
    public AudioClip[] audioClipsDefault;
    public AudioClip[] audioClipsPlayer;
    public AudioClip[] audioClipsDemon;
    public AudioClip[] audioClipsNeutral;

    private int every4thLetter = -1;
    private int currentAudioSource = -1;
    private List<AudioSource> audioSources;

    private bool isPressing = false;
    public bool isInDialogue { get; private set; } = false;
    private Transform currentCamPos = null;

    private void Start()
    {
        instance = this;

        audioSources = new List<AudioSource>();
        audioSources.Add(audioSourceOriginal.GetComponent<AudioSource>());
        for (int i = 0; i < audioSourcesCount - 1; i++)
            audioSources.Add(Instantiate(audioSourceOriginal, audioSourceOriginal.transform.position,
                audioSourceOriginal.transform.rotation, transform).GetComponent<AudioSource>());
    }

    public void StopDialogue()
    {
        if (isInDialogue)
        {
            player.SetFrozen(false);
            text.HideText();
            text.SetFirstInvisibleIndex(0);
            anim.SetBool("Open", false);

            if (currentCamPos != null)
            {
                CameraController.instance.RemoveCamera(currentCamPos);
            }

            isInDialogue = false;
        }
    }

    public void StartDialogueCoroutine(List<string> texts, DialogueVoice voice = DialogueVoice.DEFAULT, Transform camPos = null)
    {
        StartCoroutine(StartDialogue(texts, voice, camPos));
    }

    public IEnumerator StartDialogue(List<string> texts, DialogueVoice voice = DialogueVoice.DEFAULT, Transform camPos = null)
    {
        if (!isInDialogue)
        {
            isInDialogue = true;
            player.SetFrozen(true);
            text.SetFirstInvisibleIndex(0);
            anim.SetBool("Open", true);

            if (camPos != null)
                CameraController.instance.AddCamera(camPos);
            currentCamPos = camPos;

            yield return new WaitUntil(() => (!IsPressingConfirm()));
            yield return new WaitForSeconds(0.8f);

            foreach (string sentence in texts)
            {
                yield return TypeSentence(voice, sentence);

                yield return new WaitUntil(() => (IsPressingConfirm()));
                yield return new WaitUntil(() => (!IsPressingConfirm()));
            }

            StopDialogue();
        }
    }

    IEnumerator TypeSentence(DialogueVoice voice, string sentence)
    {
        text.SetText(sentence);
        bool confirmPressed = false;
        bool skip = false;

        int sentenceLength = Regex.Replace(sentence, "<.*?>", "").Length;

        for (int i = 0; i < sentenceLength; i++)
        {
            text.SetFirstInvisibleIndex(i);

            every4thLetter = (every4thLetter + 1) % 4;
            if (!skip && every4thLetter == 0)
                PlayRandomSound(voice);

            if (IsPressingConfirm())
                confirmPressed = true;
            if (confirmPressed && !IsPressingConfirm())
                skip = true;

            if (!skip)
                yield return new WaitForSeconds(1f / 60f);
        }
        yield return new WaitUntil(() => (!IsPressingConfirm()));
    }

    void PlayRandomSound(DialogueVoice voice)
    {
        AudioClip[] currentAudioClips = GetCorrectAudioClips(voice);

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                currentAudioSource = i;
                break;
            }
            if (i == audioSources.Count - 1)
                currentAudioSource = (currentAudioSource + 1) % audioSources.Count;

        }
        AudioSource audioSource = audioSources[currentAudioSource];

        if (!audioSource.isPlaying)
        {
            if (voice == DialogueVoice.NEUTRAL)
                audioSource.pitch = 1f;
            else
                audioSource.pitch = 1f + Random.Range(-.2f, .2f);

            audioSource.clip = currentAudioClips[Random.Range(0, currentAudioClips.Length)];
            audioSource.Play();
        }
    }

    AudioClip[] GetCorrectAudioClips(DialogueVoice voice)
    {
        switch (voice)
        {
            case DialogueVoice.DEFAULT:
                return audioClipsDefault;
            case DialogueVoice.PLAYER:
                return audioClipsPlayer;
            case DialogueVoice.DEMON:
                return audioClipsDemon;
            case DialogueVoice.NEUTRAL:
                return audioClipsNeutral;
            default:
                return audioClipsDefault;
        }
    }

    bool IsPressingConfirm()
    {
        if (InputManager.PressingUse())
            return true;

        if (InputManager.PressingConfirm() && !isPressing)
        {
            isPressing = true;
            return true;
        }
        if (!InputManager.PressingConfirm() && isPressing)
            isPressing = false;

        return false;
    }
}

public enum DialogueVoice
{
    DEFAULT,
    PLAYER,
    DEMON,
    NEUTRAL,
}
