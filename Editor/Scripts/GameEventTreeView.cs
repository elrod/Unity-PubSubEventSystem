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

        private readonly HashSet<GameEventTopic> _collapsedTopics = new();

        private GameEventNodeView rootNode;
        private Blackboard _blackboard;

        public override bool supportsWindowedBlackboard => true;

        public GameEventTreeView(GameEventTree allEventsTree, IEnumerable<GameEventTopic> collapsedTopics, bool interactable = true)
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            eventsTree = allEventsTree;

            this.AddManipulator(new ContentDragger());

            if (interactable)
            {
                this.AddManipulator(new SelectionDragger());
                this.AddManipulator(new RectangleSelector());
            }

            //GenerateBlackboard();

            //CollapseTopics(collapsedTopics);

            if (collapsedTopics == null || !collapsedTopics.Any()) GenerateTree();
        }

        private void GenerateBlackboard()
        {
            _blackboard = new(this);
            _blackboard.title = "Collapsed Topics";
            _blackboard.subTitle = "Topics hidden from view";
            _blackboard.SetPosition(new Rect(10, 30, 200, 350));

            _blackboard.addItemRequested = blackboard =>
            {
                var menu = new GenericDropdownMenu();

                eventsTree.GetTreeLeavesParents()
                        .Where(topic => !_collapsedTopics.Contains(topic))
                        .OrderBy(topic => topic)
                        .ToList()
                        .ForEach(topic => menu.AddItem(topic, false, () => CollapseTopic(topic)));

                var position = new Rect(blackboard.GetPosition())
                {
                    width = 0,
                    height = 0
                };
                position.x += blackboard.GetPosition().width;
                position.y += 20;
                menu.DropDown(position, blackboard, false);
            };

            Add(_blackboard);
        }

        private void GenerateTree()
        {
            Debug.Log("Generating Tree");
            DeleteElements(graphElements);

            rootNode = GenerateRootNode(eventsTree.Root.Topic);
            AddElement(rootNode);

            // Draw Nodes
            var treeLayers = eventsTree.GetTreeLayers();

            var maxNodesCount = 0;
            foreach (var k in treeLayers.Keys)
            {
                if (treeLayers[k].Count(element => _collapsedTopics.All(t => !t.IsParentOf(element))) >= maxNodesCount)
                    maxNodesCount = treeLayers[k].Count;
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
                    if (_collapsedTopics.Any(t => t != topic && t.IsParentOf(topic))) continue;

                    var nodePos = new Vector2(
                        rootNodePosition.x + defaultNodeSize.x * i + betweenNodesMinDistance.x * i,
                        childrenBoxStartHeight + (defaultNodeSize.y + currOffset));
                    currOffset += defaultNodeSize.y * betweenElementsOffset;
                    var childrenCount = i == treeLayers.Keys.Count - 1 || !_collapsedTopics.Contains(topic) ? 0 :
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

        private void InternalCollapse(string topic)
        {
            var topicItem = new GameEventTopic(topic);
            if (_collapsedTopics.Contains(topicItem)) return;

            _collapsedTopics.Add(topicItem);

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            var label = new Label(topic);

            var button = new Button(ExpandTopic);
            button.style.width = 25;
            button.text = "X";

            container.Add(button);
            container.Add(label);
            _blackboard.Add(container);

            void ExpandTopic()
            {
                _collapsedTopics.Remove(topicItem);
                _blackboard.Remove(container);

                GenerateTree();
            }
        }

        public void CollapseTopic(string topic)
        {
            InternalCollapse(topic);
            GenerateTree();
        }

        public void CollapseTopics(IEnumerable<GameEventTopic> topics)
        {
            if (topics == null || !topics.Any()) return;
            foreach (var t in topics) InternalCollapse(t);
            GenerateTree();
        }

        public void CollapseTopics(IEnumerable<string> topics) => CollapseTopics(topics.Cast<GameEventTopic>());

        public List<string> GetCollapsedTopics() => _collapsedTopics.Cast<string>().ToList();

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