namespace WitcherScriptCompanion.Commands
{
    public class UncookCommand : Command
    {
        public override bool CanExecute(string[] args) => args.Length > 1 && args[0].Equals("--uncook");

        public override void Execute()
        {
        }
    }
}