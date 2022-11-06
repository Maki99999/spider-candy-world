using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance { get; private set; }

    public Default.PlayerController player;

    public Animator anim;
    public DialogueText text;
    public GameObject audioSourceOriginal;
    public int audioSourcesCount = 25;

    [Space(10)]
    public AudioClip[] audioClipsDefault;
    public AudioClip[] audioClipsPlayer;
    public AudioClip[] audioClipsDemon;

    private AudioClip[] currentAudioClips;

    private int every4thLetter = -1;
    private int currentAudioSource = -1;
    private List<AudioSource> audioSources;

    private bool isPressing = false;
    public bool isInDialogue { get; private set; } = false;
    private Transform currentCamPos = null;
    private ulong currentCamPosId = 0;

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
            text.SetText("");
            text.SetFirstInvisibleIndex(0);
            anim.SetBool("Open", false);

            if (currentCamPos != null)
            {
                Default.CameraController.instance.RemoveStaticCamPos(currentCamPos);
                Default.CameraController.instance.ResetCameraMode(currentCamPosId);
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
            text.SetText("");
            text.SetFirstInvisibleIndex(0);
            currentAudioClips = GetCorrectAudioClips(voice);
            anim.SetBool("Open", true);

            if (camPos != null)
            {
                Default.CameraController.instance.AddStaticCamPos(camPos, null, null, "Dialogue");
                currentCamPosId = Default.CameraController.instance.SetCameraMode(Default.CameraMode.STATIC);
            }
            currentCamPos = camPos;

            yield return new WaitUntil(() => (!IsPressingConfirm()));
            yield return new WaitForSeconds(0.8f);

            foreach (string sentence in texts)
            {
                yield return TypeSentence(sentence);

                yield return new WaitUntil(() => (IsPressingConfirm()));
                yield return new WaitUntil(() => (!IsPressingConfirm()));
            }

            StopDialogue();
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        text.SetText(sentence);
        bool skip = false;

        for (int i = 0; i < sentence.Length; i++)
        {
            text.SetFirstInvisibleIndex(i);

            every4thLetter = (every4thLetter + 1) % 4;
            if (!skip && every4thLetter == 0)
                PlayRandomSound();

            if (IsPressingConfirm())
                skip = true;

            if (!skip)
                yield return new WaitForSeconds(1f / 60f);
        }
    }

    void PlayRandomSound()
    {
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
}
