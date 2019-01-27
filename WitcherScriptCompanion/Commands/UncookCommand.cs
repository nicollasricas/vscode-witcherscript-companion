using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WitcherScriptCompanion.Events;

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
            EventManager.Send(new ProgressEvent("Uncooking..."));

            var arguments = $"uncook -indir=\"{modPath}\" -outdir=\"{uncookPath}\" -imgfmt={textureFormat}";

            if (dumpSWF)
            {
                arguments += " -dumpswf";
            }

            if (skipErrors)
            {
                arguments += " -skiperrors";
            }

            WCC.Start(modKitPath, arguments);

            Process.Start("explorer.exe", uncookPath);

            EventManager.Send(new DoneEvent());
        }

        public override bool OnExecuting(string[] args)
        {
            modKitPath = Path.GetFullPath(Path.Combine(args[1], "bin/x64"));

            textureFormat = args[2];

            if (!Directory.Exists(modKitPath))
            {
                EventManager.Send(new ErrorEvent(Errors.ModKitPathNotSetted));

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