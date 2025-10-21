namespace LablabBean.Contracts.Game.Models;

/// <summary>
/// Game start options.
/// </summary>
public record GameStartOptions(
    string Difficulty,
    int? Seed,
    string? PlayerName
);
