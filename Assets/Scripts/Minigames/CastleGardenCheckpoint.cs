using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleGardenCheckpoint : MonoBehaviour
{
    public Transform checkpointPos;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CastleGardenController.instance.CheckpointReached(checkpointPos);
        }
    }
}
