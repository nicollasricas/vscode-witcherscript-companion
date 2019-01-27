using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System.IO;
using WitcherScriptCompanion.Events;

namespace WitcherScriptCompanion.Commands
{
    public class NewModCommand : Command
    {
        public override string Name => "--newmod";

        private string modName;
        private string selectedPath;

        public override bool CanExecute(string[] args) => args.Length.Equals(2) && args[0].Equals(Name);

        public override void Execute()
        {
            try
            {
                var workingPath = Path.Combine(selectedPath, modName);

                if (Directory.Exists(workingPath))
                {
                    EventManager.Send(new ResultEvent(workingPath));

                    return;
                }

                Directory.CreateDirectory(workingPath);

                var filesPath = Path.Combine(workingPath, modName);

                Directory.CreateDirectory(filesPath);

                Directory.CreateDirectory(Path.Combine(filesPath, "content"));

                Directory.CreateDirectory(Path.Combine(filesPath, "scripts"));

                File.WriteAllText(Path.Combine(workingPath, "witcher.package.json"), JsonConvert.SerializeObject(new WitcherPackage(modName), Formatting.Indented));

                EventManager.Send(new ResultEvent(workingPath));
            }
            catch
            {
                throw;
            }
        }

        public override bool OnExecuting(string[] args)
        {
            modName = args[1];

            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = Strings.ModPathToCreate;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    selectedPath = dialog.FileName;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}