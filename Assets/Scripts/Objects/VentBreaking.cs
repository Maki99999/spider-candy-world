using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentBreaking : MonoBehaviour
{
    public Animator anim;
    public AudioSource sound;
    public Transform fallDownPos;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            anim.SetTrigger("Break");
            sound.Play();
            StartCoroutine(Default.PlayerController.instance.MovePlayer(fallDownPos.position, 2.1f, true));
        }
    }
}
