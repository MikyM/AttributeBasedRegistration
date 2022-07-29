using Autofac;
using Autofac.Builder;
using Autofac.Extras.DynamicProxy;
using MikyM.Autofac.Extensions.Attributes;
using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace MikyM.Autofac.Extensions;

/// <summary>
/// Extensions.
/// </summary>
[PublicAPI]
public static class DependancyInjectionExtensions
{
    /// <summary>
    /// Registers services with <see cref="ContainerBuilder"/> via attributes.
    /// </summary>
    /// <param name="builder">Current builder instance.</param>
    /// <param name="options">Optional configuration.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static ContainerBuilder AddAttributeDefinedServices(this ContainerBuilder builder, Action<AttributeRegistrationOptions>? options = null)
    {
        var config = new AttributeRegistrationOptions(builder);
        options?.Invoke(config);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var set = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(false).Any(y => y is ServiceAttribute) &&
                            x.IsClass && !x.IsAbstract)
                .ToList();

            foreach (var type in set)
            {
                var intrAttrs = type.GetCustomAttributes<InterceptedByAttribute>(false).ToList();
                var scopeAttr = type.GetCustomAttribute<LifetimeAttribute>(false);
                var asAttrs = type.GetCustomAttributes<RegisterAsAttribute>(false).ToList();
                var ctorAttr = type.GetCustomAttribute<FindConstructorsWithAttribute>(false);
                var intrEnableAttr = type.GetCustomAttribute<EnableInterceptionAttribute>(false);

                if (ctorAttr is not null && intrEnableAttr is not null)
                    throw new InvalidOperationException(
                        "Using a custom constructor finder will prevent interception from happening");

                var scope = scopeAttr?.Scope ?? config.DefaultLifetime;

                var registerAsTypes = asAttrs.Where(x => x.RegisterAsType is not null)
                    .Select(x => x.RegisterAsType)
                    .Distinct()
                    .ToList();
                var shouldAsSelf = asAttrs.Any(x => x.RegisterAsOption == RegisterAs.Self) &&
                                   asAttrs.All(x => x.RegisterAsType != type);
                var shouldAsInterfaces = !asAttrs.Any() ||
                                         asAttrs.Any(x => x.RegisterAsOption == RegisterAs.ImplementedInterfaces);

                IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>?
                    registrationGenericBuilder = null;
                IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>?
                    registrationBuilder = null;

                if (type.IsGenericType && type.IsGenericTypeDefinition)
                {
                    if (intrEnableAttr is not null)
                        registrationGenericBuilder = shouldAsInterfaces
                            ? builder.RegisterGeneric(type).AsImplementedInterfaces().EnableInterfaceInterceptors()
                            : builder.RegisterGeneric(type).EnableInterfaceInterceptors();
                    else
                        registrationGenericBuilder = shouldAsInterfaces
                            ? builder.RegisterGeneric(type).AsImplementedInterfaces()
                            : builder.RegisterGeneric(type);
                }
                else
                {
                    if (intrEnableAttr is not null)
                    {
                        registrationBuilder = intrEnableAttr.Intercept switch
                        {
                            Intercept.InterfaceAndClass => shouldAsInterfaces
                                ? builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .EnableClassInterceptors()
                                    .EnableInterfaceInterceptors()
                                : builder.RegisterType(type)
                                    .EnableClassInterceptors()
                                    .EnableInterfaceInterceptors(),
                            Intercept.Interface => shouldAsInterfaces
                                ? builder.RegisterType(type).AsImplementedInterfaces().EnableInterfaceInterceptors()
                                : builder.RegisterType(type).EnableInterfaceInterceptors(),
                            Intercept.Class => shouldAsInterfaces
                                ? builder.RegisterType(type).AsImplementedInterfaces().EnableClassInterceptors()
                                : builder.RegisterType(type).EnableClassInterceptors(),
                            _ => throw new ArgumentOutOfRangeException(nameof(intrEnableAttr.Intercept))
                        };
                    }
                    else
                    {
                        registrationBuilder = shouldAsInterfaces
                            ? builder.RegisterType(type).AsImplementedInterfaces()
                            : builder.RegisterType(type);
                    }
                }

                if (shouldAsSelf)
                {
                    registrationBuilder = registrationBuilder?.As(type);
                    registrationGenericBuilder = registrationGenericBuilder?.AsSelf();
                }

                foreach (var asType in registerAsTypes)
                {
                    if (asType is null) throw new InvalidOperationException("Type was null during registration");

                    registrationBuilder = registrationBuilder?.As(asType);
                    registrationGenericBuilder = registrationGenericBuilder?.As(asType);
                }

                switch (scope)
                {
                    case Lifetime.SingleInstance:
                        registrationBuilder = registrationBuilder?.SingleInstance();
                        registrationGenericBuilder = registrationGenericBuilder?.SingleInstance();
                        break;
                    case Lifetime.InstancePerRequest:
                        registrationBuilder = registrationBuilder?.InstancePerRequest();
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerRequest();
                        break;
                    case Lifetime.InstancePerLifetimeScope:
                        registrationBuilder = registrationBuilder?.InstancePerLifetimeScope();
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerLifetimeScope();
                        break;
                    case Lifetime.InstancePerDependency:
                        registrationBuilder = registrationBuilder?.InstancePerDependency();
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerDependency();
                        break;
                    case Lifetime.InstancePerMatchingLifetimeScope:
                        registrationBuilder =
                            registrationBuilder?.InstancePerMatchingLifetimeScope(scopeAttr?.Tags.ToArray() ??
                                Array.Empty<object>());
                        registrationGenericBuilder =
                            registrationGenericBuilder?.InstancePerMatchingLifetimeScope(
                                scopeAttr?.Tags.ToArray() ?? Array.Empty<object>());
                        break;
                    case Lifetime.InstancePerOwned:
                        if (scopeAttr?.Owned is null) throw new InvalidOperationException("Owned type was null");

                        registrationBuilder = registrationBuilder?.InstancePerOwned(scopeAttr.Owned);
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerOwned(scopeAttr.Owned);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(scope));
                }

                foreach (var attr in intrAttrs)
                {
                    registrationBuilder = attr.IsAsync
                        ? registrationBuilder?.InterceptedBy(
                            typeof(AsyncInterceptorAdapter<>).MakeGenericType(attr.Interceptor))
                        : registrationBuilder?.InterceptedBy(attr.Interceptor);
                    registrationGenericBuilder = attr.IsAsync
                        ? registrationGenericBuilder?.InterceptedBy(
                            typeof(AsyncInterceptorAdapter<>).MakeGenericType(attr.Interceptor))
                        : registrationGenericBuilder?.InterceptedBy(attr.Interceptor);
                }

                if (ctorAttr is not null)
                {
                    var instance = Activator.CreateInstance(ctorAttr.ConstructorFinder);

                    if (instance is null)
                        throw new InvalidOperationException(
                            $"Couldn't create an instance of a custom ctor finder of type {ctorAttr.ConstructorFinder.Name}, only finders with parameterless ctors are supported");

                    registrationBuilder = registrationBuilder?.FindConstructorsWith((IConstructorFinder)instance);
                    registrationGenericBuilder = registrationGenericBuilder?.FindConstructorsWith((IConstructorFinder)instance);
                }
            }
        }

        return builder;
    }
}
