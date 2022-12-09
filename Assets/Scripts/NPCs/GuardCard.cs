using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardCard : MonoBehaviour
{
    public float viewTriangleWidth;
    public float viewTriangleHeight;

    [Space(10)]
    public Animator anim;

    [Space(10)]
    public Transform[] idlePositions;
    public int currentIdlePosition = 0;
    public float idleTime = 3.33f;

    [Space(10)]
    public float metersPerSec;
    public float degreesPerSec;

    [Space(10)]
    public GuardCard[] twins;
    private bool isDisabling = false;

    private void Awake()
    {
        MeshCollider collider = gameObject.AddComponent<MeshCollider>();

        Vector3[] newVertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(-viewTriangleWidth / 2, 0, viewTriangleHeight),
            new Vector3(viewTriangleWidth / 2, 0, viewTriangleHeight),
            new Vector3(0, 2, 0),
            new Vector3(-viewTriangleWidth / 2, 2, viewTriangleHeight),
            new Vector3(viewTriangleWidth / 2, 2, viewTriangleHeight),
        };

        int[] newTriangles = new int[] {
            0, 2, 1,    //bottom
            3, 4, 5,    //top
            0, 1, 3,    //left 1
            1, 4, 3,    //left 2
            0, 3, 2,    //right 1
            2, 3, 5,    //right 2
            1, 2, 4,    //front 1
            2, 5, 4,    //front 2
        };

        Mesh mesh = new Mesh();
        collider.sharedMesh = mesh;
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;

        collider.convex = true;
        collider.isTrigger = true;

        if (idlePositions.Length == 0)
        {
            var pos = new GameObject("guardPos").transform;
            pos.SetParent(transform.parent);
            pos.position = transform.position;
            pos.rotation = transform.rotation;
            idlePositions = new Transform[] { pos };
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CastleGardenController.instance.PlayerCaught(this);
        }
    }

    private IEnumerator RotatePositions()
    {
        if (idlePositions.Length == 0)
            yield break;

        transform.position = idlePositions[currentIdlePosition].position;
        transform.rotation = idlePositions[currentIdlePosition].rotation;

        if (idlePositions.Length == 1)
            yield break;

        while (isActiveAndEnabled)
        {
            yield return new WaitForSeconds(idleTime);
            if (!isActiveAndEnabled)
                break;

            currentIdlePosition = (currentIdlePosition + 1) % idlePositions.Length;

            var mov = StartCoroutine(Move(idlePositions[currentIdlePosition].position));
            var rot = StartCoroutine(Rotate(idlePositions[currentIdlePosition].rotation));

            anim.SetBool("walk", true);
            yield return mov;
            yield return rot;
            anim.SetBool("walk", false);
        }
    }

    private IEnumerator Rotate(Quaternion to)
    {
        while (Mathf.Abs(transform.eulerAngles.y - to.eulerAngles.y) > degreesPerSec * Time.deltaTime)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, to, degreesPerSec * Time.deltaTime);
            yield return null;
        }
        transform.rotation = to;
    }

    private IEnumerator Move(Vector3 to)
    {
        float speedSqr = metersPerSec * metersPerSec;
        while ((transform.position - to).sqrMagnitude > speedSqr * Time.deltaTime * Time.deltaTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, to, metersPerSec * Time.deltaTime);
            yield return null;
        }
        transform.position = to;
    }

    private void OnEnable()
    {
        foreach (GuardCard guardCard in twins)
            if (!guardCard.enabled)
                guardCard.enabled = true;
        StartCoroutine(RotatePositions());
    }

    private void OnDisable()
    {
        if (isDisabling)
            return;

        isDisabling = true;
        StopAllCoroutines();
        foreach (GuardCard guardCard in twins)
            if (guardCard.enabled && !guardCard.isDisabling)
                guardCard.enabled = false;
        isDisabling = false;
    }
}
