using System;
using System.Linq;
using System.Net;
using System.Reflection;

namespace WitcherScriptCompanion.Commands
{
    public class CheckUpdateCommand : Command
    {
        public override bool CanExecute(string[] args) => args.Any() && args[0].Equals("--checkupdate", StringComparison.InvariantCultureIgnoreCase);

        public override void Execute()
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            try
            {
                using (var webClient = new WebClient())
                {
                    var latestVersion = webClient.DownloadString("https://raw.githubusercontent.com/nicollasricas/vscode-witcherscript-companion/master/update.txt");

                    if (!currentVersion.Trim().Equals(latestVersion.Trim()))
                    {
                        Console.Out.Write($"There's a new update available for Witcher Script Companion, latest version: {latestVersion}, download at https://github.com/nicollasricas/vscode-witcherscript-companion/releases");
                    }
                }
            }
            catch
            {
                Console.Error.Write(Errors.UnableToCheckUpdates);
            }
        }
    }
}