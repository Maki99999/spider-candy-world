using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphEditor : EditorWindow
{
    private DialogueGraphView _graphView;

    [MenuItem("Graph/Dialogue Graph")]
    public static void CreateGraphViewWindow()
    {
        var window = GetWindow<DialogueGraphEditor>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        _graphView = new DialogueGraphView(this);
        _graphView.name = "Dialogue Graph";
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);

        AddToolbar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

    private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            //Button saveButton = new Button(() => Save()) { text = "Clear"};
            //toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);
        }

}
