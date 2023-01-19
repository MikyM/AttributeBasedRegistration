using Microsoft.Extensions.Hosting;
// ReSharper disable All

namespace AttributeBasedRegistration;

/// <summary>
/// Root scope wrapper.
/// </summary>
public record RootScopeWrapper(IServiceProvider ServiceProvider)
{
    /// <summary>
    /// Root NET scope.
    /// </summary>
    public IServiceProvider ServiceProvider { get; } = ServiceProvider;
}


/// <summary>
/// Root scope wrapper starter.
/// </summary>
public record RootScopeWrapperStarter(RootScopeWrapper RootScopeWrapper) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
