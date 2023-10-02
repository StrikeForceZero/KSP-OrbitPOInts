using System;

namespace UnityEngineMock
{
    public class Event
    {
        public EventType type;
        public Vector2 mousePosition;
        public void Use() { throw new NotImplementedException(); }
        public static Event current;
    }
}
