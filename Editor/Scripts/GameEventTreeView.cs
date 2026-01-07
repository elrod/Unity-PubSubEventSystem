using System;
using System.Collections.Generic;
using System.Linq;
using com.elrod.pubsubeventsystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.elrod.pubsubeventssystem.editor
{
    public class GameEventTreeView : GraphView
    {
        private readonly Vector2 betweenNodesMinDistance = new Vector2(250, 0);

        private readonly Vector2 defaultNodeSize = new Vector2(220, 200);

        private readonly GameEventTree eventsTree;
        private readonly Vector2 rootNodePosition = new Vector2(100, 400);
        
        private GameEventNodeView rootNode;
        private Blackboard _blackboard;

        public override bool supportsWindowedBlackboard => true;

        public GameEventTreeView(GameEventTree allEventsTree, bool interactable = true)
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            eventsTree = allEventsTree;

            this.AddManipulator(new ContentDragger());

            if (interactable)
            {
                this.AddManipulator(new SelectionDragger());
                this.AddManipulator(new RectangleSelector());
            }
            
            GenerateTree();
        }

        private void GenerateTree()
        {
            DeleteElements(graphElements);

            rootNode = GenerateRootNode(eventsTree.Root.Topic);
            AddElement(rootNode);

            // Draw Nodes
            var treeLayers = eventsTree.GetTreeLayers();

            var maxNodesCount = 0;
            foreach (var k in treeLayers.Keys)
            {
                if (treeLayers[k].Count > maxNodesCount)
                {
                    maxNodesCount = treeLayers[k].Count;
                }
            }
            
            var maxChildrenBoxHeight = defaultNodeSize.y * maxNodesCount;

            for (var i = 1; i < treeLayers.Keys.Count; i++)
            {
                var nextLayerItemsCount = eventsTree.GetNodeCountInLayer(i);
                var betweenElementsOffset = maxNodesCount / (float) nextLayerItemsCount;
                var childrenBoxStartHeight = rootNodePosition.y - maxChildrenBoxHeight / 2f;
                var currOffset = 0f;
                foreach (var topic in treeLayers[i])
                {
                    var nodePos = new Vector2(
                        rootNodePosition.x + defaultNodeSize.x * i + betweenNodesMinDistance.x * i,
                        childrenBoxStartHeight + (defaultNodeSize.y + currOffset));
                    currOffset += defaultNodeSize.y * betweenElementsOffset;
                    var childrenCount = i == treeLayers.Keys.Count - 1 ? 0 :
                            treeLayers[i + 1].Count(t => topic.IsParentOf(t));
                    var node = CreateEventTreeNode(topic, nodePos, collapsedChildren: childrenCount, layer: i);
                    AddElement(node);
                }
            }

            // Draw Connections
            ports.ForEach(p1 =>
            {
                ports.ForEach(p2 =>
                {
                    if (p1 != p2 && p1.node != p2.node)
                    {
                        var srcNode = p1.node as GameEventNodeView;
                        var dstNode = p2.node as GameEventNodeView;

                        int lastIndexOfSlash = dstNode.title.LastIndexOf("/");
                        bool parentIsRoot = lastIndexOfSlash == 0;
                        bool srcIsParent = dstNode.title.Substring(0, lastIndexOfSlash) == srcNode.title;

                        if ((parentIsRoot || srcIsParent) && srcNode.NodeTreeLayer == dstNode.NodeTreeLayer - 1)
                        {
                            var edge = new Edge();
                            edge.output = srcNode.OutputPort;
                            edge.input = dstNode.InputPort;
                            dstNode.InputPort.Connect(edge);
                            srcNode.OutputPort.Connect(edge);
                            AddElement(edge);
                        }
                    }
                });
            });
        }

        private Port GeneratePort(GameEventNodeView node, Direction portDirection,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }

        public override Blackboard GetBlackboard() => _blackboard;

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node) compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private GameEventNodeView GenerateRootNode(string rootTitle = "/")
        {
            var node = new GameEventNodeView()
            {
                GUID = Guid.NewGuid().ToString(),
                title = rootTitle,
                IsRoot = true,
                AssociatedNode = eventsTree.GetTopic("/")
            };

            var outputPort = GeneratePort(node, Direction.Output, Port.Capacity.Multi);
            outputPort.portName = "";
            node.outputContainer.Add(outputPort);

            node.OutputPort = outputPort;

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(rootNodePosition, defaultNodeSize));
            return node;
        }

        private GameEventNodeView CreateEventTreeNode(string nodeName, Vector2 position = new Vector2(), int collapsedChildren = 0, int layer = 0)
        {
            var node = new GameEventNodeView
            {
                GUID = Guid.NewGuid().ToString(),
                title = nodeName,
                NodeTreeLayer = layer,
                AssociatedNode = eventsTree.GetTopic(nodeName)
            };

            var inputPort = GeneratePort(node, Direction.Input);
            inputPort.portName = "";

            node.inputContainer.Add(inputPort);
            node.InputPort = inputPort;

            var outputPort = GeneratePort(node, Direction.Output, Port.Capacity.Multi);
            outputPort.portName = collapsedChildren == 0 ? "" : $"({collapsedChildren})";
            node.outputContainer.Add(outputPort);

            node.OutputPort = outputPort;

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(position, defaultNodeSize));
            return node;
        }
    }
}