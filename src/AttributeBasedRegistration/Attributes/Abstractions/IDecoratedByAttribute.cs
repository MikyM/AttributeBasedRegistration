namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Represents a DecoratedBy attribute.
/// </summary>
[PublicAPI]
public interface IDecoratedByAttribute : IRegistrationAttribute
{
    /// <summary>
    /// Decorator's type.
    /// </summary>
    Type Decorator { get; }
    
    /// <summary>
    /// Registration order.
    /// </summary>
    int RegistrationOrder { get; }
}
