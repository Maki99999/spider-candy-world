using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyWhenNotVisible : MonoBehaviour
{
    public float minTimeAlive = 0f;
    public Renderer targetRenderer;
    private bool ready = false;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(minTimeAlive);
        ready = true;
    }

    private void Update()
    {
        if (ready && !targetRenderer.isVisible)
            Destroy(gameObject);
    }
}
