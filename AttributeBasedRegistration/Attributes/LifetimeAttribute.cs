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
    public ServiceLifetime ServiceLifetime { get; private set; }
    
    /// <summary>
    /// Type to use for owned registrations.
    /// </summary>
    public Type? Owned { get; private set; }
    
    /// <summary>
    /// Tags to use for tagged registrations.
    /// </summary>
    public IEnumerable<object>? Tags { get; private set; }

    /// <summary>
    /// Defines with which lifetime should the service be registered.
    /// </summary>
    public LifetimeAttribute(ServiceLifetime serviceLifetime)
        => ServiceLifetime = serviceLifetime;
    
    /// <summary>
    /// Defines that this service should be registered with <see cref="AttributeBasedRegistration.ServiceLifetime.InstancePerOwned"/> with given type that owns the registration.
    /// </summary>
    /// <param name="owned">Type that owns the registration.</param>
    public LifetimeAttribute(Type owned)
    {
        ServiceLifetime = ServiceLifetime.InstancePerOwned;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    /// <summary>
    /// Defines that this service should be registered with <see cref="AttributeBasedRegistration.ServiceLifetime.InstancePerMatchingLifetimeScope"/> with given tags.
    /// </summary>
    /// <param name="tags">Tags.</param>
    public LifetimeAttribute(params object[] tags)
    {
        ServiceLifetime = ServiceLifetime.InstancePerMatchingLifetimeScope;
        if (tags.Length == 0)
            throw new ArgumentException("You must supply at least one tag");
            
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
    }
}
