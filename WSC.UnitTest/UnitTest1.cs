using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace WSC.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LaunchGame()
        {
            var process = Process.Start(new ProcessStartInfo(@"C:\Users\Nicollas\Desktop\vscode-witcherscript\vscode-witcherscript\.wsc\wsc.exe")
            {
                Arguments = "--uncook \"D:/Steam/steamapps/common/The Witcher 3\""
            });

            process.OutputDataReceived += (s, e) => System.Console.WriteLine(e.Data);

            Assert.AreEqual(true, true);
        }
    }
}