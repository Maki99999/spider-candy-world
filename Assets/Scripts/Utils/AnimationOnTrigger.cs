using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOnTrigger : MonoBehaviour
{
    [SerializeField] private string colliderTag = "Player";
    [SerializeField] private Animator animator;
    [SerializeField] private string animationBoolName = "Open";
    [SerializeField] private bool setToOnTriggerEnter = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(colliderTag))
            animator.SetBool(animationBoolName, setToOnTriggerEnter);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(colliderTag))
            animator.SetBool(animationBoolName, !setToOnTriggerEnter);
    }
}
