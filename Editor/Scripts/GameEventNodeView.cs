using com.elrod.pubsubeventsystem;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;

namespace com.elrod.pubsubeventssystem.editor
{
    public class GameEventNodeView : Node
    {
        public string GUID;
        public Port InputPort;
        public bool IsRoot;
        public int NodeTreeLayer;
        public Port OutputPort;
        
        private readonly long _deactivationTime = 500;
        private readonly Label _subscribersLabel;
        private readonly VisualElement _subscribersListContainer;
        private readonly StyleColor _originalBackgroundColor;
        private readonly StyleColor _originalColor;
        private GameEventNode _associatedNode;

        private bool _active = false;

        public GameEventNodeView()
        {
            _subscribersLabel = new Label("Subscribers: 0\n------------------");
            _subscribersListContainer = new VisualElement();
            this.Q("contents").Add(_subscribersLabel);
            this.Q("contents").Add(_subscribersListContainer);
            _originalBackgroundColor = this.style.backgroundColor;
            _originalColor = this.style.color;
        }

        public GameEventNode AssociatedNode
        {
            get => _associatedNode;
            set
            {
                if (_associatedNode != null) _associatedNode.OnSubscribersChanged -= OnSubscriberChanged;
                _associatedNode = value;
                _associatedNode.OnSubscribersChanged += OnSubscriberChanged;
                _associatedNode.OnNodeActivated += OnNodeActivated;
                OnSubscriberChanged(_associatedNode.GetCurrentSubscriberCount());
            }
        }
        
        

        private void OnNodeActivated()
        {
            if (_active) return;
            style.backgroundColor = Color.green;
            style.color = Color.black;
            _active = true;
            schedule.Execute(Deactivate).ExecuteLater(_deactivationTime);
        }

        private void Deactivate()
        {
            style.backgroundColor = _originalBackgroundColor;
            style.color = _originalColor;
            _active = false;
        }

        public void OnSubscriberChanged(int subscribersCount)
        {
            _subscribersListContainer.Clear();
            _subscribersLabel.text = string.Format("Subscribers: {0}\n------------------", subscribersCount);
            var subscribers = AssociatedNode.GetSubscriberNames();
            if (subscribers != null)
            {
                var tooltipStr = "";
                for (var i = 0; i < subscribers.Length; i++) _subscribersListContainer.Add(new Label(subscribers[i]));
                tooltip = tooltipStr;
            }
        }
    }
}