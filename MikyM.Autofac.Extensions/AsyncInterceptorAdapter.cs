using Castle.DynamicProxy;

namespace MikyM.Autofac.Extensions;

/// <summary>
/// Interceptor adapter that allows registering asynchronous interceptors
/// </summary>
public sealed class AsyncInterceptorAdapter<TAsyncInterceptor> : AsyncDeterminationInterceptor
    where TAsyncInterceptor : IAsyncInterceptor
{
    public AsyncInterceptorAdapter(TAsyncInterceptor asyncInterceptor)
        : base(asyncInterceptor)
    { }
}