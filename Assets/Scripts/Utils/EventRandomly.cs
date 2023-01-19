using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventRandomly : MonoBehaviour
{
    [SerializeField] private UnityEvent unityEvent;
    [SerializeField] private Vector2 minMaxSeconds;

    private void OnEnable()
    {
        StartCoroutine(Use());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Use()
    {
        while (isActiveAndEnabled)
        {
            yield return new WaitForSeconds(Random.Range(minMaxSeconds.x, minMaxSeconds.y));
            unityEvent.Invoke();
        }
    }
}
