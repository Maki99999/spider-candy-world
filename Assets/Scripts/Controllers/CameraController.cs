using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Default
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private new Transform camera;
        [SerializeField] private CameraMode defaultCameraMode;
        [SerializeField] private List<CameraSwitcherInfo> cameraSwitcher = new List<CameraSwitcherInfo>();
        private ulong currentCameraSwitcherId = 0;
        private CameraMode currentCameraMode;
        [SerializeField] private MonoBehaviour crosshair;

        [Space(10), SerializeField] private Transform firstPersonCamPos;
        [SerializeField] private Transform overShoulderCamPos;
        private List<CameraStuff> staticCams = new List<CameraStuff>();

        [Space(10), SerializeField] private Transform player;
        [SerializeField] private Transform playerAnim;

        private CameraStuff currentCamera = null;
        [HideInInspector] public List<CameraObserver> observers = new List<CameraObserver>();

        private Coroutine camTransition;

        public static CameraController instance { get; private set; }

        private void Awake()
        {
            instance = this;
            UpdateCameraMode(true);
        }

        public void ResetCameraMode(ulong switcherId)
        {
            cameraSwitcher.RemoveAll((info => info.id == switcherId));

            UpdateCameraMode();
        }

        public ulong SetCameraMode(CameraMode newCameraMode)
        {
            ulong newSwitcherId = currentCameraSwitcherId++;

            cameraSwitcher.Add(new CameraSwitcherInfo(newSwitcherId, newCameraMode));

            UpdateCameraMode();

            return newSwitcherId;
        }

        private void UpdateCameraMode(bool instant = false)
        {
            currentCameraMode = cameraSwitcher.Count > 0 ? cameraSwitcher[cameraSwitcher.Count - 1].mode : defaultCameraMode;

            Transform newCameraParent = null;

            if (currentCameraMode == CameraMode.FIRST_PERSON)
                newCameraParent = firstPersonCamPos;
            else if (staticCams.Count > 0)
            {
                newCameraParent = staticCams[0].camPos;
                currentCamera = staticCams[0];
                if (staticCams[0].eventSwapIn != null)
                    staticCams[0].eventSwapIn.Invoke();
            }
            else
                newCameraParent = overShoulderCamPos;

            if (currentCameraMode == CameraMode.FIRST_PERSON)
            {
                if (crosshair != null)
                    crosshair.enabled = true;
                camera.GetComponent<Camera>().cullingMask |= (1 << LayerMask.NameToLayer("FirstPersonOnly"));
                camera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("ThirdPersonOnly"));
            }
            else
            {
                if (crosshair != null)
                    crosshair.enabled = false;
                camera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("FirstPersonOnly"));
                camera.GetComponent<Camera>().cullingMask |= (1 << LayerMask.NameToLayer("ThirdPersonOnly"));
            }

            if (camera.parent != newCameraParent)
            {
                if (instant)
                    camera.SetParent(newCameraParent, false);
                else
                    StartCamTransition(newCameraParent);
                NotifiyObservers();
            }
        }

        private void StartCamTransition(Transform newCameraParent)
        {
            if (camTransition != null)
                StopCoroutine(camTransition);
            camTransition = StartCoroutine(CamTransition(newCameraParent));
        }

        private IEnumerator CamTransition(Transform newCameraParent)
        {
            camera.SetParent(transform, true);
            Vector3 positionOld = camera.position;
            Quaternion rotationOld = camera.rotation;

            float rate = 1f / 0.8f;
            float fSmooth;
            for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
            {
                fSmooth = 1 - Mathf.Pow(1 - f, 3);
                camera.position = Vector3.Lerp(positionOld, newCameraParent.position, fSmooth);
                camera.rotation = Quaternion.Lerp(rotationOld, newCameraParent.rotation, fSmooth);

                yield return null;
            }

            camera.SetParent(newCameraParent, false);
            camera.localPosition = Vector3.zero;
            camera.localEulerAngles = Vector3.zero;
        }

        public void AddStaticCamPos(Transform staticCamPos, UnityEvent eventSwapIn = null, UnityEvent eventSwapOut = null, string cameraName = "")
        {
            CameraStuff newCam = new CameraStuff(staticCamPos, eventSwapIn, eventSwapOut, cameraName);
            this.staticCams.Add(newCam);

            UpdateCameraMode();
        }

        public void RemoveStaticCamPos(Transform staticCamPos)
        {
            CameraStuff toRemove = null;
            foreach (CameraStuff cameraStuff in staticCams)
                if (cameraStuff.camPos == staticCamPos)
                    toRemove = cameraStuff;

            if (toRemove != null)
            {
                bool wasCurrentCam = toRemove == currentCamera;
                staticCams.Remove(toRemove);

                if (wasCurrentCam)
                {
                    if (toRemove.eventSwapOut != null && currentCameraMode == CameraMode.STATIC)
                        toRemove.eventSwapOut.Invoke();

                    UpdateCameraMode();
                }
            }
        }

        private void NotifiyObservers()
        {
            foreach (CameraObserver observer in observers)
                observer.CameraChanged(currentCamera == null ? "" : currentCamera.cameraName);
        }

        private class CameraStuff
        {
            public Transform camPos;
            public UnityEvent eventSwapIn;
            public UnityEvent eventSwapOut;
            public string cameraName;

            public CameraStuff(Transform camPos, UnityEvent eventSwapIn, UnityEvent eventSwapOut, string cameraName)
            {
                this.camPos = camPos;
                this.eventSwapIn = eventSwapIn;
                this.eventSwapOut = eventSwapOut;
                this.cameraName = cameraName;
            }
        }

        struct CameraSwitcherInfo
        {
            public ulong id;
            public CameraMode mode;

            public CameraSwitcherInfo(ulong id, CameraMode mode)
            {
                this.id = id;
                this.mode = mode;
            }
        }
    }

    public enum CameraMode
    {
        FIRST_PERSON,
        OVER_SHOULDER,
        STATIC,
    }

    public interface CameraObserver
    {
        public void CameraChanged(string newCameraName);
    }
}
