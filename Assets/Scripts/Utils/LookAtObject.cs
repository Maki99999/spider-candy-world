using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtObject : MonoBehaviour
{
    public Transform lookAtObject;
    public Vector3 offset;

    void Update()
    {
        transform.LookAt(lookAtObject.position + offset);
    }
}
