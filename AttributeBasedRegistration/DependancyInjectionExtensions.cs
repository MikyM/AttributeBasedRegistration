using System.Reflection;
using AttributeBasedRegistration.Attributes;
using Autofac;
using Autofac.Builder;
using Autofac.Core.Activators.Reflection;
using Autofac.Extras.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AttributeBasedRegistration;

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

        var decoratorServicePairs = new Dictionary<Type, List<Type>>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var set = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(false).Any(y => y is ServiceAttribute) &&
                            x.IsClass && !x.IsAbstract)
                .ToList();

            var decorators = set.SelectMany(x => x.GetCustomAttributes<DecoratedByAttribute>(false))
                .Select(x => x.DecoratorType).Distinct().ToList();
            decorators.ForEach(x => decoratorServicePairs.Add(x, new List<Type>()));

            foreach (var type in set)
            {
                var intrAttrs = type.GetCustomAttributes<InterceptedByAttribute>(false).ToList();
                var scopeAttr = type.GetCustomAttribute<LifetimeAttribute>(false);
                var asAttrs = type.GetCustomAttributes<RegisterAsAttribute>(false).ToList();
                var ctorAttr = type.GetCustomAttribute<FindConstructorsWithAttribute>(false);
                var intrEnableAttr = type.GetCustomAttribute<EnableInterceptionAttribute>(false);
                var decAttrs = type.GetCustomAttributes<DecoratedByAttribute>(false).ToList();

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

                if (!decAttrs.Any())
                    continue;
                    
                HashSet<Type> serviceTypes = new();

                if (shouldAsSelf)
                    serviceTypes.Add(type);
                if (registerAsTypes.Any())
                    registerAsTypes.ForEach(x =>
                    {
                        if (x is not null)
                            serviceTypes.Add(x);
                    });

                foreach (var decAttr in decAttrs.OrderBy(x => x.RegistrationOrder))
                {
                    if (decAttr.DecoratorType.IsGenericType && decAttr.DecoratorType.IsGenericTypeDefinition)
                    {
                        foreach (var serviceType in serviceTypes)
                        {
                            if (!serviceType.IsGenericType || !serviceType.IsGenericTypeDefinition)
                                throw new InvalidOperationException(
                                    "Can't register an open generic type decorator for a non-open generic type service");
                            
                            builder.RegisterGenericDecorator(decAttr.DecoratorType, serviceType);
                        }
                    }
                    else 
                    {
                        foreach (var serviceType in serviceTypes)
                        {
                            if (serviceType.IsGenericType && serviceType.IsGenericTypeDefinition)
                                throw new InvalidOperationException(
                                    "Can't register an non-open generic type decorator for an open generic type service");
                            
                            builder.RegisterDecorator(decAttr.DecoratorType, serviceType);
                        }
                    }
                }
            }
        }

        return builder;
    }
    
    /// <summary>
    /// Registers services with <see cref="ContainerBuilder"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="options">Optional configuration.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, Action<AttributeRegistrationOptions>? options = null)
    {
        var config = new AttributeRegistrationOptions(serviceCollection);
        options?.Invoke(config);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var set = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(false).Any(y => y is ServiceAttribute) &&
                            x.IsClass && !x.IsAbstract)
                .ToList(); ;

            foreach (var type in set)
            {
                var scopeAttr = type.GetCustomAttribute<LifetimeAttribute>(false);
                var asAttrs = type.GetCustomAttributes<RegisterAsAttribute>(false).ToList();
                
                var scope = scopeAttr?.Scope ?? config.DefaultLifetime;

                var registerAsTypes = asAttrs.Where(x => x.RegisterAsType is not null)
                    .Select(x => x.RegisterAsType)
                    .Distinct()
                    .ToList();
                
                var shouldAsSelf = asAttrs.Any(x => x.RegisterAsOption == RegisterAs.Self) &&
                                   asAttrs.All(x => x.RegisterAsType != type);
                var shouldAsInterfaces = !asAttrs.Any() ||
                                         asAttrs.Any(x => x.RegisterAsOption == RegisterAs.ImplementedInterfaces);

                if (!shouldAsInterfaces && !registerAsTypes.Any())
                    shouldAsSelf = true;

                var interfaces = type.GetInterfaces().ToList();
                
                switch (scope)
                {
                    case Lifetime.SingleInstance:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.TryAddSingleton(x, type));
                        if (shouldAsSelf)
                            serviceCollection.TryAddSingleton(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.TryAddSingleton(x, type));
                        break;
                    case Lifetime.InstancePerRequest:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.TryAddScoped(x, type));
                        if (shouldAsSelf)
                            serviceCollection.TryAddScoped(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.TryAddScoped(x, type));
                        break;
                    case Lifetime.InstancePerLifetimeScope:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.TryAddScoped(x, type));
                        if (shouldAsSelf)
                            serviceCollection.TryAddScoped(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.TryAddScoped(x, type));
                        break;
                    case Lifetime.InstancePerDependency:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.TryAddTransient(x, type));
                        if (shouldAsSelf)
                            serviceCollection.TryAddTransient(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.TryAddTransient(x, type));
                        break;
                    case Lifetime.InstancePerMatchingLifetimeScope:
                        throw new NotSupportedException();
                    case Lifetime.InstancePerOwned:
                        throw new NotSupportedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(scope));
                }
            }
        }

        return serviceCollection;
    }
}
