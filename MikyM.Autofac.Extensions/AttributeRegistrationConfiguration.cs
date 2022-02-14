using Autofac;

namespace MikyM.Autofac.Extensions;

/// <summary>
/// Registration extension configuration
/// </summary>
public sealed class AttributeRegistrationOptions
{
    internal ContainerBuilder Builder { get; set; }
    public Lifetime DefaultLifetime { get; set; } = Lifetime.InstancePerLifetimeScope;

    public AttributeRegistrationOptions(ContainerBuilder builder)
    {
        this.Builder = builder;
    }

    /// <summary>
    /// Registers an interceptor with <see cref="ContainerBuilder"/>
    /// </summary>
    /// <param name="factoryMethod">Factory method for the registration</param>
    /// <returns>Current instance of the <see cref="AttributeRegistrationOptions"/></returns>
    public AttributeRegistrationOptions AddInterceptor<T>(Func<IComponentContext, T> factoryMethod) where T : notnull
    {
        Builder.Register(factoryMethod);

        return this;
    }
}
