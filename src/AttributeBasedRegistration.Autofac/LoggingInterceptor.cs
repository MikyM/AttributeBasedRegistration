using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace AttributeBasedRegistration.Autofac;

/// <summary>
/// Base logging interceptor.
/// </summary>
[PublicAPI]
public class LoggingInterceptor : AsyncInterceptorBase
{
    private readonly ILogger _logger;

    /// <inheritdoc />
    public LoggingInterceptor(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        var sw = new Stopwatch();

        _logger.LogTrace("Calling {DeclaringTypeName} {MethodName}", invocation.Method.DeclaringType?.Name, invocation.Method.Name);    

        try
        {
            sw.Start();
            
            await proceed(invocation, proceedInfo).ConfigureAwait(false);
            
            sw.Stop();
        }
        catch (Exception)
        {
            sw.Stop();
            
            _logger.LogTrace("Execution of {DeclaringTypeName} {MethodName} errored after {ElapsedTotalMilliseconds} ms", invocation.Method.DeclaringType?.Name, invocation.Method.Name, sw.Elapsed.TotalMilliseconds); 
            
            throw;
        }

        _logger.LogTrace("Finished executing {DeclaringTypeName} {MethodName} after {ElapsedTotalMilliseconds} ms", invocation.Method.DeclaringType?.Name, invocation.Method.Name, sw.Elapsed.TotalMilliseconds);   
    }

    /// <inheritdoc />
    protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation,
        IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var sw = new Stopwatch();

        _logger.LogTrace("Calling {DeclaringTypeName} {MethodName}", invocation.Method.DeclaringType?.Name, invocation.Method.Name);        

        TResult? result;

        try
        {
            sw.Start();
            
            result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
            
            sw.Stop();
        }
        catch (Exception)
        {
            sw.Stop();

            _logger.LogTrace("Execution of {DeclaringTypeName} {MethodName} errored after {ElapsedTotalMilliseconds} ms", invocation.Method.DeclaringType?.Name, invocation.Method.Name, sw.Elapsed.TotalMilliseconds); 
            
            throw;
        }

        _logger.LogTrace("Finished executing {DeclaringTypeName} {MethodName} after {ElapsedTotalMilliseconds} ms", invocation.Method.DeclaringType?.Name, invocation.Method.Name, sw.Elapsed.TotalMilliseconds);   

        return result;
    }
}
