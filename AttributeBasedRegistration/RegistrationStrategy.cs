namespace AttributeBasedRegistration;

/// <summary>
/// Registration strategies.
/// </summary>
[PublicAPI]
public enum RegistrationStrategy
{
    /// <summary>
    /// Register as self.
    /// </summary>
    AsSelf,
    /// <summary>
    /// Register as implemented interfaces.
    /// </summary>
    AsImplementedInterfaces
}
