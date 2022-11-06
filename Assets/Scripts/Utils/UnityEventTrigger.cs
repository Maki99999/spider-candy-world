using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent unityEvent;
    [SerializeField] private string colliderTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(colliderTag))
            unityEvent.Invoke();
    }
}
