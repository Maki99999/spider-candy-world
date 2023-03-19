using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueGraph;

[Tags("Hidden")]
[Node(Deletable = false)]
public class StartNode : Node
{
    [Output] public DialogueFlowData flowOut;

    public override object OnRequestValue(Port port) => null;
}
