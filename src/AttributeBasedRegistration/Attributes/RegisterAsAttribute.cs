using AttributeBasedRegistration.Attributes.Abstractions;

namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines as what should the service be registered.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterAsAttribute : Attribute, IRegisterAsAttribute
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    public Type[]? ServiceTypes { get; private set; }

    /// <summary>
    /// Type of AutoFac registration.
    /// </summary>
    public RegistrationStrategy? RegistrationStrategy { get; private set; }

    /// <summary>
    /// Defines as what should the service be registered.
    /// </summary>
    /// <param name="serviceTypes">Type to use for the registration.</param>n>
    public RegisterAsAttribute(params Type[] serviceTypes)
    {
        if (serviceTypes.Length == 0)
            throw new ArgumentException("You must pass at least one service type");
        
        ServiceTypes = serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes));
    }

    /// <summary>
    /// Defines as what should the service be registered
    /// </summary>
    /// <param name="registrationStrategy">Type of registration</param>
    public RegisterAsAttribute(RegistrationStrategy registrationStrategy)
        => RegistrationStrategy = registrationStrategy;
}

#if NET7_0_OR_GREATER
/// <summary>
/// Defines as what should the service be registered.
/// </summary>
/// <typeparam name="TService">Service type.</typeparam>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterAsAttribute<TService> : Attribute, IRegisterAsAttribute where TService : class
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    public Type[]? ServiceTypes { get; private set; }

    /// <summary>
    /// Type of AutoFac registration.
    /// </summary>
    public RegistrationStrategy? RegistrationStrategy { get; private set; }

    /// <summary>
    /// Defines as what should the service be registered.
    /// </summary>
    public RegisterAsAttribute()
        => ServiceTypes = new[] { typeof(TService) };
}

/// <summary>
/// Defines as what should the service be registered.
/// </summary>
/// <typeparam name="TService1">Service type 1.</typeparam>
/// <typeparam name="TService2">Service type 2.</typeparam>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterAsAttribute<TService1, TService2> : Attribute, IRegisterAsAttribute where TService1 : class where TService2 : class
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    public Type[]? ServiceTypes { get; private set; }

    /// <summary>
    /// Type of AutoFac registration.
    /// </summary>
    public RegistrationStrategy? RegistrationStrategy { get; private set; }

    /// <summary>
    /// Defines as what should the service be registered.
    /// </summary>
    public RegisterAsAttribute()
        => ServiceTypes = new[] { typeof(TService1), typeof(TService2) };
}

/// <summary>
/// Defines as what should the service be registered.
/// </summary>
/// <typeparam name="TService1">Service type 1.</typeparam>
/// <typeparam name="TService2">Service type 2.</typeparam>
/// <typeparam name="TService3">Service type 3.</typeparam>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterAsAttribute<TService1, TService2, TService3> : Attribute, IRegisterAsAttribute where TService1 : class
    where TService2 : class
    where TService3 : class
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    public Type[]? ServiceTypes { get; private set; }

    /// <summary>
    /// Type of AutoFac registration.
    /// </summary>
    public RegistrationStrategy? RegistrationStrategy { get; private set; }

    /// <summary>
    /// Defines as what should the service be registered.
    /// </summary>
    public RegisterAsAttribute()
        => ServiceTypes = new[] { typeof(TService1), typeof(TService2), typeof(TService3) };
}

/// <summary>
/// Defines as what should the service be registered.
/// </summary>
/// <typeparam name="TService1">Service type 1.</typeparam>
/// <typeparam name="TService2">Service type 2.</typeparam>
/// <typeparam name="TService3">Service type 3.</typeparam>
/// <typeparam name="TService4">Service type 4.</typeparam>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterAsAttribute<TService1, TService2, TService3, TService4> : Attribute, IRegisterAsAttribute where TService1 : class
    where TService2 : class
    where TService3 : class
    where TService4 : class
{
    /// <summary>
    /// Type to register given service as.
    /// </summary>
    public Type[]? ServiceTypes { get; private set; }

    /// <summary>
    /// Type of AutoFac registration.
    /// </summary>
    public RegistrationStrategy? RegistrationStrategy { get; private set; }

    /// <summary>
    /// Defines as what should the service be registered.
    /// </summary>
    public RegisterAsAttribute()
        => ServiceTypes = new[] { typeof(TService1), typeof(TService2), typeof(TService3), typeof(TService4) };
}
#endif
