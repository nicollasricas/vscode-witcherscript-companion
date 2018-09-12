using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WitcherScriptCompanion
{
    internal static class Program
    {
        private static readonly List<Command> commands = new List<Command>();

        static Program()
        {
            foreach (var command in Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(m => m.IsSubclassOf(typeof(Command))))
            {
                commands.Add((Command)Activator.CreateInstance(command));
            }
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