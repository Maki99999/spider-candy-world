using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public struct DialogueSpeaker
{
    public string speakerId;
    public string participantName;
    public Transform camPosition;
    public bool customAudio;

    public DialogueVoice audioAlt;

    public LocalizedAudioClip audioClip;
    public LocalizedString audioClipTimestamps;
}
