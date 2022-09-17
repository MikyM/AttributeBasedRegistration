namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Marks a service implementation for decoration with specified decorator.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class DecoratedAttribute : Attribute
{
    /// <summary>
    /// Decorator's type.
    /// </summary>
    public Type DecoratorType { get; private set; }
    
    /// <summary>
    /// Registration order.
    /// </summary>
    public int RegistrationOrder { get; private set; }

    /// <summary>
    /// Defines a decorator to use.
    /// </summary>
    /// <param name="decorator">Decorator's type.</param>
    /// <param name="registrationOrder">Registration order.</param>
    /// n>
    public DecoratedAttribute(int registrationOrder, Type decorator)
    {
        DecoratorType = decorator;
        RegistrationOrder = registrationOrder;
    }
}
