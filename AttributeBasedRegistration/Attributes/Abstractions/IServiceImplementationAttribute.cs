namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Represents a service implementation attribute.
/// </summary>
[PublicAPI]
public interface IServiceImplementationAttribute : IRegistrationAttribute
{
    /// <summary>
    /// Lifetime to use for the registration if specified.
    /// </summary>
    ServiceLifetime? ServiceLifetime { get; }
    
    /// <summary>
    /// Services types if specified.
    /// </summary>
    Type[]? ServiceTypes { get; }
    
    /// <summary>
    /// Registration strategy if specified.
    /// </summary>
    RegistrationStrategy? RegistrationStrategy { get; }
}
