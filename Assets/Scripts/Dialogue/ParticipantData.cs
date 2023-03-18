using UnityEngine;
using UnityEngine.Localization;
using System.Linq;

[System.Serializable]
public class ParticipantData
{
    public string participantName;
    public Transform camPosition;
    public bool customAudio;

    public DialogueVoice audioAlt;

    public LocalizedString text;
    public LocalizedAudioClip audioClip;
    public LocalizedString audioClipTimestamps;

    public DialogueNode ToNodeTree(int textLineStart = 0, int textLineCount = int.MaxValue)
    {
        string[] lines = text.GetLocalizedString().Split("\n");
        System.Tuple<float, float>[] timestamps = null;
        if (customAudio)
            timestamps = text.GetLocalizedString().Split('\n')
                    .Select<string, System.Tuple<float, float>>(s => DialogueNode.StringToStartEnd(s)).ToArray();

        DialogueNode outputNode = null;
        DialogueNode lastNode = null;

        if (textLineStart >= 0 && textLineStart < lines.Length)
            for (int i = textLineStart; i < Mathf.Min(textLineStart + textLineCount, lines.Length); i++)
            {
                DialogueNode newNode;
                if (customAudio)
                    newNode = new DialogueNode
                    (
                        participantName,
                        lines[i],
                        camPosition,
                        (AudioClip)audioClip.LoadAsset(),
                        timestamps[i].Item1,
                        timestamps[i].Item2
                    );
                else
                    newNode = new DialogueNode
                    (
                        participantName,
                        lines[i],
                        camPosition,
                        audioAlt
                    );
                if (outputNode == null)
                    outputNode = newNode;
                else
                    lastNode.nextNode = newNode;
                lastNode = newNode;
            }

        return outputNode;
    }
}
