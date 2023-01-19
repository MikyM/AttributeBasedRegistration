using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace AttributeBasedRegistration;

/// <summary>
/// Registration extension configuration.
/// </summary>
[PublicAPI]
public sealed class AttributeRegistrationOptions
{
    /// <summary>
    /// Default lifetime of the registrations.
    /// </summary>
    public ServiceLifetime DefaultServiceLifetime { get; set; } = ServiceLifetime.InstancePerLifetimeScope;
}
