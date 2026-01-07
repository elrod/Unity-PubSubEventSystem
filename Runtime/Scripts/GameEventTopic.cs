using System;

namespace com.elrod.pubsubeventsystem
{
    public class GameEventTopic : IComparable<GameEventTopic>, IComparable<string>
    {
        private GameEventTopic _parentCache;

        public string Value { get; }
        public string ParentValue { get; }
        public GameEventTopic Parent => _parentCache ??= new(ParentValue);

        public bool IsRoot => ParentValue == null;

        public GameEventTopic(string topic)
        {
            if (string.IsNullOrEmpty(topic)) topic = "/";

            var lastIndexOfSlash = topic.LastIndexOf("/");
            Value = topic;
            ParentValue = topic == "/" || lastIndexOfSlash < 0 ? null : topic.Substring(0, lastIndexOfSlash);
        }

        public int CompareTo(GameEventTopic t) => Value.CompareTo(t.Value);
        public int CompareTo(string s) => Value.CompareTo(s);

        public bool IsParentOf(GameEventTopic t) => t.ParentValue == Value;

        public bool Equals(GameEventTopic t) => t != null && t.Value == Value;

        public bool Equals(string s) => s != null && s == Value;

        public override bool Equals(object obj) => obj is GameEventTopic t && Equals(t) || obj is string s && Equals(s);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;

        public static bool operator ==(GameEventTopic t1, GameEventTopic t2)
        {
            var t1IsNull = ReferenceEquals(t1, null);
            var t2IsNull = ReferenceEquals(t2, null);
            return t1IsNull && t2IsNull || !t1IsNull && t1.Equals(t2);
        }

        public static bool operator !=(GameEventTopic t1, GameEventTopic t2) => !(t1 == t2);

        public static bool operator ==(GameEventTopic t1, string s2)
        {
            var t1IsNull = ReferenceEquals(t1, null);
            var s2IsNull = ReferenceEquals(s2, null);
            return t1IsNull && s2IsNull || !t1IsNull && t1.Equals(s2);
        }

        public static bool operator !=(GameEventTopic t1, string s2) => !(t1 == s2);

        public static implicit operator string(GameEventTopic t) => t.Value;

        public static explicit operator GameEventTopic(string s) => new(s);
    }
}