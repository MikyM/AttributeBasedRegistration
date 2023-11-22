using AttributeBasedRegistration.Attributes.Abstractions;

namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines as what should the keyed service be registered.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterKeyedAsAttribute : Attribute, IRegisterKeyedAsAttribute
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    public Type ServiceType { get; private set; }

    /// <summary>
    /// The key of the service.
    /// </summary>
    public object Key { get; private set; }

    /// <summary>
    /// Defines as what should the service be registered.
    /// </summary>
    /// <param name="key">The key of the service.</param>
    /// <param name="serviceType">Type to use for the registration.</param>n>
    public RegisterKeyedAsAttribute(object key, Type serviceType)
    {
        ServiceType = serviceType;
        Key = key;
    }
}

#if NET7_0_OR_GREATER
/// <summary>
/// Defines as what should the keyed service be registered.
/// </summary>
/// <typeparam name="TService">Service type.</typeparam>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterKeyedAsAttribute<TService> : Attribute, IRegisterKeyedAsAttribute where TService : class
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    public Type ServiceType { get; private set; }
    
    /// <summary>
    /// The key of the service.
    /// </summary>
    public object Key { get; private set; }

    /// <summary>
    /// Defines as what should the service be registered.
    /// </summary>
    /// <param name="key">The key of the service.</param>
    public RegisterKeyedAsAttribute(object key)
    {
        ServiceType = typeof(TService);
        Key = key;
    }
}
#endif
