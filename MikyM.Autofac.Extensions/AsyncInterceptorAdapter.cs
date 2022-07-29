using Castle.DynamicProxy;

namespace MikyM.Autofac.Extensions;

/// <summary>
/// Interceptor adapter that allows registering asynchronous interceptors.
/// </summary>
[PublicAPI]
public sealed class AsyncInterceptorAdapter<TAsyncInterceptor> : AsyncDeterminationInterceptor
    where TAsyncInterceptor : IAsyncInterceptor
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="asyncInterceptor">Async interceptor.</param>
    public AsyncInterceptorAdapter(TAsyncInterceptor asyncInterceptor)
        : base(asyncInterceptor)
    { }
}
