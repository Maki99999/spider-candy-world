using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using BlueGraph;

[Tags("Common"), Node]
public class ChoiceNode : Node
{
    [Input(Multiple = true)] public DialogueFlowData flowIn;
    public LocalizedString choicesText;
    public int textLine1;
    [Output] public DialogueFlowData choice1;
    public int textLine2;
    [Output] public DialogueFlowData choice2;
    public int textLine3;
    [Output] public DialogueFlowData choice3;
    public int textLine4;
    [Output] public DialogueFlowData choice4;

    public override object OnRequestValue(Port port) => null;
}
