namespace WitcherScriptCompanion.Events
{
    public class ProgressEvent : Event
    {
        public string Message { get; }

        public ProgressEvent(string message) : base(EventType.Progress) => Message = message;
    }
}