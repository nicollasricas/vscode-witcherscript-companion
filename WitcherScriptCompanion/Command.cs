using System;
using Newtonsoft.Json;

namespace WitcherScriptCompanion
{
    public abstract class Command
    {
        public abstract string Name { get; }

        public static bool Execute(Command command, string[] args)
        {
            if (!command.CanExecute(args))
            {
                return false;
            }

            if (!command.OnExecuting(args))
            {
                return true;
            }

            command.Execute();

            command.OnExecuted();

            return true;
        }

        public abstract bool CanExecute(string[] args);

        public abstract void Execute();

        public virtual void OnExecuted()
        {
        }

        public void SendEvent(Event @event) => Console.Out.WriteLine(JsonConvert.SerializeObject(@event));

        public virtual bool OnExecuting(string[] args) => true;
    }
}