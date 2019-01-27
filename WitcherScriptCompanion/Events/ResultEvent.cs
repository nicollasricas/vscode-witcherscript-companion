namespace WitcherScriptCompanion.Events
{
    public class ResultEvent : Event
    {
        public string Data { get; }

        public ResultEvent(string data) : base(EventType.Result) => Data = data;
    }
}