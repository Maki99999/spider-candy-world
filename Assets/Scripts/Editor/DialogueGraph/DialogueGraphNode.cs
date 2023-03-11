using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEditor;

public class DialogueGraphNode : Node
{
    public int id;
    public bool customAudio;
    public uint textLine;
    public uint lineCount;
    public LocalizedString textSource = new LocalizedString();
    public LocalizedAudioClip audioFile = new LocalizedAudioClip();
    public LocalizedString audioTimestamps = new LocalizedString();
    public DialogueVoice audioAlt = DialogueVoice.DEFAULT;

    private SerializedInstance m_Instance;
    private SerializedObject m_SerializedObject;
    private DialogueGraphView graphView;

    public class SerializedInstance : ScriptableObject
    {
        public uint textLine;
        public uint lineCount;
        public LocalizedString textSource;
        public bool customAudio;
        public LocalizedAudioClip audioFile;
        public LocalizedString audioTimestamps;
        public DialogueVoice audioAlt;
    }

    public bool IsStartingNode()
    {
        Port inputPort = (Port)inputContainer.Children().First();
        return !inputPort.connected;
    }

    public DialogueGraphNode(int id, DialogueGraphView graphView, Vector2 position)
    {
        title = "Node " + id;
        this.id = id;
        this.graphView = graphView;

        SetPosition(new Rect(position, Vector2.zero));

        Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        inputPort.portName = "Input";

        inputContainer.Add(inputPort);
        AddOutputPort();

        extensionContainer.Add(new Button(() => AddOutputPort()) { text = "Add Output" });

        //extensionContainer.Add(CreateLocalizedStringField());
        extensionContainer.Add(new LocalizedStringVisualElement());

        RefreshPorts();
        RefreshExpandedState();
    }

    private void AddOutputPort()
    {
        //TODO: logic
        Port newPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        newPort.portName = "Output ";
        outputContainer.Add(newPort);
        RefreshPorts();
    }

    private VisualElement CreateLocalizedStringField()
    {
        if (m_Instance == null)
        {
            m_Instance = ScriptableObject.CreateInstance<SerializedInstance>();
            m_Instance.textLine = textLine;
            m_Instance.lineCount = lineCount;
            m_Instance.textSource = textSource;
            m_Instance.customAudio = customAudio;
            m_Instance.audioFile = audioFile;
            m_Instance.audioTimestamps = audioTimestamps;
            m_Instance.audioAlt = audioAlt;
            m_SerializedObject = new SerializedObject(m_Instance);
        }

        return new IMGUIContainer(() =>
        {
            m_SerializedObject.Update();
            EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("textLine"));
            EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("lineCount"));
            EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("textSource"));
            EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("customAudio"));
            if (customAudio)
            {
                EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("audioFile"));
                EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("audioTimestamps"));
            }
            else
            {
                EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("audioAlt"), new GUIContent("Voice"));
            }
            if (m_SerializedObject.ApplyModifiedProperties())
            {
                textSource = m_Instance.textSource;
                audioFile = m_Instance.audioFile;
                audioTimestamps = m_Instance.audioTimestamps;
                textLine = m_Instance.textLine;
                lineCount = m_Instance.lineCount;
                customAudio = m_Instance.customAudio;
                audioAlt = m_Instance.audioAlt;
            }
            //TODO logic on value change
        });
    }
}
