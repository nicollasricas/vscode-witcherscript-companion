using Newtonsoft.Json;

namespace WitcherScriptCompanion
{
    public class WitcherPackage
    {
        [JsonProperty(PropertyName = "mod.isDLC")]
        public bool IsDLC { get; set; } = false;

        [JsonProperty(PropertyName = "mod.name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "cook.useLocalDepot")]
        public bool UseLocalDepot { get; set; } = false;
    }
}