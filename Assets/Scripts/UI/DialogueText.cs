using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueText : MonoBehaviour
{
    public TMP_Text textMesh;

    [Space(10)]
    public Gradient textGradient;
    public float textGradientWidth;
    public float textGradientSpeed;
    public Vector2 wobbleSpeed;
    public Vector2 wobbleStrength;

    private int maxVisibleCharacters = 0;

    void Start()
    {
        HideText();
    }

    public void SetText(string text)
    {
        textMesh.text = text;

        StopAllCoroutines();
        StartCoroutine(TextAnimation());
    }

    public void SetFirstInvisibleIndex(int index)
    {
        maxVisibleCharacters = index + 1;
    }

    private Vector2 Wobble(float time)
    {
        return new Vector2(Mathf.Sin(time * wobbleSpeed.x) * wobbleStrength.x, Mathf.Sin(time * wobbleSpeed.y) * wobbleStrength.y);
    }

    public void HideText()
    {
        StopAllCoroutines();
        textMesh.text = "";

        TMP_TextInfo textInfo = textMesh.textInfo;
        int characterCount = textInfo.characterCount;

        for (int i = 0; i < characterCount; i++)
        {
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

            colors[vertexIndex + 0].a = 0;
            colors[vertexIndex + 1].a = 0;
            colors[vertexIndex + 2].a = 0;
            colors[vertexIndex + 3].a = 0;
        }
        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private IEnumerator TextAnimation()
    {
        textMesh.ForceMeshUpdate();

        TMP_TextInfo textInfo = textMesh.textInfo;
        TMP_MeshInfo[] meshInfo = textInfo.CopyMeshInfoVertexData();
        float strengthMultiplier = textMesh.fontSize / textMesh.fontSizeMax;

        while (isActiveAndEnabled)
        {
            int characterCount = textInfo.characterCount;

            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] sourceVertices = meshInfo[materialIndex].vertices;
                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

                Vector3 offset = Wobble(Time.time + i) * strengthMultiplier;

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] + offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] + offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] + offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] + offset;

                if (i > maxVisibleCharacters)
                {
                    colors[vertexIndex + 0].a = 0;
                    colors[vertexIndex + 1].a = 0;
                    colors[vertexIndex + 2].a = 0;
                    colors[vertexIndex + 3].a = 0;
                }
                else
                {
                    colors[vertexIndex + 0] = textGradient.Evaluate(Mathf.Repeat(Time.time * textGradientSpeed + sourceVertices[vertexIndex + 0].x * textGradientWidth, 1f));
                    colors[vertexIndex + 1] = textGradient.Evaluate(Mathf.Repeat(Time.time * textGradientSpeed + sourceVertices[vertexIndex + 1].x * textGradientWidth, 1f));
                    colors[vertexIndex + 2] = textGradient.Evaluate(Mathf.Repeat(Time.time * textGradientSpeed + sourceVertices[vertexIndex + 2].x * textGradientWidth, 1f));
                    colors[vertexIndex + 3] = textGradient.Evaluate(Mathf.Repeat(Time.time * textGradientSpeed + sourceVertices[vertexIndex + 3].x * textGradientWidth, 1f));
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            yield return null; ;
        }
    }
}
