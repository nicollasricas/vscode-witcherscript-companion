namespace WitcherScriptCompanion.Events
{
    public class ErrorEvent : Event
    {
        public string Message { get; }

        public ErrorEvent(string message) : base(EventType.Error) => Message = message;
    }
}