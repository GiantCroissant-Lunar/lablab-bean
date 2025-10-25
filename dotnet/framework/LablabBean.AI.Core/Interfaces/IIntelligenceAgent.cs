using LablabBean.AI.Core.Models;

namespace LablabBean.AI.Core.Interfaces;

/// <summary>
/// Interface for intelligence agents
/// </summary>
public interface IIntelligenceAgent
{
    Task<AIDecision> GetDecisionAsync(AvatarContext context, AvatarState state, AvatarMemory memory);
    Task<string> GenerateDialogueAsync(DialogueContext context);
    Task InitializeAsync();
}
