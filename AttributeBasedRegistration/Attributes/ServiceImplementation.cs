namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Marks a class as an implementation of a service to be registered with the DI container.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceImplementationAttribute : Attribute
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    public Type[]? ServiceTypes { get; private set; }

    /// <summary>
    /// Type of AutoFac registration.
    /// </summary>
    public RegistrationStrategy? RegistrationStrategy { get; private set; }
    
    /// <summary>
    /// Scope to use for the registration.
    /// </summary>
    public ServiceLifetime? ServiceLifetime { get; private set; }
    
    /// <summary>
    /// Type to use for owned registrations.
    /// </summary>
    public Type? OwnedByType { get; private set; }
    
    /// <summary>
    /// Tags to use for tagged registrations.
    /// </summary>
    public IEnumerable<object>? Tags { get; private set; }

    /// <summary>
    /// Defines that this service should be registered with <see cref="AttributeBasedRegistration.ServiceLifetime.InstancePerOwned"/> with given type that owns the registration.
    /// </summary>
    /// <param name="ownedByType">Type that owns the registration.</param>
    /// <param name="serviceTypes">Type to use for the registration.</param>n>
    public ServiceImplementationAttribute(Type ownedByType, params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes;
        ServiceLifetime = AttributeBasedRegistration.ServiceLifetime.InstancePerOwned;
        OwnedByType = ownedByType ?? throw new ArgumentNullException(nameof(ownedByType));
    }

    /// <summary>
    /// Defines that this service should be registered with <see cref="AttributeBasedRegistration.ServiceLifetime.InstancePerMatchingLifetimeScope"/> with given tags.
    /// </summary>
    /// <param name="tags">Tags.</param>
    /// <param name="serviceTypes">Type to use for the registration.</param>
    public ServiceImplementationAttribute(IEnumerable<object> tags, params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes;
        ServiceLifetime = AttributeBasedRegistration.ServiceLifetime.InstancePerMatchingLifetimeScope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }

    /// <summary>
    /// Defines as what should the service be registered.
    /// </summary>
    /// <param name="serviceTypes">Type to use for the registration.</param>n>
    /// <param name="serviceLifetime">Lifetime.</param>
    public ServiceImplementationAttribute(ServiceLifetime serviceLifetime, params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes));
        ServiceLifetime = serviceLifetime;
    }

    /// <summary>
    /// Defines as what should the service be registered
    /// </summary>
    /// <param name="registrationStrategy">Type of registration</param>
    /// <param name="serviceLifetime">Lifetime.</param>
    public ServiceImplementationAttribute(ServiceLifetime serviceLifetime, RegistrationStrategy registrationStrategy)
    {
        RegistrationStrategy = registrationStrategy;
        ServiceLifetime = serviceLifetime;
    }
    
    /// <summary>
    /// Defines as what should the service be registered.
    /// </summary>
    /// <param name="serviceTypes">Type to use for the registration.</param>n>
    public ServiceImplementationAttribute(params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes));
    }

    /// <summary>
    /// Defines as what should the service be registered
    /// </summary>
    /// <param name="registrationStrategy">Type of registration</param>
    public ServiceImplementationAttribute(RegistrationStrategy registrationStrategy)
    {
        RegistrationStrategy = registrationStrategy;
    }
}
