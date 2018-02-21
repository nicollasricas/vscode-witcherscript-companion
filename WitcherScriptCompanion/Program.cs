using System;
using System.Collections.Generic;
using WitcherScriptCompanion.Commands;

namespace WitcherScriptCompanion
{
    internal static class Program
    {
        private static readonly List<Command> commands;

        static Program()
        {
            commands = new List<Command>()
            {
                new ReadyCommand(),
                new LaunchCommand(),
                new CookCommand(),
                //new UncookCommand(),
                new CheckUpdateCommand(),
                new CreateCommand()
            };
        }

        [STAThread]
        private static void Main(string[] args)
        {
            foreach (var command in commands)
            {
                if (Command.Execute(command, args))
                {
                    break;
                }
            }
        }
    }
}