using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{
    public static CameraController instance { get; private set; }

    [SerializeField] private new Transform camera;
    [SerializeField] private CameraStuff defaultCamera;

    private List<CameraStuff> staticCams = new List<CameraStuff>();

    [HideInInspector] public bool shakingEffect;

    private CameraStuff currentCamera = null;

    private Coroutine camTransition;
    private bool inTransition = false;

    private void Awake()
    {
        instance = this;
        UpdateCameraMode(true);
    }

    private void Update()
    {
        if (shakingEffect)
        {
            float sign = Mathf.Sign(camera.localPosition.x);
            camera.localPosition += Vector3.right * sign * Time.deltaTime * 0.5f;

            if (Mathf.Abs(camera.localPosition.x) > 0.01f)
                camera.localPosition = Vector3.right * -sign * Time.deltaTime * 0.5f;
        }
        else if (!inTransition)
        {
            camera.localPosition = Vector3.zero;
        }
    }

    private void UpdateCameraMode(bool instant = false)
    {
        CameraStuff oldCamera = currentCamera;

        if (staticCams.Count > 0)
            currentCamera = staticCams[0];
        else
            currentCamera = defaultCamera;

        if (currentCamera.eventSwapIn != null)
            currentCamera.eventSwapIn.Invoke();


        if (oldCamera == null || oldCamera.hidePlayer != currentCamera.hidePlayer)
            if (currentCamera.hidePlayer)
            {
                camera.GetComponent<Camera>().cullingMask |= (1 << LayerMask.NameToLayer("FirstPersonOnly"));
                camera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("ThirdPersonOnly"));
            }
            else
            {
                camera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("FirstPersonOnly"));
                camera.GetComponent<Camera>().cullingMask |= (1 << LayerMask.NameToLayer("ThirdPersonOnly"));
            }

        if (camera.parent != currentCamera.camPos)
        {
            if (instant)
                camera.SetParent(currentCamera.camPos, false);
            else
                StartCamTransition(currentCamera.camPos);
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
        inTransition = true;

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

        inTransition = false;
    }

    public void AddCamera(Transform staticCamPos, UnityEvent eventSwapIn = null, UnityEvent eventSwapOut = null, bool hidePlayer = false, bool instant = false)
    {
        CameraStuff newCam = new CameraStuff(staticCamPos, eventSwapIn, eventSwapOut, hidePlayer);
        this.staticCams.Add(newCam);

        UpdateCameraMode(instant);
    }

    public void RemoveCamera(Transform staticCamPos, bool instant = false)
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
                if (toRemove.eventSwapOut != null)
                    toRemove.eventSwapOut.Invoke();

                UpdateCameraMode(instant);
            }
        }
    }

    [System.Serializable]
    private class CameraStuff
    {
        public Transform camPos;
        public UnityEvent eventSwapIn;
        public UnityEvent eventSwapOut;
        public bool hidePlayer;

        public CameraStuff(Transform camPos, UnityEvent eventSwapIn, UnityEvent eventSwapOut, bool hidePlayer)
        {
            this.camPos = camPos;
            this.eventSwapIn = eventSwapIn;
            this.eventSwapOut = eventSwapOut;
            this.hidePlayer = hidePlayer;
        }
    }
}
