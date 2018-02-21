using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace WitcherScriptCompanion.Commands
{
    public class CookCommand : Command
    {
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
        private WitcherPackage witcherPackage;
        private string workspacePath;

        public override bool CanExecute(string[] args) => args.Length.Equals(4) && args[0].Equals("--cook");

        public override void Execute()
        {
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

            CopyScripts();
        }

        public override bool OnCommandExecuting(string[] args)
        {
            // wsc --cook gamePath modKitPath workspacePath

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
                cookedPath = Path.Combine(workspacePath, "out", "cooked");
                tempPath = Path.Combine(workspacePath, "out", "tmp");

                Directory.CreateDirectory(tempPath);
                Directory.CreateDirectory(uncookedPath);
                Directory.CreateDirectory(cookedPath);

                outPath = Path.Combine(workspacePath, "out", witcherPackage.Name, "content");

                Directory.CreateDirectory(outPath);

                scriptsPath = Path.Combine(workspacePath, witcherPackage.Name, "scripts");
                contentPath = Path.Combine(workspacePath, witcherPackage.Name, "content");
            }
            catch
            {
                Console.Error.Write(Errors.Unexpected);

                return false;
            }

            return true;
        }

        public override void OnExecuted()
        {
            try
            {
                Directory.Delete(tempPath, true);
                Directory.Delete(uncookedPath, true);
                Directory.Delete(cookedPath, true);

                System.Diagnostics.Process.Start(outPath);
            }
            catch
            {
            }
        }

        private void BuildCacheTexture()
        {
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
            //wcc_lite cook -platform=pc -mod=<mod_dirpath>\Uncooked -basedir=<dirpath>\Uncooked -outdir=<dirpath>\Cooked
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"cook -platform=pc -mod={uncookedPath} -basedir={uncookedPath} -outdir={cookedPath}"
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private void CopyScripts()
        {
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
            var depot = witcherPackage.UseLocalDepot ? "local" : tempPath;

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
            //wcc_lite metadatastore -path=<dirpath>\Packed\modXXX\content
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"metadatastore -path={outPath}"
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private void Pack()
        {
            //wcc_lite pack-dir=<dirpath>\Cooked -outdir=<dirpath>\Packed\modXXX\content
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"pack -dir={contentPath} -outdir={outPath}"
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private void PackCooked()
        {
            //wcc_lite pack-dir=<dirpath>\Cooked -outdir=<dirpath>\Packed\modXXX\content
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = modKitPath,
                    FileName = "wcc_lite.exe",
                    Arguments = $"pack -dir={cookedPath} -outdir={outPath}"
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}