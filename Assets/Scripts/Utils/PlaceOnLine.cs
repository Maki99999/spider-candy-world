using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class PlaceOnLine : MonoBehaviour
{
    public GameObject prefab;
    public float spacingBetwPrefabs = 1f;
    public Transform lineParent;
    public Transform prefabDump;

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(PlaceOnLine))]
    public class YourScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PlaceOnLine myTarget = (PlaceOnLine)target;

            DrawDefaultInspector();
            if (GUILayout.Button("Update"))
            {
                myTarget.UpdateObjects();
            }
        }
    }

#endif

    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += UpdateObjects;
    }

    private void UpdateObjects()
    {
        if (prefab == null || spacingBetwPrefabs <= 0f || lineParent == null || prefabDump == null || lineParent.childCount < 2)
            return;

        // remove old stuff
        while (prefabDump.childCount > 0)
        {
            DestroyImmediate(prefabDump.GetChild(0).gameObject);
        }

        // Create children
        float remainder = 0;
        for (int i = 0; i < lineParent.childCount - 1; i++)
        {
            Vector3 start = lineParent.GetChild(i).position;
            Vector3 end = lineParent.GetChild(i + 1).position;
            Vector3 dir = (end - start).normalized;
            start += dir * remainder;

            float dist = Vector3.Distance(start, end);
            int numInstances = Mathf.FloorToInt(dist / spacingBetwPrefabs);
            remainder = spacingBetwPrefabs - (dist % spacingBetwPrefabs);

            for (int j = 0; j <= numInstances; j++)
            {
                Vector3 pos = start + (dir * j * spacingBetwPrefabs);
                GameObject pref = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                pref.transform.parent = prefabDump;
                pref.transform.position = pos;
                pref.transform.rotation = Quaternion.identity;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < lineParent.childCount - 1; i++)
            Gizmos.DrawLine(lineParent.GetChild(i).position, lineParent.GetChild(i + 1).position);
    }
}
