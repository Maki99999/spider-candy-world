using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool onlyYRot = true;
    public bool lookAway = false;
    public Transform camTransform;

    void Start()
    {
        if (camTransform == null)
            camTransform = Camera.main.transform;
    }

    void Update()
    {
        if (!lookAway)
            transform.LookAt(camTransform.position, Vector3.up);
        else
            transform.rotation = Quaternion.LookRotation(transform.position - camTransform.position);

        if (onlyYRot)
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }
}
