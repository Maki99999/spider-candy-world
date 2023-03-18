using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BlueGraph;

[IncludeTags("Common")]
[CreateAssetMenu(
    menuName = "Dialogue Graph",
    fileName = "New Dialogue Graph"
)]
public class DialogueGraphWindow : Graph
{
    protected override void OnGraphEnable()
    {
#if UNITY_EDITOR
        if (GetNode<StartNode>() == null)
        {
            var node = BlueGraph.Editor.NodeReflection.Instantiate<StartNode>();
            foreach (var port in node.Ports)
                if (port.Value.Name == "flowIn")
                    node.RemovePort(port.Value);
            AddNode(node);
        }
#endif
    }

    public DialogueNode Generate()
    {
        var startNode = GetNode<StartNode>();
        if (startNode == null || startNode.Ports.Count == 0)
            return null;

        return Generate(startNode.GetPort("flowOut").ConnectedPorts.First().Node);
    }

    private DialogueNode Generate(Node startNode, DialogueNode lastNode = null)
    {
        Node currentNode = startNode;

        DialogueNode outputNode = null;

        while (currentNode != null)
        {
            if (currentNode is TextNode)
            {
                TextNode n = (TextNode)currentNode;
                string[] lines = n.participant.text.GetLocalizedString().Split('\n');
                System.Tuple<float, float>[] timestamps = null;
                if (n.participant.customAudio)
                    timestamps = n.participant.text.GetLocalizedString().Split('\n')
                            .Select<string, System.Tuple<float, float>>(s => DialogueNode.StringToStartEnd(s)).ToArray();

                if (n.textLineStart >= 0 && n.textLineStart < lines.Length)
                    for (int i = n.textLineStart; i < Mathf.Min(n.textLineStart + n.textLineCount, lines.Length); i++)
                    {
                        DialogueNode newNode;
                        if (n.participant.customAudio)
                            newNode = new DialogueNode
                            (
                                n.participant.participantName,
                                lines[i],
                                n.participant.camPosition,
                                (AudioClip)n.participant.audioClip.LoadAsset(),
                                timestamps[i].Item1,
                                timestamps[i].Item2
                            );
                        else
                            newNode = new DialogueNode
                            (
                                n.participant.participantName,
                                lines[i],
                                n.participant.camPosition,
                                n.participant.audioAlt
                            );
                        if (outputNode == null)
                            outputNode = newNode;
                        else
                            lastNode.nextNode = newNode;
                        lastNode = newNode;
                    }
            }
            else if (currentNode is ChoiceNode && lastNode != null)
            {
                ChoiceNode n = (ChoiceNode)currentNode;
                string[] choiceTexts = n.choicesText.GetLocalizedString()?.Split("\n");

                if (n.GetPort("choice1").ConnectionCount == 1 && choiceTexts != null
                        && choiceTexts.Length > n.textLine1 && n.textLine1 >= 0)
                    lastNode.AddChoice(Generate(n.GetPort("choice1").ConnectedPorts.First().Node, lastNode),
                            choiceTexts[n.textLine1]);
                if (n.GetPort("choice2").ConnectionCount == 1 && choiceTexts != null
                        && choiceTexts.Length > n.textLine2 && n.textLine2 >= 0)
                    lastNode.AddChoice(Generate(n.GetPort("choice2").ConnectedPorts.First().Node, lastNode),
                            choiceTexts[n.textLine2]);
                if (n.GetPort("choice3").ConnectionCount == 1 && choiceTexts != null
                        && choiceTexts.Length > n.textLine3 && n.textLine3 >= 0)
                    lastNode.AddChoice(Generate(n.GetPort("choice3").ConnectedPorts.First().Node, lastNode),
                            choiceTexts[n.textLine3]);
                if (n.GetPort("choice4").ConnectionCount == 1 && choiceTexts != null
                        && choiceTexts.Length > n.textLine4 && n.textLine4 >= 0)
                    lastNode.AddChoice(Generate(n.GetPort("choice4").ConnectedPorts.First().Node, lastNode),
                            choiceTexts[n.textLine4]);
            }

            currentNode = currentNode.GetPort("flowOut").ConnectedPorts.First().Node;
        }

        return outputNode;
    }
}
