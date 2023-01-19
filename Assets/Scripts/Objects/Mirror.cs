using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;

public class Mirror : MonoBehaviour
{
    private Camera playerCam;
    public Camera tunnelCam;
    public Renderer outputRenderer;
    public int outputRendererMatNum;

    private void Awake()
    {
        playerCam = Camera.main;

        var renderTexture = new RenderTexture(Screen.width, Screen.height, 16);

        tunnelCam.targetTexture = renderTexture;
        outputRenderer.materials[outputRendererMatNum].mainTexture = renderTexture;
    }

    private void OnEnable()
    {
        RenderPipeline.beginCameraRendering += Render;
    }

    private void OnDisable()
    {
        RenderPipeline.beginCameraRendering -= Render;
    }

    public void Render(ScriptableRenderContext SRC, Camera camera)
    {
        Transform displayTunnel = transform;
        Transform cam = tunnelCam.transform;

        cam.forward = playerCam.transform.forward;
        cam.position = transform.position + (playerCam.transform.position - displayTunnel.position);

        float angleBtwnPortals = Vector3.SignedAngle(displayTunnel.forward, transform.forward, Vector3.up);
        tunnelCam.transform.RotateAround(transform.position, Vector3.up, angleBtwnPortals + 180);

        Vector3 reflDir = Vector3.Reflect(tunnelCam.transform.position - transform.transform.position, transform.right);
        tunnelCam.transform.position = transform.transform.position + reflDir;

        tunnelCam.transform.forward = Vector3.Reflect(tunnelCam.transform.forward, transform.right);

        SetNearClipPlane();
    }

    void SetNearClipPlane()
    {
        Plane p = new Plane(transform.forward, transform.position);
        Vector4 clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(tunnelCam.worldToCameraMatrix)) * clipPlaneWorldSpace;

        var newMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        tunnelCam.projectionMatrix = newMatrix;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var dist = new Plane(-transform.forward, transform.position).GetDistanceToPoint(other.transform.position);
            if (dist > 0f)
                other.GetComponent<Default.PlayerController>().TeleportPlayerCameraView(tunnelCam.transform);
        }
    }
}
