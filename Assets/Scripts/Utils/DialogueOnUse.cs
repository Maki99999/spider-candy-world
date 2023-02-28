using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueOnUse : MonoBehaviour, Useable
{
    public LocalizedString dialogueText;
    public DialogueSpeaker[] speaker;

    public void Use()
    {
        DialogueManager.instance.StartDialogueCoroutine(dialogueText, speaker);
    }
}
