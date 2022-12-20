using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoOpenDoor : MonoBehaviour
{
    [SerializeField] private string colliderTag = "Player";
    [SerializeField] private Animator animator;
    [SerializeField] private string animationBoolName = "Open";
    [SerializeField] private AudioSource openSfx;
    [SerializeField] private AudioSource closeSfx;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(colliderTag))
        {
            if (openSfx != null)
                openSfx.Play();
            if (closeSfx != null)
                closeSfx.Stop();
            animator.SetBool(animationBoolName, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(colliderTag))
        {
            if (openSfx != null)
                openSfx.Stop();
            if (closeSfx != null)
                closeSfx.Play();
            animator.SetBool(animationBoolName, false);
        }
    }
}
