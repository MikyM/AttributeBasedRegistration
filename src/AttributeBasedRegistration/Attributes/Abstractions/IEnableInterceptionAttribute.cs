namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Represents a EnableInterception attribute.
/// </summary>
[PublicAPI]
public interface IEnableInterceptionAttribute : IRegistrationAttribute
{
    /// <summary>
    /// Type of interception.
    /// </summary>
    InterceptionStrategy InterceptionStrategy { get; }
}
