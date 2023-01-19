using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    new public Renderer renderer;
    new public Rigidbody rigidbody;
    public Material[] materials;

    IEnumerator Start()
    {
        renderer.material = materials[Random.Range(0, materials.Length)];

        while (isActiveAndEnabled)
        {
            rigidbody.velocity = new Vector3(0, Random.value * 0.3f - 0.15f, 0);
            yield return new WaitForSeconds(Random.Range(10f, 30f));
        }
    }
}