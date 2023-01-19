using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;

public class TunnelPortal : MonoBehaviour
{
    private Camera playerCam;
    public Camera tunnelCam;
    public Renderer outputRenderer;
    public int outputRendererMatNum;
    public Transform outgoingTunnel;

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
        cam.position = outgoingTunnel.position + (playerCam.transform.position - displayTunnel.position);

        float angleBtwnPortals = Vector3.SignedAngle(displayTunnel.forward, outgoingTunnel.forward, Vector3.up);
        tunnelCam.transform.RotateAround(outgoingTunnel.position, Vector3.up, angleBtwnPortals + 180);

        SetNearClipPlane();
    }

    void SetNearClipPlane()
    {
        Plane p = new Plane(outgoingTunnel.forward, outgoingTunnel.position);
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
                other.GetComponent<PlayerController>().TeleportPlayerCameraView(tunnelCam.transform);
        }
    }
}
