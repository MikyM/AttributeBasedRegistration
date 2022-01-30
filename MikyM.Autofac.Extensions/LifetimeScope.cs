namespace MikyM.Autofac.Extensions;

/// <summary>
/// Lifetime types
/// </summary>
public enum Lifetime
{
    SingleInstance,
    InstancePerRequest,
    InstancePerLifetimeScope,
    InstancePerMatchingLifetimeScope,
    InstancePerDependancy,
    InstancePerOwned
}