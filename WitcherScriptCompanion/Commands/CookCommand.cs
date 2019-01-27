using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WitcherScriptCompanion.Events;

namespace WitcherScriptCompanion.Commands
{
    public class MergeCooked : Command
    {
        public override string Name => "--mergecooked";

        public override bool CanExecute(string[] args) => args.Length > 2 && args[0].Equals(Name);

        public override void Execute()
        {
            string tmpPath = Path.Combine(@"E:\Temp\", Guid.NewGuid().ToString());
            string uncookPath = Path.Combine(tmpPath, "Uncook");
            string uncookLogPath = Path.Combine(tmpPath, "Uncook.txt");
            string cookingPath = Path.Combine(tmpPath, "Cooking");

            if (!Directory.Exists(tmpPath))
            {
                Directory.CreateDirectory(tmpPath);
            }

            if (!Directory.Exists(uncookPath))
            {
                Directory.CreateDirectory(uncookPath);
            }

            string modsPath = @"E:\Steam\steamapps\common\The Witcher 3\mods";

            string[] mods = new[]
            {
                "modHorseDustAndControls",
                "modNextGenHayStack",
                "modPiecesofFilth",
                "modTrissAppearanceOverhaulTattoos",
                "modCharacterFaces",
                "modCharacterTextures",
                "modEyes4Everyone",
                "modRealisticEyes",
                "modRealisticEyes_BloodAndWine",
                "modNewGeraltBoat_v2",
                "mod4kGrassTextures",
                "modNewBeautifulGrass",
                "modTheButcherOfBlaviken1_9Witheyes",
                "modWolfMedallion_Rounded",
                "modNoScreenFX",
                "modE3Flares",
                "modGeraltCloak",
                "modAdditionalArmorStands",
                "modAdditionalStashLocations",
                "modLampOnBoat_NGB",
                "modsezonburz",
                "mod_FixCompleteAnimations",
                "modHQFacesV3",
                "modYenneferPubes",
                "modyenoutfit_v7"
            };

            FileStream uncookLogStream = File.Create(uncookLogPath);

            TextWriterTraceListener logTracer = new TextWriterTraceListener(uncookLogStream);

            Trace.Listeners.Add(logTracer);

            string modName = "mod000_MergedMods";

            string cookWorkspacePath = Path.Combine(cookingPath, modName);
            string cookingMoveLocation = Path.Combine(cookWorkspacePath, modName, "content");

            Directory.CreateDirectory(Path.Combine(cookWorkspacePath, modName, "scripts"));

            Directory.CreateDirectory(cookingMoveLocation);

            foreach (string mod in mods)
            {
                string cookedModPath = Path.Combine(modsPath, mod);

                string modContentPath = Path.Combine(cookedModPath, "content");

                Console.WriteLine("[UNCOOKING :: " + mod);
                string uncookModPath = Path.Combine(uncookPath, mod);
                // var uncookModPathContent = Path.Combine(uncookModPath, "content");

                if (!Directory.Exists(uncookModPath))
                {
                    Directory.CreateDirectory(uncookModPath);
                }

                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = Path.GetFullPath(Path.Combine(@"E:\Modding\TheWitcher3\ModKit", "bin/x64")),
                        FileName = Path.Combine(Path.GetFullPath(Path.Combine(@"E:\Modding\TheWitcher3\ModKit", "bin/x64")), "wcc_lite.exe"),
                        Arguments = $"uncook -indir=\"{modContentPath}\" -outdir=\"{uncookModPath}\" -imgfmt=tga",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.OutputDataReceived += (s, e) =>
                {
                    Console.WriteLine(e.Data);
                    logTracer.WriteLine(e.Data);
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    Console.WriteLine(e.Data);
                    logTracer.WriteLine(e.Data);
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                Console.WriteLine("[QUICKBMS] :: " + mod);

                //quickbms.exe - o - f "*.bundle" witcher3.bms $mod_content_folder$ $mod_content_folder$

                var bmsFile = @"E:\Modding\TheWitcher3\QuickBMS\witcher3.bms";

                foreach (string bundle in Directory.EnumerateFiles(modContentPath, "*.bundle", SearchOption.TopDirectoryOnly))
                {
                    Console.WriteLine("[QUICKBMS BUNDLE FILE] :: " + bundle);

                    var process2 = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            WorkingDirectory = Path.GetFullPath(@"E:\Modding\TheWitcher3\QuickBMS"),
                            FileName = Path.Combine(Path.GetFullPath(@"E:\Modding\TheWitcher3\QuickBMS"), "quickbms.exe"),
                            //Arguments = string.Format(@"-o -f \"{}.bundle\" ")
                            Arguments = $"-o \"{bmsFile}\" \"{bundle}\" \"{uncookModPath}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };

                    process2.OutputDataReceived += (s, e) =>
                    {
                        Console.WriteLine(e.Data);
                    };

                    process2.ErrorDataReceived += (s, e) =>
                    {
                        Console.WriteLine(e.Data);
                    };

                    process2.Start();

                    process2.BeginOutputReadLine();
                    process2.BeginErrorReadLine();

                    process2.WaitForExit();
                }

                Console.WriteLine("[MOVING FILES] :: " + mod);

                foreach (string file in Directory.EnumerateFiles(uncookModPath, "*.*", SearchOption.AllDirectories))
                {
                    string newScriptPath = Path.Combine(cookingMoveLocation, file.Replace(uncookModPath, "").Substring(1));

                    Directory.CreateDirectory(Path.GetDirectoryName(newScriptPath));

                    if (File.Exists(newScriptPath))
                    {
                        Console.WriteLine("[IGNORED]" + newScriptPath);
                    }
                    else
                    {
                        try
                        {
                            File.Copy(file, newScriptPath, false);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            Trace.Flush();

            uncookLogStream.Close();

            Process.Start("explorer.exe", uncookPath);

            Console.WriteLine("Job done!");

            Console.ReadLine();

            string gamePath = @"E:\Steam\steamapps\common\The Witcher 3";

            Command.Execute(new CookCommand(), new[] { "--cook", gamePath, @"E:\Modding\TheWitcher3\ModKit", cookWorkspacePath });

            Console.WriteLine("Job done 2!");

            Console.ReadLine();

            Directory.Delete(tmpPath, true);

            //var process = new Process()
            //{
            //    StartInfo = new ProcessStartInfo()
            //    {
            //        WorkingDirectory = modKitPath,
            //        FileName = "wcc_lite.exe",
            //        Arguments = $"uncook -indir=\"{modPath}\" -outdir=\"{uncookPath}\" -imgfmt={textureFormat}"
            //    }
            //};
        }
    }

    public class CookCommand : Command
    {
        public override string Name => "--cook";

        private WitcherPackage witcherPackage;

        private readonly string[] meshesExtensions = { ".fbx;", ".re" };
        private readonly string[] texturesExtensions = { ".dds", ".bmp", ".jpg", ".tga", ".png" };

        private string contentPath;
        private string cookedPath;
        private string gamePath;
        private string modKitPath;
        private string outPath;
        private string scriptsPath;
        private string tempPath;
        private string uncookedPath;
        private string workspacePath;
        private string modMergerPath;

        public override bool CanExecute(string[] args)
        {
            return args.Length > 3 && args[0].Equals(Name);
        }

        public override void Execute()
        {
            EventManager.Send(new ProgressEvent("Cooking..."));

            if (HasTextures() || HasMeshes())
            {
                Import();

                Cook();

                BuildCacheTexture();

                PackCooked();
            }
            else
            {
                Pack();
            }

            Metadata();

            if (HasScripts())
            {
                CopyScripts();
            }

            ModMerger();

            EventManager.Send(new DoneEvent());
        }

        public void ModMerger()
        {
            if (!string.IsNullOrEmpty(modMergerPath))
            {
                EventManager.Send(new ProgressEvent("Waiting Script Merger to exit..."));

                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = modMergerPath,
                        FileName = "WitcherScriptMerger.exe"
                    }
                };

                process.Start();
                process.WaitForExit();
            }
        }

        public override bool OnExecuting(string[] args)
        {
            // wsc --cook gamePath modKitPath workspacePath modMergerPath

            try
            {
                gamePath = Path.GetFullPath(args[1]);

                if (!Directory.Exists(gamePath))
                {
                    Console.Error.Write(Errors.GamePathNotSetted);

                    return false;
                }

                modKitPath = Path.GetFullPath(Path.Combine(args[2], "bin/x64"));

                if (!Directory.Exists(modKitPath))
                {
                    Console.Error.Write(Errors.ModKitPathNotSetted);

                    return false;
                }

                workspacePath = Path.GetFullPath(args[3]);

                string witcherPackagePath = Path.Combine(workspacePath, "witcher.package.json");

                if (!Directory.Exists(workspacePath) || !Directory.Exists(Path.Combine(workspacePath, Path.GetDirectoryName(workspacePath))))
                {
                    Console.Error.Write(Errors.ModNotCreated);

                    return false;
                }

                string modName = Path.GetFileName(workspacePath);

                if (File.Exists(witcherPackagePath))
                {
                    witcherPackage = JsonConvert.DeserializeObject<WitcherPackage>(File.ReadAllText(witcherPackagePath, Encoding.UTF8));

                    modName = witcherPackage.Name;
                }

                scriptsPath = Path.Combine(workspacePath, modName, "scripts");
                contentPath = Path.Combine(workspacePath, modName, "content");

                outPath = Path.Combine(gamePath, "mods", modName, "content");
                CreateOrEraseDirectory(outPath);

                uncookedPath = Path.Combine(workspacePath, "out", "uncooked");

                CreateOrEraseDirectory(uncookedPath);

                cookedPath = Path.Combine(workspacePath, "out", "cooked");

                CreateOrEraseDirectory(cookedPath);

                tempPath = Path.Combine(workspacePath, "out", "tmp");

                CreateOrEraseDirectory(tempPath);

                if (args.Length == 5 && !string.IsNullOrEmpty(args[4]))
                {
                    modMergerPath = args[4];
                }
            }
            catch
            {
                throw;
            }

            return true;
        }

        private void CreateOrEraseDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        public override void OnExecuted()
        {
            CreateOrEraseDirectory(Path.Combine(workspacePath, "out"));
        }

        private void BuildCacheTexture()
        {
            EventManager.Send(new ProgressEvent("Building texture cache..."));

            var arguments = $"buildcache textures -basedir=\"{uncookedPath}\" -platform=pc -db=\"{Path.Combine(cookedPath, "cook.db")}\" -out=\"{Path.Combine(outPath, "texture.cache")}\"";

            WCC.Start(modKitPath, arguments);
        }

        private void Cook()
        {
            EventManager.Send(new ProgressEvent("Cooking..."));

            var arguments = $"cook -platform=pc -mod=\"{uncookedPath}\" -basedir=\"{uncookedPath}\" -outdir=\"{cookedPath}\"";

            WCC.Start(modKitPath, arguments);
        }

        private void CopyScripts()
        {
            EventManager.Send(new ProgressEvent("Copying scripts..."));

            foreach (string scriptPath in Directory.EnumerateFiles(scriptsPath, "*.ws", SearchOption.AllDirectories))
            {
                string newScriptPath = Path.Combine(outPath, "scripts", scriptPath.Replace(scriptsPath, "").Substring(1));

                Directory.CreateDirectory(Path.GetDirectoryName(newScriptPath));

                File.Copy(scriptPath, newScriptPath, true);
            }
        }

        private bool HasScripts()
        {
            return Directory.EnumerateFiles(scriptsPath, "*.ws", SearchOption.AllDirectories).Any();
        }

        private bool HasMeshes()
        {
            return Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories)
                .Where(m => meshesExtensions.Any(k => Path.GetExtension(m) == k))
                .Any();
        }

        private bool HasTextures()
        {
            return Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories)
                .Where(m => texturesExtensions.Any(k => Path.GetExtension(m) == k))
                .Any();
        }

        private void Import()
        {
            string depot = witcherPackage != null && witcherPackage.UseLocalDepot ? "local" : tempPath;

            EventManager.Send(new ProgressEvent("Importing textures..."));

            foreach (string texturePath in Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories)
                .Where(m => texturesExtensions.Any(k => Path.GetExtension(m) == k)))
            {
                var arguments = $"import -depot=\"{depot}\" -file=\"{texturePath}\" -out=\"{Path.ChangeExtension(texturePath.Replace(contentPath, uncookedPath), "xbm")}\"";

                WCC.Start(modKitPath, arguments);
            }

            EventManager.Send(new ProgressEvent("Importing meshes..."));

            foreach (string meshPath in Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories)
                .Where(m => meshesExtensions.Any(k => Path.GetExtension(m) == k)))
            {
                var arguments = $"import -depot=\"{depot}\" -file=\"{meshPath}\" -out=\"{Path.ChangeExtension(meshPath.Replace(contentPath, uncookedPath), "w2mesh")}\"";

                WCC.Start(modKitPath, arguments);
            }
        }

        private void Metadata()
        {
            EventManager.Send(new ProgressEvent("Generating metadata..."));

            var arguments = $"metadatastore -path=\"{outPath}\"";

            WCC.Start(modKitPath, arguments);
        }

        private void Pack()
        {
            EventManager.Send(new ProgressEvent("Packing files..."));

            var arguments = $"pack -dir=\"{contentPath}\" -outdir=\"{outPath}\"";

            WCC.Start(modKitPath, arguments);
        }

        private void PackCooked()
        {
            EventManager.Send(new ProgressEvent("Packing files..."));

            var arguments = $"pack -dir=\"{cookedPath}\" -outdir=\"{outPath}\"";

            WCC.Start(modKitPath, arguments);
        }
    }
}