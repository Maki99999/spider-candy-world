using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectElement : MonoBehaviour, Useable
{
    public List<string> text;

    public void Use()
    {
        DialogueManager.instance.StartDialogueCoroutine(text, DialogueVoice.NEUTRAL);
    }
}
