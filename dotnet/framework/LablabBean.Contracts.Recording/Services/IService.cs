using System.Collections.Generic;

namespace LablabBean.Contracts.Recording.Services;

/// <summary>
/// Base service interface for recording services
/// </summary>
public interface IService
{
    /// <summary>
    /// Execute a recording action
    /// </summary>
    /// <typeparam name="TResult">Return type</typeparam>
    /// <param name="actionName">Action name</param>
    /// <param name="parameters">Action parameters</param>
    /// <returns>Action result</returns>
    TResult ExecuteAction<TResult>(string actionName, params object[] parameters);

    /// <summary>
    /// Execute a recording action without return value
    /// </summary>
    /// <param name="actionName">Action name</param>
    /// <param name="parameters">Action parameters</param>
    void ExecuteAction(string actionName, params object[] parameters);

    /// <summary>
    /// Check if an action is supported
    /// </summary>
    /// <param name="actionName">Action name</param>
    /// <returns>True if supported</returns>
    bool SupportsAction(string actionName);

    /// <summary>
    /// Get all supported actions
    /// </summary>
    /// <returns>Collection of supported actions</returns>
    IEnumerable<ActionInfo> GetSupportedActions();
}

/// <summary>
/// Information about a supported action
/// </summary>
public record ActionInfo
{
    /// <summary>
    /// Action name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Action description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Parameter types
    /// </summary>
    public Type[] ParameterTypes { get; init; } = Array.Empty<Type>();

    /// <summary>
    /// Return type
    /// </summary>
    public Type? ReturnType { get; init; }
}
