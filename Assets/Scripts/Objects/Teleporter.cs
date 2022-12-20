using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Teleporter : MonoBehaviour
{
    public Transform teleportPos;
    public UnityEvent eventOnTeleport;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TeleportController.instance.TeleportPlayerSlow(teleportPos, eventOnTeleport);
        }
    }
}
