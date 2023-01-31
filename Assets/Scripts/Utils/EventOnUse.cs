using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnUse : MonoBehaviour, Useable
{
    [SerializeField] private UnityEvent onUse;

    public void Use()
    {
        if (isActiveAndEnabled)
            onUse.Invoke();
    }
}
