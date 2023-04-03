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
    
    /// <summary>
    /// Checks whether the current type is an interceptor implementation (implements either <see cref="IInterceptor"/> or <see cref="IAsyncInterceptor"/>).
    /// </summary>
    /// <param name="type">Candidate.</param>
    /// <returns>True if type is an interceptor implementation, otherwise false.</returns>
    public static bool IsInterceptorImplementation(this Type type)
        => type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(x => x == typeof(IInterceptor) || x == typeof(IAsyncInterceptor));
}
