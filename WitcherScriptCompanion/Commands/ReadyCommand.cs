using System;
using System.Linq;

namespace WitcherScriptCompanion.Commands
{
    public class ReadyCommand : Command
    {
        public override bool CanExecute(string[] args) => args.Any() && args[0].Equals("--ready");

        public override void Execute() => Console.Out.Write("ready");
    }
}