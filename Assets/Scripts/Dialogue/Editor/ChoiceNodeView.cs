using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using BlueGraph;
using BlueGraph.Editor;
using UnityEditor;

[CustomNodeView(typeof(ChoiceNode))]
public class ChoiceNodeView : NodeView
{
    private ChoiceNode node;
    private string[] previewTexts = new string[0];

    private class SerializedInstance : ScriptableObject
    {
        public LocalizedString choicesText;
    }

    private SerializedInstance serializedInstance;
    private SerializedObject serializedObject;

    protected override void OnInitialize()
    {
        node = (ChoiceNode)Target;

        serializedInstance = ScriptableObject.CreateInstance<SerializedInstance>();
        serializedInstance.choicesText = node.choicesText;
        serializedObject = new SerializedObject(serializedInstance);

        node.choicesText.StringChanged += LocalizedStringChanged;

        var container = new IMGUIContainer(OnGUI);
        extensionContainer.Add(container);
        RefreshExpandedState();
    }

    protected override void OnDestroy()
    {
        node.choicesText.StringChanged -= LocalizedStringChanged;
    }

    private void LocalizedStringChanged(string s)
    {
        if (s == null)
            previewTexts = new string[0];
        else
            previewTexts = s.Split("\n");
    }

    private void OnGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("choicesText"));
        if (serializedObject.ApplyModifiedProperties())
            node.choicesText = serializedInstance.choicesText;

        node.textLine1 = EditorGUILayout.IntField("Text Line 1", node.textLine1);
        if (previewTexts.Length > node.textLine1 && node.textLine1 >= 0)
            GUILayout.Label(" > " + previewTexts[node.textLine1]);
        node.textLine2 = EditorGUILayout.IntField("Text Line 2", node.textLine2);
        if (previewTexts.Length > node.textLine2 && node.textLine2 >= 0)
            GUILayout.Label(" > " + previewTexts[node.textLine2]);
        node.textLine3 = EditorGUILayout.IntField("Text Line 3", node.textLine3);
        if (previewTexts.Length > node.textLine3 && node.textLine3 >= 0)
            GUILayout.Label(" > " + previewTexts[node.textLine3]);
        node.textLine4 = EditorGUILayout.IntField("Text Line 4", node.textLine4);
        if (previewTexts.Length > node.textLine4 && node.textLine4 >= 0)
            GUILayout.Label(" > " + previewTexts[node.textLine4]);

    }
}
