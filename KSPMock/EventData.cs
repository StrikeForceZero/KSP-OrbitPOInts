using System;

namespace KSPMock
{
    public class EventData<T> : BaseGameEvent
    {
        public EventData(string eventName)
            : base(eventName)
        {

        }

        public void Add(EventData<T>.OnEvent evt)
        {
            throw new NotImplementedException();
        }

        public void Remove(EventData<T>.OnEvent evt)
        {
            throw new NotImplementedException();
        }

        public delegate void OnEvent(T data);
    }
}
