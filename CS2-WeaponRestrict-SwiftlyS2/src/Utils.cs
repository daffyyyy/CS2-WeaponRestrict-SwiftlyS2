using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace CS2_WeaponRestrict_SwiftlyS2;

public partial class CS2_WeaponRestrict_SwiftlyS2
{
    private int CountWeaponsOnTeam(string designerName, IEnumerable<IPlayer> players)
    {
        var count = 0;

        foreach (var player in players)
        {
            if ((_config.VIPFlag != "" && Core.Permission.PlayerHasPermission(player.SteamID, _config.VIPFlag))
                || player.Controller.LifeState != (int)LifeState_t.LIFE_ALIVE
                || player.PlayerPawn is not { IsValid: true }
                || player.PlayerPawn.WeaponServices == null)
                continue;

            // Iterate over player weapons
            foreach (CHandle<CBasePlayerWeapon> weapon in player.PlayerPawn.WeaponServices.MyWeapons)
            {
                //Get the item DesignerName and compare it to the counted name
                if (weapon.Value == null || weapon.Value.DesignerName != designerName) continue;
                count++;
            }
        }

        return count;
    }
    
    private void LoadMapConfig()
    {
        // Load map config if exists

        // First check if there is any direct value for the map name in MapConfigs
        if (!_config.MapConfigs.TryGetValue(Core.Engine.GlobalVars.MapName.Value, out var currentMapConfig))
        {
            // If the first check failed, check with regex on every MapConfigs key
            KeyValuePair<string, Dictionary<string, Dictionary<string, float>>> wildcardConfig = _config.MapConfigs.FirstOrDefault(p => Regex.IsMatch(Core.Engine.GlobalVars.MapName.Value, $"^{p.Key}$"));

            // If there is a match, and the properties are not null, set the currentMapConfig variable to the regex match value.
            if (wildcardConfig.Value is { Count: >= 0 })
            {
                currentMapConfig = wildcardConfig.Value;
            }
            else
            {
                // Load the default config
                _weaponLimits = _config.WeaponLimits;
                _weaponQuotas = _config.WeaponQuotas;

                Core.Logger.LogInformation($"WeaponRestrict: Loaded default config for {Core.Engine.GlobalVars.MapName.Value} (Limits: {string.Join(Environment.NewLine, _weaponLimits)}, Quotas: {string.Join(Environment.NewLine, _weaponQuotas)})");
                return;
            }
        };

        if (currentMapConfig.TryGetValue("WeaponQuotas", out var value1))
        {
            _weaponQuotas = value1;
        }
        else
        {
            _weaponQuotas.Clear();
        }

        if (currentMapConfig.TryGetValue("WeaponLimits", out var value))
        {
            // Convert float dict to int dict (stored as float values for simplicity)
            _weaponLimits = value.ToDictionary(k => k.Key, v => (int)v.Value);
        }
        else
        {
            _weaponLimits.Clear();
        }

        Core.Logger.LogInformation($"WeaponRestrict: Loaded map config for {Core.Engine.GlobalVars.MapName.Value} (Limits: {string.Join(Environment.NewLine, _weaponLimits)}, Quotas: {string.Join(Environment.NewLine, _weaponQuotas)})");
    }
}