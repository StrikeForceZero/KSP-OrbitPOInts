using System;

namespace KSPMock
{
    public class EventVoid : BaseGameEvent
    {
        public EventVoid(string eventName) : base(eventName)
        {
        }

        public void Add(EventVoid.OnEvent evt)
        {
            throw new NotImplementedException();
        }

        public void Remove(EventVoid.OnEvent evt)
        {
            throw new NotImplementedException();
        }

        public delegate void OnEvent();
    }
}
