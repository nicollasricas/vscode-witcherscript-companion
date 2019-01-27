using System;
using System.Linq;
using System.Reflection;

namespace WitcherScriptCompanion
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            foreach (var command in Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(m => m.IsSubclassOf(typeof(Command))))
            {
                if (Command.Execute((Command)Activator.CreateInstance(command), args))
                {
                    break;
                }
            }
        }
    }
}