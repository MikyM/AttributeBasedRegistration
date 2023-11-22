using AttributeBasedRegistration.Attributes.Abstractions;

namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines with what interceptors should the service be intercepted.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class InterceptedByAttribute : Attribute, IInterceptedByAttribute
{
    /// <summary>
    /// Interceptor type.
    /// </summary>
    public Type Interceptor { get; private set; }

    /// <summary>
    /// Registration order.
    /// </summary>
    public int RegistrationOrder { get; private set; }

    /// <summary>
    /// Defines with what interceptor should the service be intercepted.
    /// </summary>
    /// <param name="interceptor">Interceptor type.</param>
    /// <param name="registrationOrder">Registration order.</param>
    public InterceptedByAttribute(int registrationOrder, Type interceptor)
    {
        Interceptor = interceptor;
        RegistrationOrder = registrationOrder;
    }
}

#if NET7_0_OR_GREATER
/// <summary>
/// Defines with what interceptors should the service be intercepted.
/// </summary>
/// <typeparam name="TInterceptor">Type of the ctor interceptor.</typeparam>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class InterceptedByAttribute<TInterceptor> : Attribute, IInterceptedByAttribute where TInterceptor : class
{
    /// <summary>
    /// Interceptor type.
    /// </summary>
    public Type Interceptor { get; private set; }

    /// <summary>
    /// Registration order.
    /// </summary>
    public int RegistrationOrder { get; private set; }

    /// <summary>
    /// Defines with what interceptor should the service be intercepted.
    /// </summary>
    /// <param name="registrationOrder">Registration order.</param>
    public InterceptedByAttribute(int registrationOrder)
    {
        var interceptor = typeof(TInterceptor);

        Interceptor = interceptor;
        RegistrationOrder = registrationOrder;
    }
}
#endif
