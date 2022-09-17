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
    Self,
    /// <summary>
    /// Register as implemented interfaces.
    /// </summary>
    ImplementedInterfaces
}
