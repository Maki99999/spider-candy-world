using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomController : MonoBehaviour
{
    public FOVController fOVController;

    void Update()
    {
        fOVController.isZooming = InputManager.PressingZoom();
    }
}
