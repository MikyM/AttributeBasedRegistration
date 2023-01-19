namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Represents a InterceptedBy attribute.
/// </summary>
[PublicAPI]
public interface IInterceptedByAttribute : IRegistrationAttribute
{
    /// <summary>
    /// Interceptor type.
    /// </summary>
    Type Interceptor { get; }

    /// <summary>
    /// Registration order.
    /// </summary>
    int RegistrationOrder { get; }
}
