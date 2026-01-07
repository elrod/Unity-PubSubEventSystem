using System;
using System.Collections.Generic;
using System.Linq;

namespace com.elrod.pubsubeventsystem
{
    public class GameEventNode
    {
        private readonly GameEventNode _parent;
        private readonly Dictionary<string, GameEventNode> _subtopics;
        public readonly string Topic;
        
        private event Action<GameEvent> OnEventTriggered;
        
#if UNITY_EDITOR
        public event Action<int> OnSubscribersChanged;
#endif

        public GameEventNode(string eventTopic = "/", GameEventNode parent = null)
        {
            _parent = parent;
            Topic = eventTopic;
            _subtopics = new Dictionary<string, GameEventNode>();
        }
        
        /// <summary>
        ///     Get or create a subtopic
        /// </summary>
        /// <param name="subtopicList">the tokenized list of subtopics to create</param>
        /// <param name="created">whether the node was created or not</param>
        /// <returns>The root of the subtopic</returns>
        public GameEventNode GetOrCreateSubtopic(List<string> subtopicList, ref bool created)
        {
            if (subtopicList.Count == 0) return this;

            var subtopicKey = subtopicList[0];
            if (!_subtopics.ContainsKey(subtopicKey))
            {
                _subtopics[subtopicKey] = new GameEventNode(Topic + subtopicKey + "/", this);
                created = true;
            }

            subtopicList.RemoveAt(0);
            return _subtopics[subtopicKey].GetOrCreateSubtopic(subtopicList, ref created);
        }
        
        public void GetSubtopic(List<string> subtopicList, ref GameEventNode returnNode)
        {
            if (subtopicList.Count == 0)
            {
                returnNode = this;
                return;
            }

            var subtopicKey = subtopicList[0];
            if (!_subtopics.ContainsKey(subtopicKey)) return;
            subtopicList.RemoveAt(0);
            _subtopics[subtopicKey].GetSubtopic(subtopicList, ref returnNode);
        }

        /// <summary>
        ///     Subscribe to topic and all subtopics
        /// </summary>
        /// <param name="onEventTriggeredAction">The action callback for the event</param>
        public void Subscribe(Action<GameEvent> onEventTriggeredAction)
        {
            OnEventTriggered += onEventTriggeredAction;
#if UNITY_EDITOR
            OnSubscribersChanged?.Invoke(GetCurrentSubscriberCount());
#endif
        }

        /// <summary>
        ///     Unsubscribe from topic and all subtopics
        /// </summary>
        /// <param name="onEventTriggeredAction">The action callback for the event</param>
        public void Unsubscribe(Action<GameEvent> onEventTriggeredAction)
        {
            OnEventTriggered -= onEventTriggeredAction;
#if UNITY_EDITOR
            OnSubscribersChanged?.Invoke(GetCurrentSubscriberCount());
#endif
        }
        
        /// <summary>
        ///     Invoke all callbacks registered for the ExergameEventAsset topic starting from leaf to root
        /// </summary>
        /// <param name="gameEvent">The target Game Event</param>
        public void InvokeSubscribersCallbacks(GameEvent gameEvent)
        {
            OnEventTriggered?.Invoke(gameEvent);
            _parent?.InvokeSubscribersCallbacks(gameEvent);
        }
#if UNITY_EDITOR
        /// <summary>
        ///     Get the number of currently subscribed delegates
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSubscriberCount()
        {
            return OnEventTriggered != null ? OnEventTriggered.GetInvocationList().Length : 0;
        }

        /// <summary>
        ///     Get the list of methods subscribed to this node
        /// </summary>
        /// <returns></returns>
        public string[] GetSubscriberNames()
        {
            if (GetCurrentSubscriberCount() == 0) return null;
            var delegates = OnEventTriggered.GetInvocationList();
            var retList = new string[delegates.Length];
            for (var i = 0; i < delegates.Length; i++)
                retList[i] = delegates[i].Method.DeclaringType.Name + "." + delegates[i].Method.Name;
            return retList;
        }
#endif
    }
    
    public class GameEventTree
    {
        public GameEventNode Root { get; }
        
#if UNITY_EDITOR
        private readonly Dictionary<int, HashSet<GameEventTopic>> _layers;
#endif

        public GameEventTree()
        {
            Root = new GameEventNode();
#if UNITY_EDITOR
            _layers = new Dictionary<int, HashSet<GameEventTopic>> { [0] = new HashSet<GameEventTopic> { (GameEventTopic)Root.Topic } };
#endif
        }
        
