using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using BlueGraph;

[Tags("Common")]
[Node]
public class TextNode : Node
{
    [Input(Multiple = true)] public DialogueFlowData flowIn;
    [Input] public ParticipantData participant;
    [Output] public DialogueFlowData flowOut;

    public int textLineStart;
    public int textLineCount;

    public override object OnRequestValue(Port port) => null;

    public ParticipantData GetParticipantRuntime() => GetInputValue("participant", participant);
}
