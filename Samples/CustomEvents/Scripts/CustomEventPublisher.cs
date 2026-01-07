using System.Collections;
using UnityEngine;

namespace com.elrod.pubsubeventsystem.samples
{
    public class CustomEventPublisher : MonoBehaviour
    {
        [SerializeField]
        private EventWithMessage[] _SysEvents;
        
        void Start()
        {
            StartCoroutine(PublishCorot());
        }

        private IEnumerator PublishCorot()
        {
            int i = 0;
            while (true)
            {
                yield return new WaitForSeconds(1);
                _SysEvents[i % _SysEvents.Length].Raise();
                i = i == _SysEvents.Length - 1 ? 0 : i + 1;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}