using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueGraph;

[Tags("Common")]
[Node]
public class ParticipantNode : Node
{
    [Output] public ParticipantData participant;

    public override object OnRequestValue(Port port) => participant;
}
