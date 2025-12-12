using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace CS2_WeaponRestrict_SwiftlyS2;

public partial class CS2_WeaponRestrict_SwiftlyS2
{
    [EventListener<EventDelegates.OnMapLoad>]
    public void OnMapLoadEvent(IOnMapLoadEvent @event)
    {
        LoadMapConfig();
    }
    
    [EventListener<EventDelegates.OnItemServicesCanAcquireHook>]
    public void OnItemServicesCanAcquireHook(IOnItemServicesCanAcquireHookEvent @event)
    {
        var weaponData = @event.WeaponVData;
        if (weaponData == null) return;
        
        var gameRules = Core.EntitySystem.GetGameRules();
        if (gameRules != null)
        {
            if (gameRules.WarmupPeriod)
                return;

            if (_config.AllowPickup && gameRules.BuyTimeEnded && @event.AcquireMethod == AcquireMethod.PickUp)
                return;
        }

        if (!_weaponQuotas.ContainsKey(weaponData.Name.Value) && !_weaponLimits.ContainsKey(weaponData.Name.Value))
            return;
        
        var controller = @event.ItemServices.Pawn?.As<CCSPlayerPawn>().OriginalController.Value;
        if (controller == null)
            return;
        
        var player = Core.PlayerManager.GetPlayer((int)(controller.Index-1));
        
        if (player is not { IsValid: true } || player.Controller.LifeState != (int)LifeState_t.LIFE_ALIVE)
            return;
        
        if (_config.VIPFlag != "" && Core.Permission.PlayerHasPermission(player.SteamID, _config.VIPFlag))
            return;
        
        // Get every valid player that is currently connected
        var players = Core.PlayerManager.GetAllPlayers().Where(p =>
            p.IsValid 
            && (!_config.DoTeamCheck || p.Controller.Team == player.Controller.Team)
        ).ToList();

        int limit = int.MaxValue;
        bool disabled = false;
        
        if (_weaponQuotas.TryGetValue(weaponData.Name.Value, out float cfgQuota))
        {
            limit = Math.Min(limit, cfgQuota > 0f ? (int)(players.Count * cfgQuota) : 0);
            disabled |= cfgQuota == 0f;
        }
            
        if (_weaponLimits.TryGetValue(weaponData.Name.Value, out int cfgLimit))
        {
            limit = Math.Min(limit, cfgLimit);
            disabled |= cfgLimit == 0;
        }
		
        if (!disabled)
        {
            var count = CountWeaponsOnTeam(weaponData.Name.Value, players);
            if (count < limit)
                return;
        }

        // Print chat message if we attempted to buy this weapon
        if (@event.AcquireMethod != AcquireMethod.PickUp)
        {
            var localizer = Core.Translation.GetPlayerLocalizer(player);
                
            string msg = "";
            msg = string.Format(disabled ? $"{_config.PluginTag} {localizer["DisabledMessage", weaponData.Name.Value.Replace("weapon_", "").ToUpper()]}" : $"{_config.PluginTag} {localizer["RestrictMessage", weaponData.Name.Value.Replace("weapon_", "").ToUpper(), limit.ToString()]}");

            if (!string.IsNullOrEmpty(msg))
                player.SendChat(msg.Colored());
            
            @event.SetAcquireResult(AcquireResult.AlreadyOwned);
        }
        else
        {
            @event.SetAcquireResult(AcquireResult.InvalidItem);
        }
    }
}