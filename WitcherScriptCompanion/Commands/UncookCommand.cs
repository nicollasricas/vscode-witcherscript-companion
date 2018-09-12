using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WitcherScriptCompanion.Commands
{
    public class UncookCommand : Command
    {
        public override string Name => "--uncook";

        private string modKitPath;
        private string modPath;
        private string uncookPath;
        private string textureFormat;
        private bool dumpSWF;
        private bool skipErrors;

        public override bool CanExecute(string[] args) => args.Length > 2 && args[0].Equals(Name);

        public override void Execute()
        {
            SendEvent(new Event(EventType.Progress, "Uncooking..."));

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"uncook -indir=\"{modPath}\" -outdir=\"{uncookPath}\" -imgfmt={textureFormat}"
                }
            };

            if (dumpSWF)
            {
                process.StartInfo.Arguments += " -dumpswf";
            }

            if (skipErrors)
            {
                process.StartInfo.Arguments += " -skiperrors";
            }

            process.Start();
            process.WaitForExit();

            Process.Start("explorer.exe", uncookPath);

            SendEvent(new Event(EventType.Done));
        }

        public override bool OnExecuting(string[] args)
        {
            modKitPath = Path.GetFullPath(Path.Combine(args[1], "bin/x64"));

            textureFormat = args[2];

            if (!Directory.Exists(modKitPath))
            {
                SendEvent(new Event(EventType.Error, Errors.ModKitPathNotSetted));

                return false;
            }

            if (args.Contains("--dumpswf"))
            {
                dumpSWF = true;
            }

            if (args.Contains("--skiperrors"))
            {
                skipErrors = true;
            }

            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                dialog.Title = Strings.SelectContentToUncook;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    modPath = dialog.FileName;
                }
                else
                {
                    return false;
                }

                dialog.Title = Strings.SelectPathToSave;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    uncookPath = dialog.FileName;
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