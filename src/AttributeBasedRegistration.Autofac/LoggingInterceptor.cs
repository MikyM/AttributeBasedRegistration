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
        var args = new List<string>();

        foreach (var arg in invocation.Arguments.Select((x, i) => new { i, x }))
        {
            string str = string.Empty;
            dynamic obj = new ExpandoObject();
            obj.Index = arg.i;
            obj.ArgumentType = arg.x.GetType().Name;

            try
            {
                str = JsonSerializer.Serialize(arg);
            }
            catch
            {
                obj.Value = "Couldnt serialize";
                args.Add(JsonSerializer.Serialize(obj));
            }

            obj.Value = str;
            args.Add(JsonSerializer.Serialize(obj));
        }

        args.Insert(0, "{[");
        args.Add("]}");
        var serializedArgs = string.Join(" ", args);

        _logger.LogDebug($"Calling {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs}");

        try
        {
            sw.Start();
            await proceed(invocation, proceedInfo).ConfigureAwait(false);
            sw.Stop();
        }
        catch (Exception)
        {
            sw.Stop();
            _logger.LogDebug($"Execution of {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs} errored after {sw.Elapsed.TotalMilliseconds} ms");
            throw;
        }

        _logger.LogDebug($"Finished executing {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs} after {sw.Elapsed.TotalMilliseconds} ms");
    }

    /// <inheritdoc />
    protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation,
        IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var sw = new Stopwatch();
        var args = new List<string>();

        foreach (var arg in invocation.Arguments.Select((x, i) => new { i, x }))
        {
            string str = string.Empty;
            dynamic obj = new ExpandoObject();
            obj.Index = arg.i;
            obj.ArgumentType = arg.x.GetType().Name;

            try
            {
                str = JsonSerializer.Serialize(arg);
            }
            catch
            {
                obj.Value = "Couldnt serialize";
                args.Add(JsonSerializer.Serialize(obj));
            }

            obj.Value = str;
            args.Add(JsonSerializer.Serialize(obj));
        }

        args.Insert(0, "{[");
        args.Add("]}");
        var serializedArgs = string.Join(" ", args);

        _logger.LogDebug($"Calling {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs}");

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
            _logger.LogDebug($"Execution of {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs} errored after {sw.Elapsed.TotalMilliseconds} ms");
            throw;
        }

        _logger.LogDebug($"Finished executing {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs} after {sw.Elapsed.TotalMilliseconds} ms");

        return result;
    }
}
