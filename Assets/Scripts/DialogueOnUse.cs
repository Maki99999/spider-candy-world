using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOnUse : MonoBehaviour, Default.Useable
{
    public List<string> dialogueText;
    public DialogueVoice dialogueVoice = DialogueVoice.DEFAULT;
    public Transform dialogueCameraPos;

    public void Use()
    {
        DialogueManager.instance.StartDialogueCoroutine(dialogueText, dialogueVoice, dialogueCameraPos);
    }
}
