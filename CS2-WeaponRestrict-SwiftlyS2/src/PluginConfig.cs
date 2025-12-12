namespace CS2_WeaponRestrict_SwiftlyS2;

public class PluginConfig
{
    public string PluginTag { get; set; } = "[WeaponRestrict]";

    public Dictionary<string, float> WeaponQuotas { get; set; } = new()
    {
    };

    public Dictionary<string, int> WeaponLimits { get; set; } = new()
    {
        ["weapon_awp"] = 3,
        ["weapon_negev"] = 2,
        ["weapon_scar20"] = 0,
        ["weapon_g3sg1"] = 0,
        ["weapon_m249"] = 2
    };

    public bool DoTeamCheck { get; set; } = true;
    public bool AllowPickup { get; set; } = false;
    public string VIPFlag { get; set; } = "vip.weapons";

    public Dictionary<string, Dictionary<string, Dictionary<string, float>>> MapConfigs { get; set; } = new()
    {
        ["de_dust2"] = new Dictionary<string, Dictionary<string, float>>()
        {
            ["WeaponQuotas"] = new(),
            ["WeaponLimits"] = new(),
            ["awp.*"] = new()
        }
    };
}