using System;
using UnityEngine;

namespace com.elrod.pubsubeventsystem
{
    [Serializable]
    public class GameEvent
    {
        [SerializeField] 
        private string _Topic;
        
        public string Topic => _Topic;

        /// <summary>
        /// Subscribe cbk to this event
        /// </summary>
        /// <param name="cbk">Callback must receive a game event as a parameter</param>
        public void Subscribe(Action<GameEvent> cbk)
        {
            GameEventSystem.Instance.Subscribe(this, cbk);
        }

        /// <summary>
        /// Unsubscribe cbk from this event
        /// </summary>
        /// <param name="cbk">the callback to unsubscribe</param>
        public void Unsubscribe(Action<GameEvent> cbk)
        {
            GameEventSystem.Instance.Unsubscribe(this, cbk);
        }

        /// <summary>
        /// Raise this event
        /// </summary>
        public void Raise()
        {
            GameEventSystem.Instance.Raise(this);
        }
    }
}

