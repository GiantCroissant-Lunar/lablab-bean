namespace LablabBean.Contracts.Input.Mapper;

/// <summary>
/// Input mapper service for mapping raw keyboard input to logical actions.
/// </summary>
public interface IService
{
    /// <summary>
    /// Map a raw key event to a logical action.
    /// </summary>
    /// <param name="rawKey">Raw keyboard event.</param>
    void Map(RawKeyEvent rawKey);

    /// <summary>
    /// Try to get a registered action by name.
    /// </summary>
    /// <param name="actionName">Name of the action.</param>
    /// <param name="action">Output action if found.</param>
    /// <returns>True if action was found, false otherwise.</returns>
    bool TryGetAction(string actionName, out InputAction action);

    /// <summary>
    /// Register a mapping from raw key to action name.
    /// </summary>
    /// <param name="actionName">Name of the action.</param>
    /// <param name="key">Raw key event that triggers the action.</param>
    void RegisterMapping(string actionName, RawKeyEvent key);

    /// <summary>
    /// Unregister a mapping.
    /// </summary>
    /// <param name="actionName">Name of the action to unregister.</param>
    void UnregisterMapping(string actionName);

    /// <summary>
    /// Get all registered action names.
    /// </summary>
    /// <returns>Collection of action names.</returns>
    IReadOnlyCollection<string> GetActionNames();
}
