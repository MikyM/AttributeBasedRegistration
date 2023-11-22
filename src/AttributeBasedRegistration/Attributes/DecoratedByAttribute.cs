using AttributeBasedRegistration.Attributes.Abstractions;

namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines a decorator for a service.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DecoratedByAttribute : Attribute, IDecoratedByAttribute
{
    /// <summary>
    /// Decorator's type.
    /// </summary>
    public Type Decorator { get; private set; }
    
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
    public DecoratedByAttribute(int registrationOrder, Type decoratorType)
    {
        Decorator = decoratorType;
        RegistrationOrder = registrationOrder;
    }
}

#if NET7_0_OR_GREATER
/// <summary>
/// Defines a decorator for a service.
/// </summary>
/// <typeparam name="TDecorator">Type of the decorator.</typeparam>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DecoratedByAttribute<TDecorator> : Attribute, IDecoratedByAttribute where TDecorator : class
{
    /// <summary>
    /// Decorator's type.
    /// </summary>
    public Type Decorator { get; private set; }
    
    /// <summary>
    /// Registration order.
    /// </summary>
    public int RegistrationOrder { get; private set; }

    /// <summary>
    /// Defines a decorator to use.
    /// </summary>
    /// <param name="registrationOrder">Registration order.</param>
    public DecoratedByAttribute(int registrationOrder)
    {
        RegistrationOrder = registrationOrder;
        Decorator = typeof(TDecorator);
    }
}
#endif
