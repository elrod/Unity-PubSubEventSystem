using System;

namespace com.elrod.pubsubeventsystem
{
    public class GameEventSystem : Singleton<GameEventSystem>
    {
        public GameEventTree EventsTree { get; private set; }

        protected override void OnAwake()
        {
            EventsTree = new GameEventTree();
        }

        /// <summary>
        /// Subscribe to a topic
        /// </summary>
        /// <param name="topic">target topic</param>
        /// <param name="cbk">callback</param>
        public void Subscribe(string topic, Action<GameEvent> cbk) => EventsTree.SubscribeToTopic(topic, cbk);
        /// <summary>
        /// Subscribe to a game event
        /// </summary>
        /// <param name="evt">target game event</param>
        /// <param name="cbk">callback</param>
        public void Subscribe(GameEvent evt, Action<GameEvent> cbk) => Subscribe(evt.Topic, cbk);

        /// <summary>
        /// Unsubscribe from a topic
        /// </summary>
        /// <param name="topic">target topic</param>
        /// <param name="cbk">previously registered callback</param>
        public void Unsubscribe(string topic, Action<GameEvent> cbk) => EventsTree.UnSubscribeFromTopic(topic, cbk);
        /// <summary>
        /// Unsubscribe from a game event
        /// </summary>
        /// <param name="evt">target game event</param>
        /// <param name="cbk">previously subscribed callback</param>
        public void Unsubscribe(GameEvent evt, Action<GameEvent> cbk) => Unsubscribe(evt.Topic, cbk);
        
        /// <summary>
        /// Raise a game event
        /// </summary>
        /// <param name="evt">Game event</param>
        public void Raise(GameEvent evt) => EventsTree.Raise(evt);

    }
}