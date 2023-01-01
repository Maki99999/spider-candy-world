using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public Animator anim;
    public AudioSource slideSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            SetState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            SetState(false);
    }

    public void SetState(bool open)
    {
        anim.SetBool("Open", open);
        slideSound.Play();
    }
}