        public event Action OnTreeChanged;

        /// <summary>
        ///     Subscribe to a topic
        /// </summary>
        /// <param name="topic">the topic</param>
        /// <param name="onEventTriggeredAction">the callback action for when the event is triggered</param>
        public void SubscribeToTopic(string topic, Action<GameEvent> onEventTriggeredAction)
        {
            var nodeToSubscribe = GetOrCreateTopic(topic);
            nodeToSubscribe.Subscribe(onEventTriggeredAction);
        }

        /// <summary>
        ///     Unsubscribe from topic
        /// </summary>
        /// <param name="topic">the topic</param>
        /// <param name="onEventTriggeredAction">the callback action to remove</param>
        public void UnSubscribeFromTopic(string topic, Action<GameEvent> onEventTriggeredAction)
        {
            var nodeToUnSubscribe = GetOrCreateTopic(topic);
            nodeToUnSubscribe.Unsubscribe(onEventTriggeredAction);
        }

        /// <summary>
        ///     Searches for the event topic and invoke callbacks on the corresponding node
        /// </summary>
        /// <param name="eventAsset"></param>
        public void Raise(GameEvent eventAsset)
        {
            var nodeToInvoke = GetOrCreateTopic(eventAsset.Topic);
            nodeToInvoke.InvokeSubscribersCallbacks(eventAsset);
        }

        /// <summary>
        ///     Gets a topic from the tree... if the topic does not exists it creates it
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <returns>The tree node corresponding to the specified topic</returns>
        private GameEventNode GetOrCreateTopic(string topic)
        {
            var tokenizedTopic = topic.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

#if UNITY_EDITOR
            string composedTopic = "";
            for (var i = 0; i < tokenizedTopic.Length; i++)
            {
                composedTopic += "/" + tokenizedTopic[i];
                if (!_layers.ContainsKey(i + 1)) _layers[i + 1] = new();
                _layers[i + 1].Add(new(composedTopic));
            }
#endif
            
            if (tokenizedTopic.Length == 0) return Root;
            var created = false;
            var returnNode = Root.GetOrCreateSubtopic(tokenizedTopic.ToList(), ref created);
            if (created) OnTreeChanged?.Invoke();
            return returnNode;
        }
        
        public GameEventNode GetTopic(string topic)
        {
            var tokenizedTopic = topic.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            if (tokenizedTopic.Length == 0) return Root;

            GameEventNode returnNode = null;
            Root.GetSubtopic(tokenizedTopic.ToList(), ref returnNode);
            return returnNode;
        }
        
#if UNITY_EDITOR
        /// <summary>
        ///     Get the dictionary of tree layers
        /// </summary>
        /// <returns>Returns a dictionary in which each key represents a layer and each value is a set of topics</returns>
        public Dictionary<int, HashSet<GameEventTopic>> GetTreeLayers() => _layers;

        public List<GameEventTopic> GetTreeLeavesParents()
        {
            var result = new List<GameEventTopic>();
            foreach (var kvp in _layers.OrderByDescending(p => p.Key))
            {
                foreach (var topic in kvp.Value)
                {
                    if (topic.IsRoot ||
                        result.Contains(topic) || 
                        result.Contains(topic.Parent) || 
                        result.Any(t => topic.Parent.IsParentOf(t)))
                    {
                        continue;
                    }

                    result.Add(topic.Parent);
                }
            }
            return result;
        }

        /// <summary>
        ///     Get the count of topics for a selected layer
        /// </summary>
        /// <param name="targetLayer">the layer: 0 - root, 1 - root's children, and so on</param>
        /// <returns>the topics count for that layer</returns>
        public int GetNodeCountInLayer(int targetLayer)
        {
            return !_layers.ContainsKey(targetLayer) ? 0 : _layers[targetLayer].Count();
        }

        /// <summary>
        ///     Get the count of topics with a given parent topic.
        /// </summary>
        /// <param name="topic">the parent to look for</param>
        /// <returns>the count of topics with a given parent topic</returns>
        public int GetChildrenCountForTopic(string topic)
        {
            var topicItem = new GameEventTopic(topic);
            return _layers.Select(kvp => kvp.Value.Count(t => topicItem.IsParentOf(t))).Sum();
        }
#endif
    }
}