namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Represents a RegisterAs attribute.
/// </summary>
[PublicAPI]
public interface IRegisterAsAttribute : IRegistrationAttribute
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    Type[]? ServiceTypes { get; }

    /// <summary>
    /// Type of AutoFac registration.
    /// </summary>
    RegistrationStrategy? RegistrationStrategy { get; }
}
