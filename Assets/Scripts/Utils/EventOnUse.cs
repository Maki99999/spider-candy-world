using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnUse : MonoBehaviour, Default.Useable
{
    [SerializeField] private UnityEvent onUse;

    public void Use()
    {
        onUse.Invoke();
    }
}
