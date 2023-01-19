using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalOffsetTeleporter : MonoBehaviour
{
    public Transform fromLocalSpace;
    public Transform toLocalSpace;

    private PlayerController player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 posDiff = toLocalSpace.position - fromLocalSpace.position;

            player.TeleportAndRotateAround(player.transform.position + posDiff, toLocalSpace.position, toLocalSpace.eulerAngles.y - fromLocalSpace.eulerAngles.y);
        }
    }
}
