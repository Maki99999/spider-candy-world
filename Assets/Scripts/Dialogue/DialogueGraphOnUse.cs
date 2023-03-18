using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueGraphOnUse : MonoBehaviour, Useable
{
    public DialogueGraphWindow dialogueGraph;

    public void Use()
    {
        DialogueManager.instance.StartDialogueCoroutine(dialogueGraph.Generate());
    }
}
