using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    public string participantName;
    public string text;
    public Transform camPosition;
    public bool customAudio;
    public bool hasChoices;

    public DialogueVoice audioAlt;

    public AudioClip audioClip;
    public float audioClipStartTime;
    public float audioClipLength;

    private DialogueNode _nextNode;
    public DialogueNode nextNode
    {
        get { return _nextNode; }
        set
        {
            hasChoices = false;
            _nextNode = value;
        }
    }

    public DialogueNode[] nextNodes { get; private set; }
    public string[] nextNodesTexts { get; private set; }

    public DialogueNode(string participantName, string text, Transform camPosition, AudioClip audioClip,
            float audioClipStartTime, float audioClipLength)
    {
        this.participantName = participantName;
        this.text = text;
        this.camPosition = camPosition;
        this.customAudio = true;
        this.hasChoices = false;
        this.audioClip = audioClip;
        this.audioClipStartTime = audioClipStartTime;
        this.audioClipLength = audioClipLength;
        this.nextNodes = new DialogueNode[0];
        this.nextNodesTexts = new string[0];
    }

    public DialogueNode(string participantName, string text, Transform camPosition, DialogueVoice voice)
    {
        this.participantName = participantName;
        this.text = text;
        this.camPosition = camPosition;
        this.customAudio = false;
        this.hasChoices = false;
        this.audioAlt = voice;
        this.nextNodes = new DialogueNode[0];
        this.nextNodesTexts = new string[0];
    }

    public void AddChoice(DialogueNode node, string text)
    {
        hasChoices = true;
        nextNodes = nextNodes.Concat(new DialogueNode[] { node }).ToArray();
        nextNodesTexts = nextNodesTexts.Concat(new string[] { text }).ToArray();
    }

    public static System.Tuple<float, float> StringToStartEnd(string s)
    {
        string[] split = s.Split(';');
        System.TimeSpan startTime;
        System.TimeSpan endTime;

        if (split.Length == 2 && System.TimeSpan.TryParse(split[0], out startTime)
                && System.TimeSpan.TryParse(split[1], out endTime))
        {
            float start = (float)(startTime.TotalMilliseconds / 1000);
            return new System.Tuple<float, float>(start, ((float)(endTime.TotalMilliseconds / 1000)) - start);
        }
        return new System.Tuple<float, float>(0, 0);
    }
}
