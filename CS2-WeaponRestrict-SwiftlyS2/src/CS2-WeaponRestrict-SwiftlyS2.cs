using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared;

namespace CS2_WeaponRestrict_SwiftlyS2;

[PluginMetadata(Id = "CS2_WeaponRestrict_SwiftlyS2", Version = "1.0.0", Name = "CS2-WeaponRestrict-SwiftlyS2",
    Author = "daffyy", Description = "WeaponRestrict for SwiftlyS2")]
public partial class CS2_WeaponRestrict_SwiftlyS2 : BasePlugin
{
    private readonly PluginConfig _config;
    
    private Dictionary<string, float> _weaponQuotas = new();
    private Dictionary<string, int> _weaponLimits = new();

    public CS2_WeaponRestrict_SwiftlyS2(ISwiftlyCore core) : base(core)
    {
        const string configFileName = "config.jsonc";
        const string configSection = "CS2-WeaponRestrict";

        Core.Configuration
            .InitializeJsonWithModel<PluginConfig>(configFileName, configSection)
            .Configure(builder => {
                builder.AddJsonFile(Core.Configuration.GetConfigPath(configFileName), optional: false, reloadOnChange: true);
            });

        ServiceCollection services = new();
        services.AddSwiftly(Core).AddOptionsWithValidateOnStart<PluginConfig>().BindConfiguration(configSection);

        var provider = services.BuildServiceProvider();
        _config = provider.GetRequiredService<IOptions<PluginConfig>>().Value;
    }

    public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
    {
    }

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
    }

    public override void Load(bool hotReload)
    {
        if (hotReload)
        {
            Core.Scheduler.DelayBySeconds(0.1f, LoadMapConfig);
        }
    }

    public override void Unload()
    {
    }
}