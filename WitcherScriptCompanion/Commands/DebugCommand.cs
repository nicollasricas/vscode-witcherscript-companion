using System;
using WitcherScriptCompanion.Features;

namespace WitcherScriptCompanion.Commands
{
    public class DebugCommand : Command
    {
        public override string Name => "--debug";

        public override bool CanExecute(string[] args) => args.Length > 0 && args[0].Equals(Name);

        public override void Execute()
        {
            // using for dispose resources
            new DebugFeature();

            Console.WriteLine("Press any key to interrupt the debugging process.");

            Console.ReadKey(true);
        }
    }
}