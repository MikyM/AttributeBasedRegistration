namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Marks a class for registration as a service implementation.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ServiceImplementationAttribute : Attribute
{
    /// <summary>
    /// Lifetime to use for the registration if specified.
    /// </summary>
    public ServiceLifetime? ServiceLifetime { get; private set; }
    
    /// <summary>
    /// Services types if specified.
    /// </summary>
    public Type[]? ServiceTypes { get; private set; }
    
    /// <summary>
    /// Registration strategy if specified.
    /// </summary>
    public RegistrationStrategy? RegistrationStrategy { get; private set; }
    
    /// <summary>
    /// Constructor.
    /// </summary>
    public ServiceImplementationAttribute()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceLifetime">Lifetime for this registration.</param>
    public ServiceImplementationAttribute(ServiceLifetime serviceLifetime)
    {
        ServiceLifetime = serviceLifetime;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceLifetime">Lifetime for this registration.</param>
    /// <param name="registrationStrategy">Registration strategy.</param>
    public ServiceImplementationAttribute(ServiceLifetime serviceLifetime, RegistrationStrategy registrationStrategy)
    {
        ServiceLifetime = serviceLifetime;
        RegistrationStrategy = registrationStrategy;
    }
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="registrationStrategy">Registration strategy.</param>
    public ServiceImplementationAttribute(RegistrationStrategy registrationStrategy)
    {
        RegistrationStrategy = registrationStrategy;
    }
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceTypes">Service types.</param>
    public ServiceImplementationAttribute(params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes;
    }
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceLifetime">Lifetime for this registration.</param>
    /// <param name="serviceTypes">Service types.</param>
    public ServiceImplementationAttribute(ServiceLifetime serviceLifetime, params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes;
        ServiceLifetime = serviceLifetime;
    }
}
