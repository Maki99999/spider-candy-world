using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTower : MonoBehaviour
{
    public int layerCount = 100;
    public Material material;
    public Vector2 radius;
    public Vector2 layerHeight;
    public Vector2Int ringVertCount;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private List<Vector3> previousRing;
    private float currentHeight = 0;

    private void Start()
    {
        GenerateMesh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            AddRing();
    }

    private void GenerateMesh()
    {
        //init
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        mesh = new Mesh();

        //rings
        for (int i = 0; i < layerCount; i++)
            AddRing();
    }

    private void AddRing()
    {
        //generate new ring
        int currentVertCount = Random.Range(ringVertCount.x / 2, (ringVertCount.y + 1) / 2) * 2;
        if (previousRing != null)
            currentHeight += Random.Range(layerHeight.x, layerHeight.y);
        float currentRadius = Random.Range(radius.x, radius.y);

        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < currentVertCount; i++)
        {
            var radians = 2 * Mathf.PI / currentVertCount * i;

            var vertical = Mathf.Sin(radians);
            var horizontal = Mathf.Cos(radians);

            var spawnDir = new Vector3(horizontal, currentHeight, vertical);

            vertices.Add(spawnDir * currentRadius);
        }

        List<Vector3> vertsCombined = new List<Vector3>(mesh.vertices);
        vertsCombined.AddRange(vertices);

        //connect with previous ring
        List<int> trisCombined = new List<int>(mesh.triangles);
        if (previousRing != null)
        {
            List<int> newTris = new List<int>();
            int currentRingCount = vertices.Count;
            int previousRingCount = previousRing.Count;
            int offsetCurr = mesh.vertices.Length;
            int offsetPrev = offsetCurr - previousRing.Count;

            //previous ring
            for (int i = 0; i < previousRing.Count; i++)
            {
                int neighbor = (i + 1) % previousRing.Count;
                int nearest = Mathf.RoundToInt((i + 0.5f) / previousRingCount * currentRingCount) % currentRingCount;

                newTris.Add(offsetPrev + neighbor);
                newTris.Add(offsetPrev + i);
                newTris.Add(offsetCurr + nearest);
            }
            //new ring
            for (int i = 0; i < vertices.Count; i++)
            {
                int neighbor = (i + 1) % vertices.Count;
                int nearest = Mathf.RoundToInt((i + 0.5f) / currentRingCount * previousRingCount) % previousRingCount;

                newTris.Add(offsetCurr + i);
                newTris.Add(offsetCurr + neighbor);
                newTris.Add(offsetPrev + nearest);
            }

            trisCombined.AddRange(newTris);
        }
        previousRing = vertices;

        mesh.vertices = vertsCombined.ToArray();
        mesh.triangles = trisCombined.ToArray();
        //mesh.RecalculateNormals();

        if (mesh.triangles.Length > 0)
        {
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }
}
