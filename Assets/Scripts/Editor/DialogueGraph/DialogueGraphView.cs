using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class DialogueGraphView : GraphView
{
    private DialogueGraphEditor editorWindow;

    public DialogueGraphView(DialogueGraphEditor editorWindow)
    {
        this.editorWindow = editorWindow;

        //manipulators
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new FreehandSelector());
        this.AddManipulator(CreateNodeContextualMenu());

        //grid
        var grid = new GridBackground();
        grid.StretchToParentSize();
        Insert(0, grid);
    }

    private IManipulator CreateNodeContextualMenu()
    {
        ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction("Add Node", actionEvent => AddElement(new DialogueGraphNode(GetFreeId(), this, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
        );
        return contextualMenuManipulator;
    }

    public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
    {
        Vector2 worldMousePosition = mousePosition;

        if (isSearchWindow)
        {
            worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition - editorWindow.position.position);
        }

        Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

        return localMousePosition;
    }

    private int GetFreeId()
    {
        List<int> ids = new List<int>();

        foreach (var node in nodes)
        {
            if (node.GetType() == typeof(DialogueGraphNode))
                ids.Add(((DialogueGraphNode)node).id);
        }

        return Enumerable.Range(1, Int32.MaxValue).Except(ids).First();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList()!.Where(endPort =>
                      endPort.direction != startPort.direction &&
                      endPort.node != startPort.node &&
                      endPort.portType == startPort.portType)
          .ToList();
    }
}
