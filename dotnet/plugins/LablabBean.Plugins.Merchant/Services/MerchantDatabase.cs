using System.Text.Json;
using LablabBean.Plugins.Merchant.Data;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Merchant.Services;

/// <summary>
/// Loads merchant definitions from JSON files.
/// </summary>
public class MerchantDatabase
{
    private readonly Dictionary<Guid, MerchantDefinition> _merchants = new();
    private readonly ILogger _logger;

    public MerchantDatabase(ILogger logger)
    {
        _logger = logger;
    }

    public Dictionary<Guid, MerchantDefinition> Merchants => _merchants;

    public async Task LoadMerchantsAsync(string merchantsDirectory)
    {
        if (!Directory.Exists(merchantsDirectory))
        {
            _logger.LogWarning("Merchants directory not found: {Directory}", merchantsDirectory);
            LoadDefaultMerchants();
            return;
        }

        var jsonFiles = Directory.GetFiles(merchantsDirectory, "*.json");
        foreach (var file in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var merchant = JsonSerializer.Deserialize<MerchantDefinition>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (merchant != null)
                {
                    _merchants[merchant.Id] = merchant;
                    _logger.LogDebug("Loaded merchant: {MerchantName}", merchant.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load merchant from {File}", file);
            }
        }

        _logger.LogInformation("Loaded {Count} merchants", _merchants.Count);
    }

    public MerchantDefinition? GetMerchant(Guid id)
    {
        return _merchants.TryGetValue(id, out var merchant) ? merchant : null;
    }

    private void LoadDefaultMerchants()
    {
        // General Store
        var generalStoreId = Guid.Parse("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d");
        _merchants[generalStoreId] = new MerchantDefinition
        {
            Id = generalStoreId,
            Name = "General Store",
            Description = "Basic supplies and equipment",
            Type = MerchantType.GeneralStore,
            BuyPriceMultiplier = 0.5f,
            SellPriceMultiplier = 1.0f,
            Inventory = new List<MerchantItem>
            {
                new()
                {
                    ItemId = Guid.NewGuid(),
                    ItemName = "Health Potion",
                    Description = "Restores 50 HP",
                    BasePrice = 50,
                    InitialStock = 10,
                    IsInfinite = true,
                    MinLevel = 1,
                    Rarity = ItemRarity.Common
                },
                new()
                {
                    ItemId = Guid.NewGuid(),
                    ItemName = "Mana Potion",
                    Description = "Restores 50 Mana",
                    BasePrice = 50,
                    InitialStock = 10,
                    IsInfinite = true,
                    MinLevel = 1,
                    Rarity = ItemRarity.Common
                },
                new()
                {
                    ItemId = Guid.NewGuid(),
                    ItemName = "Torch",
                    Description = "Provides light in dark places",
                    BasePrice = 10,
                    InitialStock = 20,
                    IsInfinite = true,
                    MinLevel = 1,
                    Rarity = ItemRarity.Common
                }
            }
        };

        // Blacksmith
        var blacksmithId = Guid.Parse("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e");
        _merchants[blacksmithId] = new MerchantDefinition
        {
            Id = blacksmithId,
            Name = "Blacksmith",
            Description = "Weapons and armor",
            Type = MerchantType.Blacksmith,
            BuyPriceMultiplier = 0.4f,
            SellPriceMultiplier = 1.2f,
            Inventory = new List<MerchantItem>
            {
                new()
                {
                    ItemId = Guid.NewGuid(),
                    ItemName = "Iron Sword",
                    Description = "+5 Attack",
                    BasePrice = 100,
                    InitialStock = 3,
                    IsInfinite = false,
                    MinLevel = 1,
                    Rarity = ItemRarity.Common
                },
                new()
                {
                    ItemId = Guid.NewGuid(),
                    ItemName = "Iron Shield",
                    Description = "+5 Defense",
                    BasePrice = 100,
                    InitialStock = 3,
                    IsInfinite = false,
                    MinLevel = 1,
                    Rarity = ItemRarity.Common
                },
                new()
                {
                    ItemId = Guid.NewGuid(),
                    ItemName = "Steel Sword",
                    Description = "+10 Attack",
                    BasePrice = 250,
                    InitialStock = 2,
                    IsInfinite = false,
                    MinLevel = 3,
                    Rarity = ItemRarity.Uncommon
                }
            }
        };

        _logger.LogInformation("Loaded {Count} default merchants", _merchants.Count);
    }
}
