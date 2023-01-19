namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Defines as what should the keyed service be registered.
/// </summary>
public interface IRegisterKeyedAsAttribute : IRegistrationAttribute
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    Type ServiceType { get; }

    /// <summary>
    /// The key of the service.
    /// </summary>
    object Key { get; }
}
