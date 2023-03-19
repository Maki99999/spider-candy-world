using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOnUse : MonoBehaviour, Useable
{
    public ParticipantData participant;
    public int textLineStart;
    public int textLineCount;

    public void Use()
    {
        DialogueManager.instance.StartDialogueCoroutine(participant.ToNodeGraph(textLineStart, textLineCount));
    }
}
