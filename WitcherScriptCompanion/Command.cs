namespace WitcherScriptCompanion
{
    public abstract class Command
    {
        public static bool Execute(Command command, string[] args)
        {
            if (!command.CanExecute(args))
            {
                return false;
            }

            if (!command.OnCommandExecuting(args))
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

        public virtual bool OnCommandExecuting(string[] args) => true;
    }
}