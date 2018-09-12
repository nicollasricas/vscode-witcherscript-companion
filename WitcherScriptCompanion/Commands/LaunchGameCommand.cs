using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WitcherScriptCompanion.Commands
{
    public class LaunchGameCommand : Command
    {
        public override string Name => "--launchgame";

        private bool enableDebug;
        private string gameExecutable;

        public override bool CanExecute(string[] args) => args.Length > 1 && args[0].Equals(Name);

        public override void Execute()
        {
            SendEvent(new Event(EventType.Progress, "Launching Game..."));

            try
            {
                var debugParams = enableDebug ? " -net -debugscripts" : "";

                Process.Start("steam://rungameid/292030" + debugParams);

                Process.Start(gameExecutable + debugParams);

                SendEvent(new Event(EventType.Done));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override bool OnExecuting(string[] args)
        {
            if (Process.GetProcessesByName("Witcher3").Length > 0)
            {
                SendEvent(new Event(EventType.Error, Strings.GameAlreadyRunning));

                return false;
            }

            if (args.Contains("--debug"))
            {
                enableDebug = true;
            }

            gameExecutable = Path.Combine(args[1], "bin/x64/witcher3.exe");

            if (!File.Exists(gameExecutable))
            {
                SendEvent(new Event(EventType.Error, Strings.GameExecutableNotFound));

                return false;
            }

            return true;
        }
    }
}