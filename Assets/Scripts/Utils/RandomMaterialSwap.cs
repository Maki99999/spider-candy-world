using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMaterialSwap : MonoBehaviour
{
    new public Renderer renderer;
    public Material[] materials;
    public Vector2 randomTime;

    private IEnumerator Start()
    {
        while (isActiveAndEnabled)
        {
            renderer.material = materials[Random.Range(0, materials.Length)];
            yield return new WaitForSeconds(Random.Range(randomTime.x, randomTime.y));
        }
    }
}
