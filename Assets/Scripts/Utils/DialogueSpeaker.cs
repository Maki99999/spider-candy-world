using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public struct DialogueSpeaker
{
    public string speakerId;
    public string speakerName;
    public Transform speakerCameraPos;
    public LocalizedAudioClip speakerAudio;
    public LocalizedString speakerAudioTiming;
    public DialogueVoice speakerAudioAlt;
}
