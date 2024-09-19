namespace FoodShelves;

public class ConfigServer : IModConfig {
    public const string ConfigServerName = "FoodShelvesServer.json";

    public bool EnableVariants { get; set; } = false;

    public ConfigServer(ICoreAPI api, ConfigServer previousConifg = null) {
        if (previousConifg == null) return;

        EnableVariants = previousConifg.EnableVariants;
    }
}
