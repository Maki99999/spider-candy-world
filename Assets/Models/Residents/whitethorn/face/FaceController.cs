using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceController : MonoBehaviour
{
    public Vector2 leftPupilOffset;
    public Vector2 rightPupilOffset;
    public bool eyesClosed;

    public SkinnedMeshRenderer mesh;
    public int materialSlot;

    private Material materialInstance;

    void Start()
    {
        materialInstance = mesh.materials[materialSlot];
    }

    void Update()
    {
        materialInstance.SetVector("_LeftPupilOffset", leftPupilOffset);
        materialInstance.SetVector("_RightPupilOffset", rightPupilOffset);
        materialInstance.SetFloat("_EyesClosed", eyesClosed ? 1f : 0f);
    }
}
