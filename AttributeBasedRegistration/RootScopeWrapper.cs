using Autofac;
using Microsoft.Extensions.Hosting;
// ReSharper disable All

namespace AttributeBasedRegistration;

/// <summary>
/// Root scope wrapper.
/// </summary>
public record NetRootScopeWrapper(IServiceProvider ServiceProvider)
{
    /// <summary>
    /// Root NET scope.
    /// </summary>
    public IServiceProvider ServiceProvider { get; } = ServiceProvider;
}

/// <summary>
/// Root scope wrapper.
/// </summary>
public record AutofacRootScopeWrapper(ILifetimeScope LifetimeScope)
{
    /// <summary>
    /// Root Autofac's scope if any.
    /// </summary>
    public ILifetimeScope LifetimeScope { get; } = LifetimeScope;
}

/// <summary>
/// Root scope wrapper starter.
/// </summary>
public record NetRootScopeWrapperStarter(NetRootScopeWrapper RootScopeWrapper) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

/// <summary>
/// Root scope wrapper starter.
/// </summary>
public record AutofacRootScopeWrapperStarter(AutofacRootScopeWrapper RootScopeWrapper) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
