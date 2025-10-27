namespace LablabBean.Contracts.Game.UI;

/// <summary>
/// Game-specific UI interface for Dungeon Crawler features.
/// Handles HUD, dialogue, quest, inventory panels, and camera follow.
/// </summary>
public interface IDungeonCrawlerUI
{
    /// <summary>
    /// Toggle HUD visibility.
    /// </summary>
    void ToggleHud();

    /// <summary>
    /// Show or hide the dialogue panel.
    /// </summary>
    void ShowDialogue(string speaker, string text, string[]? choices = null);

    /// <summary>
    /// Hide the dialogue panel.
    /// </summary>
    void HideDialogue();

    /// <summary>
    /// Show or hide the quest panel.
    /// </summary>
    void ShowQuests();

    /// <summary>
    /// Hide the quest panel.
    /// </summary>
    void HideQuests();

    /// <summary>
    /// Show or hide the inventory panel.
    /// </summary>
    void ShowInventory();

    /// <summary>
    /// Hide the inventory panel.
    /// </summary>
    void HideInventory();

    /// <summary>
    /// Update HUD with current player stats.
    /// </summary>
    void UpdatePlayerStats(int health, int maxHealth, int mana, int maxMana, int level, int experience);

    /// <summary>
    /// Set camera to follow an entity.
    /// </summary>
    void SetCameraFollow(int entityId);
}
