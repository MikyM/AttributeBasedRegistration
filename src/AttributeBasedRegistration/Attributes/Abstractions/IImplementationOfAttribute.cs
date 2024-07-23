namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Represents a service implementation attribute.
/// </summary>
/// <remarks> A newer equivalent of <see cref="IServiceImplementationAttribute"/>.</remarks>
[PublicAPI]
public interface IImplementationOfAttribute : IRegistrationAttribute
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
