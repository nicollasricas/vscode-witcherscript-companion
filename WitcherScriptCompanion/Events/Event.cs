using Newtonsoft.Json;
using System;

namespace WitcherScriptCompanion.Events
{
    public enum EventType
    {
        Output = 1,
        Progress = 2,
        Error = 3,
        Result = 4,
        Done = 5,
        Notification = 6
    }

    public abstract class Event
    {
        public EventType Type { get; set; }

        public Event(EventType type) => Type = type;
    }

    public class EventManager
    {
        public static void Send(Event @event) => Console.Out.WriteLine(JsonConvert.SerializeObject(@event));
    }
}