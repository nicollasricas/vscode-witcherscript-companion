using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace WitcherScriptCompanion.Commands
{
    public class CookCommandd : Command
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

        public override bool CanExecute(string[] args) => args.Length > 3 && args[0].Equals(Name);

        public override void Execute()
        {
            SendEvent(new Event(EventType.Progress, "Cooking..."));

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

            SendEvent(new Event(EventType.Done));
        }

        public void ModMerger()
        {
            if (!string.IsNullOrEmpty(modMergerPath))
            {
                SendEvent(new Event(EventType.Progress, "Waiting for merge scripts..."));

                var process = new Process()
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

                var witcherPackagePath = Path.Combine(workspacePath, "witcher.package.json");

                if (!Directory.Exists(workspacePath) || !File.Exists(witcherPackagePath))
                {
                    Console.Error.Write(Errors.ModNotCreated);

                    return false;
                }

                witcherPackage = JsonConvert.DeserializeObject<WitcherPackage>(File.ReadAllText(witcherPackagePath, Encoding.UTF8));

                uncookedPath = Path.Combine(workspacePath, "out", "uncooked");

                CreateOrEraseDirectory(uncookedPath);

                cookedPath = Path.Combine(workspacePath, "out", "cooked");

                CreateOrEraseDirectory(cookedPath);

                tempPath = Path.Combine(workspacePath, "out", "tmp");

                CreateOrEraseDirectory(tempPath);

                outPath = Path.Combine(gamePath, "mods", witcherPackage.Name, "content");

                CreateOrEraseDirectory(outPath);

                scriptsPath = Path.Combine(workspacePath, witcherPackage.Name, "scripts");
                contentPath = Path.Combine(workspacePath, witcherPackage.Name, "content");

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

        public override void OnExecuted() => CreateOrEraseDirectory(Path.Combine(workspacePath, "out"));

        private void BuildCacheTexture()
        {
            SendEvent(new Event(EventType.Progress, "Building texture cache..."));

            //wcc_lite buildcache textures -basedir=<dirpath>\Uncooked -platform=pc -db=<dirpath>\Cooked\cook.db -out=<dirpath>\Packed\modXXX\content\texture.cache

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"buildcache textures -basedir=\"{uncookedPath}\" -platform=pc -db=\"{Path.Combine(cookedPath, "cook.db")}\" -out=\"{Path.Combine(outPath, "texture.cache")}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private void Cook()
        {
            SendEvent(new Event(EventType.Progress, "Cooking..."));

            //wcc_lite cook -platform=pc -mod=<mod_dirpath>\Uncooked -basedir=<dirpath>\Uncooked -outdir=<dirpath>\Cooked
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"cook -platform=pc -mod=\"{uncookedPath}\" -basedir=\"{uncookedPath}\" -outdir=\"{cookedPath}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private void CopyScripts()
        {
            SendEvent(new Event(EventType.Progress, "Copying scripts..."));

            foreach (var scriptPath in Directory.EnumerateFiles(scriptsPath, "*.ws", SearchOption.AllDirectories))
            {
                var newScriptPath = Path.Combine(outPath, "scripts", scriptPath.Replace(scriptsPath, "").Substring(1));

                Directory.CreateDirectory(Path.GetDirectoryName(newScriptPath));

                File.Copy(scriptPath, newScriptPath, true);
            }
        }

        private bool HasScripts() => Directory.EnumerateFiles(scriptsPath, "*.ws", SearchOption.AllDirectories).Any();

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
            var depot = witcherPackage.UseLocalDepot ? "local" : tempPath;

            SendEvent(new Event(EventType.Progress, "Importing textures..."));

            foreach (var texturePath in Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories)
                .Where(m => texturesExtensions.Any(k => Path.GetExtension(m) == k)))
            {
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = modKitPath,
                        FileName = "wcc_lite.exe",
                        Arguments = $"import -depot=\"{depot}\" -file=\"{texturePath}\" -out=\"{Path.ChangeExtension(texturePath.Replace(contentPath, uncookedPath), "xbm")}\""
                    }
                };

                process.Start();
                process.WaitForExit();
            }

            SendEvent(new Event(EventType.Progress, "Importing meshes..."));

            foreach (var meshPath in Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories)
                .Where(m => meshesExtensions.Any(k => Path.GetExtension(m) == k)))
            {
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = modKitPath,
                        FileName = "wcc_lite.exe",
                        Arguments = $"import -depot=\"{depot}\" -file=\"{meshPath}\" -out=\"{Path.ChangeExtension(meshPath.Replace(contentPath, uncookedPath), "w2mesh")}\""
                    }
                };

                process.Start();
                process.WaitForExit();
            }
        }

        private void Metadata()
        {
            SendEvent(new Event(EventType.Progress, "Generating metadata..."));

            //wcc_lite metadatastore -path=<dirpath>\Packed\modXXX\content
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"metadatastore -path=\"{outPath}\"",
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private void Pack()
        {
            SendEvent(new Event(EventType.Progress, "Packing files..."));

            //wcc_lite pack-dir=<dirpath>\Cooked -outdir=<dirpath>\Packed\modXXX\content
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"pack -dir=\"{contentPath}\" -outdir=\"{outPath}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private void PackCooked()
        {
            SendEvent(new Event(EventType.Progress, "Packing files..."));

            //wcc_lite pack-dir=<dirpath>\Cooked -outdir=<dirpath>\Packed\modXXX\content
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"pack -dir=\"{cookedPath}\" -outdir=\"{outPath}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}