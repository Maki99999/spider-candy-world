using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onTriggerEnter;
    [SerializeField] private UnityEvent onTriggerExit;
    [SerializeField] private string colliderTag = "Player";
    [SerializeField] private bool ignoreActiveState = true;

    private void OnTriggerEnter(Collider other)
    {
        if ((ignoreActiveState || isActiveAndEnabled) && other.CompareTag(colliderTag))
            onTriggerEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if ((ignoreActiveState || isActiveAndEnabled) && other.CompareTag(colliderTag))
            onTriggerExit.Invoke();
    }
}
