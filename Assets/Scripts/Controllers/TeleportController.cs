using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TeleportController : MonoBehaviour
{
    public static TeleportController instance { get; private set; }

    public Default.PlayerController player;
    public Animator fadeAnim;
    public AudioSource teleportSound;

    private void Awake()
    {
        instance = this;
    }

    public void TeleportPlayerSlow(Transform newPos, UnityEvent eventOnTeleport)
    {
        StopAllCoroutines();
        StartCoroutine(TeleportPlayerSlowAnim(newPos, eventOnTeleport));
    }

    private IEnumerator TeleportPlayerSlowAnim(Transform newPos, UnityEvent eventOnTeleport)
    {
        player.SetFrozen(true);

        teleportSound.Play();
        fadeAnim.SetBool("Black", true);
        yield return new WaitForSeconds(0.6f);
        eventOnTeleport.Invoke();
        player.TeleportPlayer(newPos);
        fadeAnim.SetBool("Black", false);

        player.SetFrozen(false);
    }
}
