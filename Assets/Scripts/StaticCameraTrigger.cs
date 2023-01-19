using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Default
{
    public class StaticCameraTrigger : MonoBehaviour
    {
        public Transform cameraTransform;
        public UnityEvent eventSwapIn;
        public UnityEvent eventSwapOut;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                CameraController.instance.AddCamera(cameraTransform, eventSwapIn, eventSwapOut, false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                CameraController.instance.RemoveCamera(cameraTransform);
            }
        }
    }
}
