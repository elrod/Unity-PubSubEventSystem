using com.elrod.pubsubeventsystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace com.elrod.pubsubeventssystem.editor
{
    public class GameEventNodeView : Node
    {
        private readonly Label SubscribersLabel;
        private readonly VisualElement SubscribersListContainer;
        private GameEventNode _associatedNode;
        public string GUID;
        public Port InputPort;
        public bool isRoot;
        public int nodeTreeLayer;
        public Port OutputPort;

        public GameEventNodeView()
        {
            SubscribersLabel = new Label("Subscribers: 0\n------------------");
            SubscribersListContainer = new VisualElement();
            this.Q("contents").Add(SubscribersLabel);
            this.Q("contents").Add(SubscribersListContainer);
        }

        public GameEventNode AssociatedNode
        {
            get => _associatedNode;
            set
            {
                if (_associatedNode != null) _associatedNode.OnSubscribersChanged -= OnSubscriberChanged;
                _associatedNode = value;
                _associatedNode.OnSubscribersChanged += OnSubscriberChanged;
                OnSubscriberChanged(_associatedNode.GetCurrentSubscriberCount());
            }
        }

        public void OnSubscriberChanged(int subscribersCount)
        {
            SubscribersListContainer.Clear();
            SubscribersLabel.text = string.Format("Subscribers: {0}\n------------------", subscribersCount);
            var subscribers = AssociatedNode.GetSubscriberNames();
            if (subscribers != null)
            {
                var tooltipStr = "";
                for (var i = 0; i < subscribers.Length; i++) SubscribersListContainer.Add(new Label(subscribers[i]));
                tooltip = tooltipStr;
            }
        }
    }
}