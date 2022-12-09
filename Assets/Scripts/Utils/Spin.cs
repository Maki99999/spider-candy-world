using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public Vector3 degreesPerSecond;

    private void OnEnable()
    {
        StartCoroutine(SpinAround());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpinAround()
    {
        while (isActiveAndEnabled)
        {
            transform.Rotate(degreesPerSecond);
            yield return null;
        }
    }
}
