using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WitcherScriptCompanion.Commands
{
    public class CreateCommand : Command
    {
        private string modName;

        public override bool CanExecute(string[] args) => args.Length.Equals(2) && args[0].Equals("--create");

        public override void Execute()
        {
            try
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var workingPath = Path.Combine(dialog.SelectedPath, modName);

                        if (Directory.Exists(workingPath))
                        {
                            Console.Out.Write(workingPath);

                            return;
                        }

                        Directory.CreateDirectory(workingPath);

                        var filesPath = Path.Combine(workingPath, modName);
                        Directory.CreateDirectory(filesPath);

                        Directory.CreateDirectory(Path.Combine(filesPath, "content"));
                        Directory.CreateDirectory(Path.Combine(filesPath, "scripts"));

                        var mod = new WitcherPackage
                        {
                            Name = modName
                        };

                        File.WriteAllText(Path.Combine(workingPath, "witcher.package.json"), JsonConvert.SerializeObject(mod, Formatting.Indented));

                        Console.Out.Write(workingPath);
                    }
                    else
                    {
                        Console.Error.Write(Errors.PathNotSelected);
                    }
                }
            }
            catch
            {
                Console.Error.Write(Errors.Unexpected);
            }
        }

        public override bool OnCommandExecuting(string[] args)
        {
            modName = args[1];

            return true;
        }
    }
}