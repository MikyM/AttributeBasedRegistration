namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines as what should the service be registered.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterAsAttribute : Attribute
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
    /// Defines as what should the service be registered.
    /// </summary>
    /// <param name="serviceTypes">Type to use for the registration.</param>n>
    public RegisterAsAttribute(params Type[] serviceTypes)
    {
        if (serviceTypes.Length == 0)
            throw new ArgumentException("You must pass at least one service type");
        
        ServiceTypes = serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes));
    }

    /// <summary>
    /// Defines as what should the service be registered
    /// </summary>
    /// <param name="registrationStrategy">Type of registration</param>
    public RegisterAsAttribute(RegistrationStrategy registrationStrategy)
        => RegistrationStrategy = registrationStrategy;
}
