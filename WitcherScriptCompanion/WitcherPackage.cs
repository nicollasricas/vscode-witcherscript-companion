using Newtonsoft.Json;

namespace WitcherScriptCompanion
{
    public class WitcherPackage
    {
        public WitcherPackage()
        {
        }

        public WitcherPackage(string modName) => Name = modName;

        [JsonProperty("mod.isDLC")]
        public bool IsDLC { get; set; } = false;

        [JsonProperty("mod.name")]
        public string Name { get; set; }

        [JsonProperty("cook.useLocalDepot")]
        public bool UseLocalDepot { get; set; } = false;

        [JsonProperty("cook.skipErrors")]
        public bool SkipErrors { get; set; } = false;
    }
}