using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeDialogue : MonoBehaviour, Useable
{
    public GameObject playerPrison;
    public Transform lookAtPos;

    [Space(10)]
    public AudioSource audioSource;
    public AudioClip dialogueFirst;
    public AudioClip dialogueRepeated;

    [Space(10)]
    public Renderer meRenderer;
    public Material idleMaterial;
    public Material[] speakingMaterials;
    public Vector2 materialsRandomTime;

    private bool inDialogue;
    private bool firstTime = true;

    private void Start()
    {
        meRenderer.material = idleMaterial;
    }

    public void Use()
    {
        if (!isActiveAndEnabled || inDialogue || dialogueFirst == null)
            return;

        StartCoroutine(TheDialogue());
    }

    IEnumerator TheDialogue()
    {
        inDialogue = true;
        PlayerController.instance.focusedObject = lookAtPos;
        playerPrison.SetActive(true);

        if (firstTime)
        {
            audioSource.clip = dialogueFirst;

            if (dialogueRepeated == null)
                GetComponent<Collider>().enabled = false;
        }
        else
            audioSource.clip = dialogueRepeated;

        audioSource.Play();

        yield return DialogueWait();

        firstTime = false;
        PlayerController.instance.focusedObject = null;
        playerPrison.SetActive(false);
        meRenderer.material = idleMaterial;
        inDialogue = false;
    }

    public IEnumerator DialogueWait()
    {
        while (audioSource.isPlaying && !Input.GetKey(KeyCode.RightControl))
        {
            meRenderer.material = speakingMaterials[Random.Range(0, speakingMaterials.Length)];
            yield return new WaitForSeconds(Random.Range(materialsRandomTime.x, materialsRandomTime.y));
        }
        meRenderer.material = idleMaterial;
    }
}
