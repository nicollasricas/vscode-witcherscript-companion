using System.Diagnostics;
using System.IO;
using WitcherScriptCompanion.Events;

namespace WitcherScriptCompanion
{
    public class WCC
    {
        public static void Start(string modKitPath, string arguments)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = Path.Combine(modKitPath, "wcc_lite.exe"),
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            EventManager.Send(new OutputEvent($"\n\n{process.StartInfo.FileName} {process.StartInfo.Arguments}"));

            process.OutputDataReceived += (s, e) => EventManager.Send(new OutputEvent(e.Data));
            process.ErrorDataReceived += (s, e) => EventManager.Send(new OutputEvent(e.Data));
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}