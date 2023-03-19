using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, Useable
{
    public Animator doorAnim;
    public AudioSource doorAudio;
    public AudioClip sfxOpen;
    public AudioClip sfxClose;
    public AudioClip sfxRattle;

    public bool currentlyOpen = false;

    public State lockedMode = State.UNLOCKED;
    public List<string> notOpeningText;

    private void Start()
    {
        if (doorAnim != null)
        {
            doorAnim.SetBool("Open", currentlyOpen);
        }
    }

    void Useable.Use()
    {
        if (lockedMode != State.UNLOCKED)
        {
            StartCoroutine(LockedDialogue(lockedMode));
            return;
        }
        else if (currentlyOpen)
            Close();
        else
            Open();
    }

    private IEnumerator LockedDialogue(State lockedMode)
    {
        if (lockedMode == State.LOCKED)
        {
            doorAnim.SetTrigger("Rattle");
            doorAudio.clip = sfxRattle;
            doorAudio.Play();
        }
        else if (lockedMode == State.UNINTERESTING)
        {
            doorAnim.SetBool("Peek", true);
            doorAudio.clip = sfxOpen;
            doorAudio.Play();
        }

        if (notOpeningText.Count > 0)
        {
            PlayerController.instance.SetFrozen(true);
            yield return DialogueManager.instance.StartDialogue(notOpeningText, DialogueVoice.NEUTRAL);
            PlayerController.instance.SetFrozen(false);
        }

        if (lockedMode == State.UNINTERESTING)
        {
            doorAnim.SetBool("Peek", false);
            doorAudio.clip = sfxClose;
            doorAudio.PlayDelayed(0.2f);
        }
    }

    public void Open()
    {
        if (currentlyOpen)
            return;
        currentlyOpen = true;
        if (doorAnim != null)
            doorAnim.SetBool("Open", true);

        doorAudio.clip = sfxOpen;
        doorAudio.Play();
    }

    public void Close()
    {
        if (!currentlyOpen)
            return;
        currentlyOpen = false;
        if (doorAnim != null)
            doorAnim.SetBool("Open", false);

        doorAudio.clip = sfxClose;
        doorAudio.PlayDelayed(0.5f);
    }

    [System.Serializable]
    public enum State
    {
        UNLOCKED,
        LOCKED,
        UNINTERESTING
    }
}
