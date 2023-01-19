namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Represents a SelectConstructorsWith attribute.
/// </summary>
[PublicAPI]
public interface ISelectConstructorsWithAttribute : IRegistrationAttribute
{
    /// <summary>
    /// Type of the ctor selector.
    /// </summary>
    Type ConstructorSelector { get; }
}
