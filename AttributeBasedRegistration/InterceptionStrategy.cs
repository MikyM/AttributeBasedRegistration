namespace AttributeBasedRegistration;

/// <summary>
/// Interception strategies.
/// </summary>
[PublicAPI]
public enum InterceptionStrategy
{
    /// <summary>
    /// Intercept interface and class.
    /// </summary>
    InterfaceAndClass,
    /// <summary>
    /// Intercept interface.
    /// </summary>
    Interface,
    /// <summary>
    /// Intercept class.
    /// </summary>
    Class
}
