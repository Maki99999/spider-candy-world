using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Default
{
    public class ZoomController : MonoBehaviour
    {
        public FOVController fOVController;

        void Update()
        {
            fOVController.isZooming = InputManager.PressingZoom();
        }
    }
}
