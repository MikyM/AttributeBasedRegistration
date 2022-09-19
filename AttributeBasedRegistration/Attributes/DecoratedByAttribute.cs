namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines a decorator for a service.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DecoratedByAttribute : Attribute
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
    /// <param name="decoratorType">Decorator's type.</param>
    /// <param name="registrationOrder">Registration order.</param>
    /// n>
    public DecoratedByAttribute(Type decoratorType, int registrationOrder)
    {
        DecoratorType = decoratorType;
        RegistrationOrder = registrationOrder;
    }
}
