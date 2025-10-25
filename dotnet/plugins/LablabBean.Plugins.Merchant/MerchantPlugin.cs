using Arch.Core;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Merchant.Services;
using LablabBean.Plugins.Merchant.Systems;
using LablabBean.Plugins.NPC.Services;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Merchant;

/// <summary>
/// Merchant & Trading plugin - provides economy system with buying/selling items for gold.
/// </summary>
public class MerchantPlugin : IPlugin
{
    private IPluginContext? _context;
    private World? _world;
    private MerchantDatabase? _merchantDatabase;
    private MerchantSystem? _merchantSystem;
    private TradingSystem? _tradingSystem;
    private MerchantService? _merchantService;

    public string Id => "merchant";
    public string Name => "Merchant System";
    public string Version => "1.0.0";

    public async Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _context = context;
        _context.Logger.LogInformation("Initializing Merchant plugin...");

        if (!context.Registry.IsRegistered<World>())
        {
            _context.Logger.LogWarning("Merchant plugin: World service not found; initializing in passive mode.");
            return;
        }

        var worldService = context.Registry.Get<World>();
        _world = worldService ?? throw new InvalidOperationException("World service not found");

        _merchantDatabase = new MerchantDatabase(_context.Logger);

        var pluginDir = Path.GetDirectoryName(typeof(MerchantPlugin).Assembly.Location);
        var merchantsDir = Path.Combine(pluginDir ?? ".", "Data", "Merchants");

        await _merchantDatabase.LoadMerchantsAsync(merchantsDir);

        _merchantSystem = new MerchantSystem(_world, _context.Logger);
        _tradingSystem = new TradingSystem(_world, _context.Logger, _merchantSystem);
        _merchantService = new MerchantService(
            _world,
            _context.Logger,
            _merchantDatabase.Merchants,
            _tradingSystem,
            _merchantSystem);

        context.Registry.Register(_merchantService);
        context.Registry.Register(_merchantDatabase);

        // Check for NPC integration
        try
        {
            if (context.Registry.IsRegistered<NPCService>())
            {
                var npc = context.Registry.Get<NPCService>();
                if (npc != null)
                {
                    _context.Logger.LogInformation("Merchant plugin: NPCService detected. Trading hooks can be enabled.");
                }
            }
            else
            {
                _context.Logger.LogInformation("Merchant plugin: NPCService not found. Running without NPC dialogue integration.");
            }
        }
        catch (Exception ex)
        {
            _context.Logger.LogWarning(ex, "Merchant plugin optional NPC integration failed");
        }

        _context.Logger.LogInformation(
            "Merchant plugin initialized with {MerchantCount} merchants",
            _merchantDatabase.Merchants.Count);
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _context?.Logger.LogInformation("Merchant plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _context?.Logger.LogInformation("Merchant plugin stopped");
        return Task.CompletedTask;
    }

    public void RefreshAllMerchants(int currentLevel)
    {
        _merchantSystem?.RefreshAllMerchants(currentLevel);
    }
}
