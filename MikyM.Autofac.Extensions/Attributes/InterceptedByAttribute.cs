using Castle.DynamicProxy;

namespace MikyM.Autofac.Extensions.Attributes;

/// <summary>
/// Defines with what interceptors should the service be intercepted.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class InterceptedByAttribute : Attribute
{
    /// <summary>
    /// Interceptor's type.
    /// </summary>
    public Type Interceptor { get; private set; }
    /// <summary>
    /// Whether it's an async interceptor.
    /// </summary>
    public bool IsAsync { get; private set; }

    /// <summary>
    /// Defines with what interceptors should the service be intercepted.
    /// </summary>
    public InterceptedByAttribute(Type interceptor)
    {
        Interceptor = interceptor ?? throw new ArgumentNullException(nameof(interceptor));

        if (interceptor.GetInterfaces().Any(x => x == typeof(IAsyncInterceptor)))
            IsAsync = true;
    }
}
