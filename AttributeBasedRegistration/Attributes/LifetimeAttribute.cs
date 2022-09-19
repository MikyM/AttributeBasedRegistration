namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines with which lifetime should the service be registered.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class LifetimeAttribute : Attribute
{
    /// <summary>
    /// Lifetime to use for the registration.
    /// </summary>
    public ServiceLifetime Scope { get; private set; }
    /// <summary>
    /// Type to use for owned registrations.
    /// </summary>
    public Type? Owned { get; private set; }
    /// <summary>
    /// Tags to use for tagged registrations.
    /// </summary>
    public IEnumerable<object> Tags { get; private set; } = new List<string>();

    /// <summary>
    /// Defines with which lifetime should the service be registered.
    /// </summary>
    public LifetimeAttribute(ServiceLifetime scope)
        => Scope = scope;
    
    /// <summary>
    /// Defines that this service should be registered with <see cref="ServiceLifetime.InstancePerOwned"/> with given type that owns the registration.
    /// </summary>
    /// <param name="owned">Type that owns the registration.</param>
    public LifetimeAttribute(Type owned)
    {
        Scope = ServiceLifetime.InstancePerOwned;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    /// <summary>
    /// Defines that this service should be registered with <see cref="ServiceLifetime.InstancePerMatchingLifetimeScope"/> with given tags.
    /// </summary>
    /// <param name="tags">Tags.</param>
    public LifetimeAttribute(IEnumerable<object> tags)
    {
        Scope = ServiceLifetime.InstancePerMatchingLifetimeScope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }
}
