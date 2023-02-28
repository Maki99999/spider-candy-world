using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimationStart : MonoBehaviour
{
    public string animStartState;

    private void Start()
    {
        Animator anim = GetComponent<Animator>();
        anim.Play(animStartState, 0, Random.Range(0, 1f));
    }
}
