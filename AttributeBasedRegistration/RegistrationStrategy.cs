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
    AsImplementedInterfaces,
    /// <summary>
    /// Register as direct ancestor interfaces.
    /// </summary>
    AsDirectAncestorInterfaces,
    /// <summary>
    /// Register using naming convention (SomeService -> ISomeService).
    /// </summary>
    AsConventionNamedInterface
}
