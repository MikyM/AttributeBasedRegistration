namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Marks a class for registration as a service implementation.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ServiceImplementationAttribute : Attribute
{
    /// <summary>
    /// Lifetime to use for the registration.
    /// </summary>
    public ServiceLifetime? ServiceLifetime { get; private set; }
    
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
}
