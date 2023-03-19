using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using BlueGraph;
using BlueGraph.Editor;
using UnityEditor;

[CustomNodeView(typeof(ParticipantNode))]
public class ParticipantNodeView : NodeView
{
    private class SerializedInstance : ScriptableObject
    {
        public LocalizedString textSource;
        public LocalizedAudioClip audioSource;
        public LocalizedString audioSourceTimestamps;
    }

    private SerializedInstance serializedInstance;
    private SerializedObject serializedObject;

    private ParticipantNode node;
    private string textPreviewRaw;
    private Label textPreviewLabel;

    protected override void OnInitialize()
    {
        node = (ParticipantNode)Target;

        serializedInstance = ScriptableObject.CreateInstance<SerializedInstance>();
        serializedInstance.textSource = node.participant.text;
        serializedInstance.audioSource = node.participant.audioClip;
        serializedInstance.audioSourceTimestamps = node.participant.audioClipTimestamps;
        serializedObject = new SerializedObject(serializedInstance);

        var container = new IMGUIContainer(OnGUI);
        extensionContainer.Add(container);

        textPreviewLabel = new Label();
        var labelWrap = new Foldout();
        labelWrap.value = false;
        labelWrap.text = "Text Preview";
        labelWrap.contentContainer.Add(textPreviewLabel);
        extensionContainer.Add(labelWrap);
        node.participant.text.StringChanged += LocalizedStringChanged;

        RefreshExpandedState();
    }

    protected override void OnDestroy()
    {
        node.participant.text.StringChanged -= LocalizedStringChanged;
    }

    private void LocalizedStringChanged(string s)
    {
        textPreviewLabel.text = s;
        RefreshExpandedState();
    }

    private void OnGUI()
    {
        serializedObject.Update();
        node.participant.participantName = EditorGUILayout.TextField("participantName", node.participant.participantName);
        node.participant.camPosition = EditorGUILayout.ObjectField("camPosition", node.participant.camPosition, typeof(Transform), true) as Transform;
        node.participant.customAudio = EditorGUILayout.Toggle("customAudio", node.participant.customAudio);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("textSource"));
        if (node.participant.customAudio)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("audioSource"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("audioSourceTimestamps"));
        }
        else
        {
            node.participant.audioAlt = (DialogueVoice)EditorGUILayout.EnumPopup("audioAlt", node.participant.audioAlt);
        }

        if (serializedObject.ApplyModifiedProperties())
        {
            node.participant.text = serializedInstance.textSource;
            node.participant.audioClip = serializedInstance.audioSource;
            node.participant.audioClipTimestamps = serializedInstance.audioSourceTimestamps;
        }
    }
}
