namespace MikyM.Autofac.Extensions;

/// <summary>
/// Lifetime types.
/// </summary>
[PublicAPI]
public enum Lifetime
{
    /// <summary>
    /// Single instance.
    /// </summary>
    SingleInstance,
    /// <summary>
    /// Instance per request.
    /// </summary>
    InstancePerRequest,
    /// <summary>
    /// Instance per lifetime scope.
    /// </summary>
    InstancePerLifetimeScope,
    /// <summary>
    /// Instance per matching lifetime scope.
    /// </summary>
    InstancePerMatchingLifetimeScope,
    /// <summary>
    /// Instance per dependancy.
    /// </summary>
    InstancePerDependency,
    /// <summary>
    /// Instance per owned.
    /// </summary>
    InstancePerOwned
}
