using Castle.DynamicProxy;

namespace AttributeBasedRegistration.Autofac;

/// <summary>
/// Type extensions.
/// </summary>
[PublicAPI]
public static class TypeExtensions
{
    /// <summary>
    /// Whether given interceptor is an async interceptor.
    /// </summary>
    public static bool IsAsyncInterceptor(this Type interceptorCandidate) => interceptorCandidate.GetInterfaces().Any(x => x == typeof(IAsyncInterceptor));
}
