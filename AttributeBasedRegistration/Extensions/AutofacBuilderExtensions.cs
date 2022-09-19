using Autofac.Builder;
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
    public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
        AsDirectAncestorInterfaces(
            this IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> builder)
    {
        foreach (var ancestor in builder.ActivatorData.ImplementationType.GetDirectInterfaceAncestors().Where(x => x != typeof(IDisposable) && x != typeof(IAsyncDisposable)))
            builder.As(ancestor);

        return builder;
    }
    
    /// <summary>
    /// Specifies that the type is registered with it's direct ancestor interfaces.
    /// </summary>
    /// <param name="builder">Autofac's builder.</param>
    /// <returns>Current Autofac builder instance.</returns>
    public static IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>
        AsDirectAncestorInterfaces(
            this IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle> builder)
    {
        foreach (var ancestor in builder.ActivatorData.ImplementationType.GetDirectInterfaceAncestors().Where(x => x != typeof(IDisposable) && x != typeof(IAsyncDisposable)))
            builder.As(ancestor);

        return builder;
    }
    
    /// <summary>
    /// Specifies that the type is registered with an interface that is found via naming convention (Service -> IService).
    /// </summary>
    /// <param name="builder">Autofac's builder.</param>
    /// <returns>Current Autofac builder instance.</returns>
    public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
        AsConventionNamedInterface(
            this IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> builder)
    {
        builder.As(builder.ActivatorData.ImplementationType.GetInterfaceByNamingConvention() ??
                   throw new ArgumentException("Couldn't find an implemented interface that follows the naming convention"));

        return builder;
    }
    
    /// <summary>
    /// Specifies that the type is registered with an interface that is found via naming convention (Service -> IService).
    /// </summary>
    /// <param name="builder">Autofac's builder.</param>
    /// <returns>Current Autofac builder instance.</returns>
    public static IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>
        AsConventionNamedInterface(
            this IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle> builder)
    {
        builder.As(builder.ActivatorData.ImplementationType.GetInterfaceByNamingConvention() ??
                   throw new ArgumentException("Couldn't find an implemented interface that follows the naming convention"));

        return builder;
    }
}
