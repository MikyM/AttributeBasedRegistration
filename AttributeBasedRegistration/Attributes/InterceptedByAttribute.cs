using Castle.DynamicProxy;

namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines with what interceptors should the service be intercepted.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class InterceptedByAttribute : Attribute
{
    /// <summary>
    /// Interceptor type.
    /// </summary>
    public Type Interceptor { get; private set; }
    
    /// <summary>
    /// Interceptor lifetime.
    /// </summary>
    public ServiceLifetime InterceptorLifetime { get; private set; }
    
    /// <summary>
    /// Registration order.
    /// </summary>
    public int RegistrationOrder { get; private set; }

    /// <summary>
    /// Defines with what interceptor should the service be intercepted.
    /// </summary>
    /// <param name="interceptor">Interceptor type.</param>
    /// <param name="registrationOrder">Registration order.</param>
    /// <param name="interceptorLifetime">Interceptor's lifetime.</param>
    public InterceptedByAttribute(int registrationOrder, Type interceptor, ServiceLifetime interceptorLifetime)
    {
        if (interceptor.GetInterfaces().All(x => x != typeof(IAsyncInterceptor) && x != typeof(IInterceptor)))
            throw new ArgumentException($"{interceptor.Name} does not implement any interceptor interface");

        Interceptor = interceptor;
        RegistrationOrder = registrationOrder;
        InterceptorLifetime = interceptorLifetime;
    }
}
