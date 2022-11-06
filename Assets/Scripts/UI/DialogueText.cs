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

    private Mesh mesh;
    private Vector3[] vertices;
    int invisibleCharIndex = 0;

    void Start()
    {
        SetText("");
    }

    public void SetText(string text)
    {
        textMesh.text = text;

        Update();
    }

    public void SetFirstInvisibleIndex(int index)
    {
        invisibleCharIndex = index;
    }

    void Update()
    {
        textMesh.ForceMeshUpdate();
        mesh = textMesh.mesh;
        vertices = mesh.vertices;

        TMP_TextInfo textInfo = textMesh.textInfo;
        Color32[] colors = textInfo.meshInfo[0].colors32;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            Vector3 offset = Wobble(Time.time + i);
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];

            int vertexIndex = characterInfo.vertexIndex;

            if (characterInfo.character == ' ')
                continue;

            if (characterInfo.index > invisibleCharIndex)
            {
                colors[vertexIndex + 0] = new Color(0, 0, 0, 0);
                colors[vertexIndex + 1] = new Color(0, 0, 0, 0);
                colors[vertexIndex + 2] = new Color(0, 0, 0, 0);
                colors[vertexIndex + 3] = new Color(0, 0, 0, 0);
            }
            else
            {
                colors[vertexIndex + 0] = textGradient.Evaluate(Mathf.Repeat(Time.time * textGradientSpeed + vertices[vertexIndex + 0].x * textGradientWidth, 1f));
                colors[vertexIndex + 1] = textGradient.Evaluate(Mathf.Repeat(Time.time * textGradientSpeed + vertices[vertexIndex + 1].x * textGradientWidth, 1f));
                colors[vertexIndex + 2] = textGradient.Evaluate(Mathf.Repeat(Time.time * textGradientSpeed + vertices[vertexIndex + 2].x * textGradientWidth, 1f));
                colors[vertexIndex + 3] = textGradient.Evaluate(Mathf.Repeat(Time.time * textGradientSpeed + vertices[vertexIndex + 3].x * textGradientWidth, 1f));
            }

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        mesh.vertices = vertices;
        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        if (textInfo.characterCount > 0)
            textMesh.canvasRenderer.SetMesh(mesh);
        else
            textMesh.canvasRenderer.SetMesh(new Mesh());
    }

    Vector2 Wobble(float time)
    {
        return new Vector2(Mathf.Sin(time * wobbleSpeed.x) * wobbleStrength.x, Mathf.Sin(time * wobbleSpeed.y) * wobbleStrength.y);
    }
}
