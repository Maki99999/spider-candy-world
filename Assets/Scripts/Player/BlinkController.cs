using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer blinkMesh;
    [SerializeField] private int blinkBlendShapeIndex;

    private IEnumerator Start()
    {
        while (isActiveAndEnabled)
        {
            yield return Blink();
            yield return new WaitForSeconds(Random.Range(4, 6));
        }
    }

    private IEnumerator Blink()
    {
        blinkMesh.SetBlendShapeWeight(blinkBlendShapeIndex, 100f);
        yield return new WaitForSeconds(0.3f);
        blinkMesh.SetBlendShapeWeight(blinkBlendShapeIndex, 0f);
    }
}
