using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance { get; private set; }

    public PlayerController player;

    public Animator anim;
    public DialogueText text;
    public DialogueText[] choiceTexts;
    public UnityEngine.EventSystems.EventSystem uiEventSystem;
    public AudioSource audioSourceVoice;
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
    private Coroutine audioVoiceCoroutine;

    private bool isPressing = false;
    public bool isInDialogue { get; private set; } = false;
    private Transform currentCamPos = null;

    private int chosenChoice = -1;

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
                currentCamPos = null;
            }

            if (audioVoiceCoroutine != null)
                StopCoroutine(audioVoiceCoroutine);

            isInDialogue = false;
        }
    }

    public void StartDialogueCoroutine(DialogueNode startNode)
    {
        StartCoroutine(StartDialogue(startNode));
    }

    public IEnumerator StartDialogue(DialogueNode startNode)
    {
        if (!isInDialogue)
        {
            isInDialogue = true;
            player.SetFrozen(true);
            text.SetFirstInvisibleIndex(0);
            anim.SetBool("Open", true);

            yield return new WaitUntil(() => (!IsPressingConfirm()));

            DialogueNode currentNode = startNode;

            bool first = true;
            while (currentNode != null)
            {
                if (currentNode.camPosition != null)
                    CameraController.instance.AddCamera(currentNode.camPosition, null, null, true);
                if (currentCamPos != null)
                    CameraController.instance.RemoveCamera(currentCamPos);
                currentCamPos = currentNode.camPosition;

                if (first)
                {
                    yield return new WaitForSeconds(0.8f);
                    first = false;
                }

                if (audioVoiceCoroutine != null)
                    StopCoroutine(audioVoiceCoroutine);

                if (currentNode.customAudio)
                    yield return TypeSentence(currentNode.text, currentNode.audioClip, currentNode.audioClipStartTime,
                            currentNode.audioClipLength);
                else
                    yield return TypeSentence(currentNode.text, currentNode.audioAlt);

                yield return new WaitUntil(() => (IsPressingConfirm()));
                yield return new WaitUntil(() => (!IsPressingConfirm()));

                if (currentNode.hasChoices)
                {
                    yield return ChoosingChoice(currentNode);
                    currentNode = currentNode.nextNodes[chosenChoice];
                }
                else
                    currentNode = currentNode.nextNode;
            }

            StopDialogue();
        }
    }

    private IEnumerator ChoosingChoice(DialogueNode currentNode)
    {
        uiEventSystem.SetSelectedGameObject(null);
        anim.SetBool("Choices", true);
        choiceTexts[0].transform.parent.parent.gameObject.SetActive(true);

        for (int i = 0; i < 4; i++)
        {
            if (currentNode.nextNodes.Length > i)
            {
                choiceTexts[i].transform.parent.gameObject.SetActive(currentNode.nextNodes.Length > i);
                choiceTexts[i].SetFirstInvisibleIndex(int.MaxValue - 1);
                yield return null;
                choiceTexts[i].SetText(currentNode.nextNodesTexts[i]);
            }
            else
            {
                choiceTexts[i].transform.parent.gameObject.SetActive(false);
            }
        }

        InputManager.instance.FreeCursor(this);
        yield return new WaitForSeconds(2f);

        chosenChoice = -1;
        yield return new WaitUntil(() => chosenChoice >= 0);
        anim.SetBool("Choices", false);
        choiceTexts[0].transform.parent.parent.gameObject.SetActive(false);
        InputManager.instance.LockCursor(this);

        for (int i = 0; i < 4; i++)
            choiceTexts[i].transform.parent.gameObject.SetActive(false);
    }

    public void ChooseChoice(int choice) => chosenChoice = choice;

    private IEnumerator PlayAudioClipPart(AudioClip clip, float startTime, float length)
    {
        audioSourceVoice.clip = clip;
        audioSourceVoice.pitch = 1f;
        audioSourceVoice.time = startTime;
        audioSourceVoice.Play();
        yield return new WaitForSecondsRealtime(length);
        audioSourceVoice.Stop();
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
                CameraController.instance.AddCamera(camPos, null, null, true);
            currentCamPos = camPos;

            yield return new WaitUntil(() => (!IsPressingConfirm()));
            yield return new WaitForSeconds(0.8f);

            foreach (string sentence in texts)
            {
                yield return TypeSentence(sentence, voice);

                yield return new WaitUntil(() => (IsPressingConfirm()));
                yield return new WaitUntil(() => (!IsPressingConfirm()));
            }

            StopDialogue();
        }
    }

    IEnumerator TypeSentence(string line, DialogueVoice voice)
    {
        text.SetText(line);
        bool confirmPressed = false;
        bool skip = false;

        int sentenceLength = Regex.Replace(line, @"<.*?>", "").Length;

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

    IEnumerator TypeSentence(string dialogueLine, AudioClip clip, float clipStart, float clipLength)
    {
        text.SetText(dialogueLine);
        bool confirmPressed = false;
        bool skip = false;

        if (audioVoiceCoroutine != null)
            StopCoroutine(audioVoiceCoroutine);
        audioVoiceCoroutine = StartCoroutine(PlayAudioClipPart(clip, clipStart, clipLength));

        int sentenceLength = Regex.Replace(dialogueLine, @"<.*?>", "").Length;
        float timePerChar = clipLength / sentenceLength;

        for (int i = 0; i < sentenceLength; i++)
        {
            text.SetFirstInvisibleIndex(i);

            if (IsPressingConfirm())
                confirmPressed = true;
            if (confirmPressed && !IsPressingConfirm())
                skip = true;

            if (!skip)
                yield return new WaitForSeconds(timePerChar);
        }
        yield return new WaitUntil(() => (!IsPressingConfirm()));
    }

    void PlayRandomSound(DialogueVoice voice)
    {
        AudioClip[] currentAudioClips = GetCorrectAudioClips(voice);
        if (currentAudioClips.Length <= 0)
            return;

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
            case DialogueVoice.NONE:
                return new AudioClip[0];
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
    NONE,
}
