using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace AttributeBasedRegistration;

/// <summary>
/// Registration extension configuration.
/// </summary>
[PublicAPI]
public sealed class AttributeRegistrationOptions
{
    internal ContainerBuilder? Builder { get; set; }
    internal IServiceCollection? ServiceCollection { get; set; }
    
    /// <summary>
    /// Default lifetime of the registrations.
    /// </summary>
    public ServiceLifetime DefaultServiceLifetime { get; set; } = ServiceLifetime.InstancePerLifetimeScope;

    /// <summary>
    /// Default lifetime of the interceptors.
    /// </summary>
    public ServiceLifetime DefaultInterceptorLifetime { get; set; } = ServiceLifetime.InstancePerDependency;
    
    /// <summary>
    /// Whether to automatically register interceptors found in the provided assemblies to scan.
    /// </summary>
    public bool RegisterInterceptors { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="builder">Builder.</param>
    public AttributeRegistrationOptions(ContainerBuilder builder)
        => Builder = builder;
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceCollection">Service collection.</param>
    public AttributeRegistrationOptions(IServiceCollection serviceCollection)
        => ServiceCollection = serviceCollection;

    /// <summary>
    /// Registers an interceptor with <see cref="ContainerBuilder"/>.
    /// </summary>
    /// <param name="factoryMethod">Factory method for the registration.</param>
    /// <returns>Current instance of the <see cref="AttributeRegistrationOptions"/>.</returns>
    public AttributeRegistrationOptions AddInterceptor<T>(Func<IComponentContext, T> factoryMethod) where T : notnull
    {
        Builder?.Register(factoryMethod);

        return this;
    }
}
