using Autofac.Builder;
using Castle.DynamicProxy;
using MikyM.Utilities.Extensions;

namespace AttributeBasedRegistration.Extensions;

/// <summary>
/// Autofac builder extensions.
/// </summary>
[PublicAPI]
public static class AutofacBuilderExtensions
{
    /// <summary>
    /// Specifies that the type is registered with it's direct ancestor interfaces.
    /// </summary>
    /// <param name="builder">Autofac's builder.</param>
    /// <returns>Current Autofac builder instance.</returns>
    public static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>?
        AsDirectAncestorInterfaces<TRegistrationStyle, TActivatorData>(
            this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> builder)
        where TActivatorData : ReflectionActivatorData
    {
        var implType = ProxyUtil.IsProxyType(builder.ActivatorData.ImplementationType)
            ? builder.ActivatorData.ImplementationType.BaseType
            : builder.ActivatorData.ImplementationType;

        foreach (var ancestor in implType!.GetDirectInterfaceAncestors().Where(x => x != typeof(IDisposable) && x != typeof(IAsyncDisposable)))
            builder.As(ancestor);

        return builder;
    }

    /// <summary>
    /// Specifies that the type is registered with an interface that is found via naming convention (Service -> IService).
    /// </summary>
    /// <param name="builder">Autofac's builder.</param>
    /// <returns>Current Autofac builder instance.</returns>
    public static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>?
        AsConventionNamedInterface<TRegistrationStyle, TActivatorData>(
            this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> builder)
        where TActivatorData : ReflectionActivatorData
    {
        var implType = ProxyUtil.IsProxyType(builder.ActivatorData.ImplementationType)
            ? builder.ActivatorData.ImplementationType.BaseType
            : builder.ActivatorData.ImplementationType;
        
        builder.As(implType!.GetInterfaceByNamingConvention() ??
                   throw new ArgumentException("Couldn't find an implemented interface that follows the naming convention"));

        return builder;
    }
}
