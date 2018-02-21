using System;
using System.Linq;

namespace WitcherScriptCompanion.Commands
{
    public class CheckUpdateCommand : Command
    {
        public override bool CanExecute(string[] args) => args.Any() && args[0].Equals("--checkupdate", StringComparison.InvariantCultureIgnoreCase);

        public override void Execute()
        {
            Console.Out.Write("WIP");
        }
    }
}