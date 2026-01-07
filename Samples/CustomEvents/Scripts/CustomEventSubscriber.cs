using UnityEngine;

namespace com.elrod.pubsubeventsystem.samples
{
    public class CustomEventSubscriber : MonoBehaviour
    {
        [SerializeField]
        private string _Topic = "/";
        
        private void Start()
        {
            GameEventSystem.Instance.Subscribe(_Topic, OnEventReceived);
        }

        private void OnDestroy()
        {
            GameEventSystem.Instance.Unsubscribe(_Topic, OnEventReceived);
        }

        private void OnEventReceived(GameEvent evt)
        {
            Debug.Log($"Event received: {evt.Topic}");
            if (evt is EventWithMessage evtWithMessage)
            {
                Debug.Log($"Event received with message: {evtWithMessage.Message}");
            }
        }
    }
}