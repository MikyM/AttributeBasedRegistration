namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Represents a FindConstructorsWith attribute.
/// </summary>
[PublicAPI]
public interface IFindConstructorsWithAttribute : IRegistrationAttribute
{
    /// <summary>
    /// Type of the ctor finder.
    /// </summary>
    Type ConstructorFinder { get; }
}
