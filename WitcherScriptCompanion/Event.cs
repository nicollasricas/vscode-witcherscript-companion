namespace WitcherScriptCompanion
{
    public enum EventType
    {
        Output = 1,
        Progress = 2,
        Error = 3,
        Result = 4,
        Done = 5
    }

    public class Event
    {
        public EventType Type { get; }

        public string Data { set; get; }

        public Event(EventType type) => Type = type;

        public Event(EventType type, string data)
        {
            Type = type;
            Data = data;
        }
    }
}