using System;
using System.Diagnostics;
using System.IO;

namespace WitcherScriptCompanion.Commands
{
    public class LaunchCommand : Command
    {
        private string gameExecutablePath;

        public override bool CanExecute(string[] args) => args.Length.Equals(2) && args[0].Equals("--launch");

        public override void Execute()
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo()
                {
                    FileName = "steam://rungameid/292030",
                    UseShellExecute = true
                });

                //If the game version isn't steam try to run it from the executable,
                //it will work with GOG version and cracked (go buy the game, it's worth it, it's not like Ubisoft bugged games.
                System.Diagnostics.Process.Start(new ProcessStartInfo()
                {
                    FileName = gameExecutablePath,
                    UseShellExecute = true
                });
            }
            catch
            {
                Console.Error.Write(Errors.UnableToLaunch);
            }
        }

        public override bool OnCommandExecuting(string[] args)
        {
            if (!Directory.Exists(args[1]))
            {
                Console.Error.Write(Errors.GamePathNotSetted);

                return false;
            }

            gameExecutablePath = Path.Combine(args[1], "bin/x64/witcher3.exe");

            if (!File.Exists(gameExecutablePath))
            {
                Console.Error.Write(Errors.GameExeNotFound);

                return false;
            }

            return true;
        }
    }
}