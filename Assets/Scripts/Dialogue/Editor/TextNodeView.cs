using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using BlueGraph;
using BlueGraph.Editor;
using UnityEditor;

[CustomNodeView(typeof(TextNode))]
public class TextNodeView : NodeView
{
    private LocalizedString text;
    private TextNode node;
    private string textPreviewRaw;
    private Label textPreviewLabel;

    protected override void OnInitialize()
    {
        node = (TextNode)Target;

        var container = new IMGUIContainer(OnGUI);
        extensionContainer.Add(container);

        textPreviewLabel = new Label();
        extensionContainer.Add(textPreviewLabel);

        RefreshExpandedState();
    }

    protected override void OnDestroy()
    {
        if (text != null)
            text.StringChanged -= LocalizedStringChanged;
    }

    private void LocalizedStringChanged(string s)
    {
        textPreviewRaw = s;
        UpdateTextPreview();
    }

    private void UpdateTextPreview()
    {
        if (System.String.IsNullOrEmpty(textPreviewRaw))
        {
            textPreviewLabel.text = "";
        }
        else
        {
            string[] split = textPreviewRaw.Split('\n');
            System.Text.StringBuilder formatted = new System.Text.StringBuilder("\n");
            if (node.textLineStart >= 0 && node.textLineStart < split.Length)
                for (int i = node.textLineStart; i < Mathf.Min(node.textLineStart + node.textLineCount, split.Length); i++)
                    formatted.Append(split[i]).Append("\n");
            textPreviewLabel.text = formatted.ToString();
        }
        RefreshExpandedState();
    }

    private void OnGUI()
    {
        if (text == null && node != null && node.GetPort("participant") != null && node.GetPort("participant").ConnectionCount > 0)
        {
            text = ((ParticipantNode)node.GetPort("participant").ConnectedPorts.First<Port>().Node).participant.text;
            text.StringChanged += LocalizedStringChanged;
        }

        int newTextLineStart = EditorGUILayout.IntField("textLineStart", node.textLineStart);
        int newTextLineCount = EditorGUILayout.IntField("textLineCount", node.textLineCount);
        if (newTextLineStart != node.textLineStart || newTextLineCount != node.textLineCount)
        {
            node.textLineStart = newTextLineStart;
            node.textLineCount = newTextLineCount;
            UpdateTextPreview();
        }
    }
}
