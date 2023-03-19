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

    //[System.Obsolete]
    public void StartDialogueCoroutine(LocalizedString text, DialogueSpeaker[] speakers)
    {
        StartCoroutine(StartDialogueLegacy2(text, speakers));
    }

    //[System.Obsolete]
    public IEnumerator StartDialogueLegacy2(LocalizedString text, DialogueSpeaker[] speakers)
    {
        if (!isInDialogue)
        {
            isInDialogue = true;
            player.SetFrozen(true);
            this.text.SetFirstInvisibleIndex(0);
            anim.SetBool("Open", true);

            string[] texts = text.GetLocalizedString().TrimEnd('\n').Split('\n');
            Dictionary<string, Speaker> speakersDict = new Dictionary<string, Speaker>();
            foreach (var speaker in speakers)
                speakersDict.Add(speaker.speakerId, SpeakerToInstance(speaker));

            bool first = true;
            foreach (var dialogueLine in texts)
            {
                // split custom tags at start and text
                StringBuilder lineTags = new StringBuilder();
                bool inTag = false;
                foreach (char part in dialogueLine)
                {
                    if (!inTag && part != '<')
                        break;
                    else if (!inTag && part == '<')
                    {
                        lineTags.Append(part);
                        inTag = true;
                    }
                    else if (inTag && part == '>')
                    {
                        lineTags.Append(part);
                        inTag = false;
                    }
                    else
                        lineTags.Append(part);
                }
                string lineTrimmed = dialogueLine.Substring(lineTags.Length);
                var tags = ProcessCustomTags(lineTags.ToString());

                // get speaker
                Speaker speaker = new Speaker();
                if (tags.ContainsKey("name"))
                {
                    if (speakersDict.ContainsKey(tags["name"]))
                        speaker = speakersDict[tags["name"]];
                    else if (speakersDict.Count > 0)
                        speaker = speakersDict.Values.First();
                }
                else if (speakersDict.Count > 0)
                    speaker = speakersDict.Values.First();

                //delay on first message
                if (first)
                {
                    first = false;
                    yield return new WaitUntil(() => (!IsPressingConfirm()));
                    yield return new WaitForSeconds(0.8f);
                }

                //write
                if (speaker.audio != null)
                    yield return TypeSentence(lineTrimmed, speaker);
                else
                    yield return TypeSentence(lineTrimmed, speaker.audioAlt);

                yield return new WaitUntil(() => (IsPressingConfirm()));
                yield return new WaitUntil(() => (!IsPressingConfirm()));
            }

            StopDialogue();
        }
    }

    private Speaker SpeakerToInstance(DialogueSpeaker speaker)
    {
        List<float> starts = new List<float>();
        List<float> lengths = new List<float>();

        string[] startAndEnds = speaker.audioClipTimestamps.GetLocalizedString().Split('\n');
        System.TimeSpan startTime;
        System.TimeSpan endTime;
        foreach (string startAndEnd in startAndEnds)
        {
            string[] split = startAndEnd.Split(';');

            if (startAndEnd.Length != 2 && System.TimeSpan.TryParse(split[0], out startTime)
                    && System.TimeSpan.TryParse(split[1].Split('\n')[0], out endTime))
            {
                float start = (float)(startTime.TotalMilliseconds / 1000);
                starts.Add(start);
                lengths.Add(((float)(endTime.TotalMilliseconds / 1000)) - start);
            }
        }

        return new Speaker(speaker.participantName, speaker.camPosition, speaker.audioClip.LoadAsset(),
                starts.ToArray(), lengths.ToArray(), speaker.audioAlt);
    }

    private IEnumerator PlayAudioClipPart(AudioClip clip, float startTime, float length)
    {
        audioSourceVoice.clip = clip;
        audioSourceVoice.pitch = 1f;
        audioSourceVoice.time = startTime;
        audioSourceVoice.Play();
        yield return new WaitForSecondsRealtime(length);
        audioSourceVoice.Stop();
    }

    private Dictionary<string, string> ProcessCustomTags(string text)
    {
        Dictionary<string, string> tags = new Dictionary<string, string>();

        MatchCollection matches = Regex.Matches(text, @"<[^>]*>");

        foreach (Match match in matches)
        {
            var matchStr = match.Value;
            if (matchStr.Contains("="))
            {
                var matchSplit = matchStr.Split("=");
                tags.Add(matchSplit[0], matchSplit[1]);
            }
            else
            {
                tags.Add(matchStr, "");
            }
        }

        return tags;
    }

    //[System.Obsolete]
    public void StartDialogueCoroutine(List<string> texts, DialogueVoice voice = DialogueVoice.DEFAULT, Transform camPos = null)
    {
        StartCoroutine(StartDialogueLegacy(texts, voice, camPos));
    }

    //[System.Obsolete]
    public IEnumerator StartDialogueLegacy(List<string> texts, DialogueVoice voice = DialogueVoice.DEFAULT, Transform camPos = null)
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

    //[System.Obsolete]
    IEnumerator TypeSentence(string dialogueLine, Speaker speaker)
    {
        text.SetText(dialogueLine);
        bool confirmPressed = false;
        bool skip = false;

        Vector2 startAndLength = speaker.GetNextStartTimeAndLength();
        Coroutine audioCoroutine = StartCoroutine(PlayAudioClipPart(speaker.audio,
                startAndLength.x, startAndLength.y));

        int sentenceLength = Regex.Replace(dialogueLine, @"<.*?>", "").Length;
        float timePerChar = startAndLength.y / sentenceLength;

        for (int i = 0; i < sentenceLength; i++)
        {
            text.SetFirstInvisibleIndex(i);

            every4thLetter = (every4thLetter + 1) % 4;
            if (!skip && every4thLetter == 0)
                //PlayRandomSound(voice);

                if (IsPressingConfirm())
                    confirmPressed = true;
            if (confirmPressed && !IsPressingConfirm())
                skip = true;

            if (!skip)
                yield return new WaitForSeconds(timePerChar);
        }
        yield return new WaitUntil(() => (!IsPressingConfirm()));
        StopCoroutine(audioCoroutine);
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

    private class Speaker
    {
        public string name;
        public Transform cameraPos;
        public AudioClip audio;
        private float[] audioStartTimes;
        private float[] audioLengths;
        public DialogueVoice audioAlt;

        private int nextAudioIndex;

        public Speaker(string name, Transform cameraPos, AudioClip audio, float[] audioStartTimes, float[] audioLengths, DialogueVoice audioAlt)
        {
            this.name = name;
            this.cameraPos = cameraPos;
            this.audio = audio;
            this.audioStartTimes = audioStartTimes;
            this.audioLengths = audioLengths;
            this.audioAlt = audioAlt;
        }

        public Speaker()
        {
            this.name = "";
            this.cameraPos = null;
            this.audio = null;
            this.audioStartTimes = null;
            this.audioLengths = null;
            this.audioAlt = DialogueVoice.DEFAULT;
        }

        public Vector2 GetNextStartTimeAndLength()
        {
            Vector2 output = new Vector2(audioStartTimes[nextAudioIndex], audioLengths[nextAudioIndex]);
            nextAudioIndex++;
            return output;
        }
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
