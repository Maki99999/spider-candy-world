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
        public string cameraName = "";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                CameraController.instance.AddStaticCamPos(cameraTransform, eventSwapIn, eventSwapOut, cameraName);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                CameraController.instance.RemoveStaticCamPos(cameraTransform);
            }
        }
    }
}
