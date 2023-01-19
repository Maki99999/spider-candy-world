using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIKController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform lookAtTransform;

    private void OnAnimatorIK(int layerIndex)
    {
        animator.SetLookAtWeight(0.7f);
        animator.SetLookAtPosition(lookAtTransform.position + lookAtTransform.forward);
    }
}
