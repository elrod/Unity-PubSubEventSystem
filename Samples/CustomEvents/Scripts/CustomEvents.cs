using System;
using UnityEngine;

namespace com.elrod.pubsubeventsystem.samples
{
    [Serializable]
    public class EventWithMessage : GameEvent
    {
        [field: SerializeField] public string Message { get; set; }
    }
}
