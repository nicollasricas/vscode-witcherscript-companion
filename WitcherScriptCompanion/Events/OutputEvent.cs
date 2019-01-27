namespace WitcherScriptCompanion.Events
{
    public class OutputEvent : Event
    {
        public string Message { get; }

        public OutputEvent(string message) : base(EventType.Output) => Message = message;
    }
}