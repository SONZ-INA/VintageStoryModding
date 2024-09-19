using ConfigLib;
using ImGuiNET;

namespace FoodShelves;

// Totally did NOT steal this from Dana
public class ConfigLibCompatibility {
    public ConfigLibCompatibility(ICoreAPI api) {
        api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(Lang.Get("foodshelves:foodshelvesserver"), (id, buttons) => EditConfigServer(id, buttons, api));
    }

    private void EditConfigServer(string id, ControlButtons buttons, ICoreAPI api) {
        if (buttons.Save) ModConfig.WriteConfig(api, ConfigServer.ConfigServerName, Core.ConfigServer);
        if (buttons.Restore) Core.ConfigServer = ModConfig.ReadConfig<ConfigServer>(api, ConfigServer.ConfigServerName);
        if (buttons.Defaults) Core.ConfigServer = new(api);

        BuildSettingsServer(Core.ConfigServer, id);
    }

    private void BuildSettingsServer(ConfigServer config, string id) {
        config.EnableVariants = OnCheckBox(id, config.EnableVariants, nameof(config.EnableVariants));
    }

    private bool OnCheckBox(string id, bool value, string name) {
        bool newValue = value;
        ImGui.Checkbox(Lang.Get(name) + $"##{name}-{id}", ref newValue);
        return newValue;
    }
}

