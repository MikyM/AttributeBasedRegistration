using System.Reflection;
using AttributeBasedRegistration.Attributes;
using AttributeBasedRegistration.Extensions;
using Autofac;
using Autofac.Builder;
using Autofac.Core.Activators.Reflection;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MikyM.Utilities.Extensions;

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
    /// <param name="assembliesContainingTypesToScan">Assemblies containing types to scan for services.</param>
    /// <param name="options">Optional configuration.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static ContainerBuilder AddAttributeDefinedServices(this ContainerBuilder builder,
        IEnumerable<Type> assembliesContainingTypesToScan, Action<AttributeRegistrationOptions>? options = null)
        => AddAttributeDefinedServices(builder, assembliesContainingTypesToScan.Select(x => x.Assembly), options);
        
    /// <summary>
    /// Registers services with <see cref="ContainerBuilder"/> via attributes.
    /// </summary>
    /// <param name="builder">Current builder instance.</param>
    /// <param name="assembliesToScan">Assemblies to scan for services.</param>
    /// <param name="options">Optional configuration.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static ContainerBuilder AddAttributeDefinedServices(this ContainerBuilder builder, IEnumerable<Assembly> assembliesToScan, Action<AttributeRegistrationOptions>? options = null)
    {
        var config = new AttributeRegistrationOptions(builder);
        options?.Invoke(config);
        

        foreach (var assembly in assembliesToScan)
        {
            var set = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(false).Any(y => y is ServiceImplementationAttribute) &&
                            x.IsClass && !x.IsAbstract)
                .ToList();

            foreach (var type in set)
            {
                var attr = type.GetCustomAttribute<ServiceImplementationAttribute>(false);
                if (attr is null)
                    throw new InvalidOperationException();
                
                var intrAttrs = type.GetCustomAttributes<InterceptedByAttribute>(false).ToList();
                var scopeAttr = type.GetCustomAttribute<LifetimeAttribute>(false);
                var asAttrs = type.GetCustomAttributes<RegisterAsAttribute>(false).ToList();
                var ctorAttr = type.GetCustomAttribute<FindConstructorsWithAttribute>(false);
                var intrEnableAttr = type.GetCustomAttribute<EnableInterceptionAttribute>(false);
                var decAttrs = type.GetCustomAttributes<DecoratedByAttribute>(false).ToList();

                if (ctorAttr is not null && intrEnableAttr is not null)
                    throw new InvalidOperationException(
                        "Using a custom constructor finder will prevent interception from happening");

                var scope = scopeAttr?.ServiceLifetime ?? attr?.ServiceLifetime ?? config.DefaultServiceLifetime;

                var registerAsTypes = asAttrs.Where(x => x.ServiceTypes is not null)
                    .SelectMany(x => x.ServiceTypes ?? Type.EmptyTypes)
                    .Concat(attr?.ServiceTypes ?? Type.EmptyTypes)
                    .Distinct()
                    .ToList();

                var shouldAsSelf = (asAttrs.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsSelf) ||
                                    attr?.RegistrationStrategy is RegistrationStrategy.AsSelf) &&
                                   registerAsTypes.All(y => y != type);

                var shouldAsInterfaces = asAttrs.Any(x =>
                                             x.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces) ||
                                         attr?.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces;

                var shouldAsDirectAncestors =
                    asAttrs.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces) ||
                    attr?.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces;

                var shouldUsingNamingConvention =
                    asAttrs.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface) ||
                    attr?.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface;

                if (!shouldAsInterfaces && !registerAsTypes.Any() && !shouldAsDirectAncestors &&
                    !shouldUsingNamingConvention)
                    shouldAsSelf = true;

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
                        registrationBuilder = intrEnableAttr.InterceptionStrategy switch
                        {
                            InterceptionStrategy.InterfaceAndClass => shouldAsInterfaces
                                ? builder.RegisterType(type)
                                    .AsImplementedInterfaces()
                                    .EnableClassInterceptors()
                                    .EnableInterfaceInterceptors()
                                : builder.RegisterType(type)
                                    .EnableClassInterceptors()
                                    .EnableInterfaceInterceptors(),
                            InterceptionStrategy.Interface => shouldAsInterfaces
                                ? builder.RegisterType(type).AsImplementedInterfaces().EnableInterfaceInterceptors()
                                : builder.RegisterType(type).EnableInterfaceInterceptors(),
                            InterceptionStrategy.Class => shouldAsInterfaces
                                ? builder.RegisterType(type).AsImplementedInterfaces().EnableClassInterceptors()
                                : builder.RegisterType(type).EnableClassInterceptors(),
                            _ => throw new ArgumentOutOfRangeException(nameof(intrEnableAttr.InterceptionStrategy))
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

                if (shouldAsDirectAncestors)
                {
                    registrationBuilder = registrationBuilder?.AsDirectAncestorInterfaces();
                    registrationGenericBuilder = registrationGenericBuilder?.AsDirectAncestorInterfaces();
                }
                
                if (shouldUsingNamingConvention)
                {
                    registrationBuilder = registrationBuilder?.AsConventionNamedInterface();
                    registrationGenericBuilder = registrationGenericBuilder?.AsConventionNamedInterface();
                }

                foreach (var asType in registerAsTypes)
                {
                    if (asType is null) throw new InvalidOperationException("Type was null during registration");

                    registrationBuilder = registrationBuilder?.As(asType);
                    registrationGenericBuilder = registrationGenericBuilder?.As(asType);
                }

                switch (scope)
                {
                    case ServiceLifetime.SingleInstance:
                        registrationBuilder = registrationBuilder?.SingleInstance();
                        registrationGenericBuilder = registrationGenericBuilder?.SingleInstance();
                        break;
                    case ServiceLifetime.InstancePerRequest:
                        registrationBuilder = registrationBuilder?.InstancePerRequest();
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerRequest();
                        break;
                    case ServiceLifetime.InstancePerLifetimeScope:
                        registrationBuilder = registrationBuilder?.InstancePerLifetimeScope();
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerLifetimeScope();
                        break;
                    case ServiceLifetime.InstancePerDependency:
                        registrationBuilder = registrationBuilder?.InstancePerDependency();
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerDependency();
                        break;
                    case ServiceLifetime.InstancePerMatchingLifetimeScope:
                        registrationBuilder =
                            registrationBuilder?.InstancePerMatchingLifetimeScope(scopeAttr?.Tags?.ToArray() ??
                                Array.Empty<object>());
                        registrationGenericBuilder =
                            registrationGenericBuilder?.InstancePerMatchingLifetimeScope(
                                scopeAttr?.Tags?.ToArray() ?? Array.Empty<object>());
                        break;
                    case ServiceLifetime.InstancePerOwned:
                        if (scopeAttr?.Owned is null) throw new InvalidOperationException("Owned type was null");

                        registrationBuilder = registrationBuilder?.InstancePerOwned(scopeAttr.Owned);
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerOwned(scopeAttr.Owned);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(scope));
                }

                foreach (var interceptor in intrAttrs.SelectMany(x => x.Interceptors)
                             .Concat(intrEnableAttr?.Interceptors ?? Type.EmptyTypes).Distinct())
                {
                    registrationBuilder = IsInterceptorAsync(interceptor)
                        ? registrationBuilder?.InterceptedBy(
                            typeof(AsyncInterceptorAdapter<>).MakeGenericType(interceptor))
                        : registrationBuilder?.InterceptedBy(interceptor);
                    registrationGenericBuilder = IsInterceptorAsync(interceptor)
                        ? registrationGenericBuilder?.InterceptedBy(
                            typeof(AsyncInterceptorAdapter<>).MakeGenericType(interceptor))
                        : registrationGenericBuilder?.InterceptedBy(interceptor);
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
                    registerAsTypes.ForEach(x => serviceTypes.Add(x));
                if (shouldAsInterfaces)
                    type.GetInterfaces().Where(x => x != typeof(IDisposable) && x != typeof(IAsyncDisposable)).ToList()
                        .ForEach(x => serviceTypes.Add(x));
                if (shouldAsDirectAncestors)
                    type.GetDirectInterfaceAncestors()
                        .Where(x => x != typeof(IDisposable) && x != typeof(IAsyncDisposable)).ToList()
                        .ForEach(x => serviceTypes.Add(x));
                if (shouldUsingNamingConvention)
                    serviceTypes.Add(type.GetInterfaceByNamingConvention() ?? throw new InvalidOperationException("Couldn't find an interface by naming convention"));

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
    /// <param name="assembliesContainingTypesToScan">Assemblies containing types to scan for services.</param>
    /// <param name="options">Optional configuration.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection,
        IEnumerable<Type> assembliesContainingTypesToScan, Action<AttributeRegistrationOptions>? options = null)
        => AddAttributeDefinedServices(serviceCollection, assembliesContainingTypesToScan.Select(x => x.Assembly),
            options);

    /// <summary>
    /// Registers services with <see cref="ContainerBuilder"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="assembliesToScan">Assemblies to scan for services.</param>
    /// <param name="options">Optional configuration.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, IEnumerable<Assembly> assembliesToScan, Action<AttributeRegistrationOptions>? options = null)
    {
        var config = new AttributeRegistrationOptions(serviceCollection);
        options?.Invoke(config);

        foreach (var assembly in assembliesToScan)
        {
            var set = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(false).Any(y => y is ServiceImplementationAttribute) &&
                            x.IsClass && !x.IsAbstract)
                .ToList(); ;

            foreach (var type in set)
            {
                var attr = type.GetCustomAttribute<ServiceImplementationAttribute>(false);
                if (attr is null)
                    throw new InvalidOperationException();
                
                var scopeAttr = type.GetCustomAttribute<LifetimeAttribute>(false);
                var asAttrs = type.GetCustomAttributes<RegisterAsAttribute>(false).ToList();
                
                var scope = scopeAttr?.ServiceLifetime ?? attr?.ServiceLifetime ?? config.DefaultServiceLifetime;

                var registerAsTypes = asAttrs.Where(x => x.ServiceTypes is not null)
                    .SelectMany(x => x.ServiceTypes ?? Type.EmptyTypes)
                    .Concat(attr?.ServiceTypes ?? Type.EmptyTypes)
                    .Distinct()
                    .ToList();

                var shouldAsSelf = (asAttrs.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsSelf) ||
                                    attr?.RegistrationStrategy is RegistrationStrategy.AsSelf) &&
                                   registerAsTypes.All(y => y != type);

                var shouldAsInterfaces = asAttrs.Any(x =>
                                             x.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces) ||
                                         attr?.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces;
                
                var shouldAsDirectAncestors =
                    asAttrs.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces) ||
                    attr?.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces;

                var shouldUsingNamingConvention =
                    asAttrs.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface) ||
                    attr?.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface;

                if (!shouldAsInterfaces && !registerAsTypes.Any() && !shouldAsDirectAncestors &&
                    !shouldUsingNamingConvention)
                    shouldAsSelf = true;

                var interfaces = type.GetInterfaces().ToList();

                switch (scope)
                {
                    case ServiceLifetime.SingleInstance:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.AddSingleton(x, type));
                        if (shouldAsSelf)
                            serviceCollection.AddSingleton(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.AddSingleton(x, type));
                        if (shouldAsDirectAncestors)
                            type.GetDirectInterfaceAncestors()
                                .Where(x => x != typeof(IDisposable) && x != typeof(IAsyncDisposable)).ToList()
                                .ForEach(x => serviceCollection.AddSingleton(x, type));
                        if (shouldUsingNamingConvention)
                            serviceCollection.AddSingleton(type.GetInterfaceByNamingConvention() ?? throw new ArgumentException(
                                "Couldn't find an implemented interface that follows the naming convention"), type);
                        break;
                    case ServiceLifetime.InstancePerRequest:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.AddScoped(x, type));
                        if (shouldAsSelf)
                            serviceCollection.AddScoped(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.AddScoped(x, type));
                        if (shouldAsDirectAncestors)
                            type.GetDirectInterfaceAncestors()
                                .Where(x => x != typeof(IDisposable) && x != typeof(IAsyncDisposable)).ToList()
                                .ForEach(x => serviceCollection.AddScoped(x, type));
                        if (shouldUsingNamingConvention)
                            serviceCollection.AddScoped(type.GetInterfaceByNamingConvention() ?? throw new ArgumentException(
                                "Couldn't find an implemented interface that follows the naming convention"), type);
                        break;
                    case ServiceLifetime.InstancePerLifetimeScope:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.AddScoped(x, type));
                        if (shouldAsSelf)
                            serviceCollection.AddScoped(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.AddScoped(x, type));
                        if (shouldAsDirectAncestors)
                            type.GetDirectInterfaceAncestors()
                                .Where(x => x != typeof(IDisposable) && x != typeof(IAsyncDisposable)).ToList()
                                .ForEach(x => serviceCollection.AddScoped(x, type));
                        if (shouldUsingNamingConvention)
                            serviceCollection.AddScoped(type.GetInterfaceByNamingConvention() ?? throw new ArgumentException(
                                "Couldn't find an implemented interface that follows the naming convention"), type);
                        break;
                    case ServiceLifetime.InstancePerDependency:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.AddTransient(x, type));
                        if (shouldAsSelf)
                            serviceCollection.AddTransient(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.AddTransient(x, type));
                        if (shouldAsDirectAncestors)
                            type.GetDirectInterfaceAncestors()
                                .Where(x => x != typeof(IDisposable) && x != typeof(IAsyncDisposable)).ToList()
                                .ForEach(x => serviceCollection.AddTransient(x, type));
                        if (shouldUsingNamingConvention)
                            serviceCollection.AddTransient(type.GetInterfaceByNamingConvention() ?? throw new ArgumentException(
                                "Couldn't find an implemented interface that follows the naming convention"), type);
                        break;
                    case ServiceLifetime.InstancePerMatchingLifetimeScope:
                        throw new NotSupportedException();
                    case ServiceLifetime.InstancePerOwned:
                        throw new NotSupportedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(scope));
                }
            }
        }

        return serviceCollection;
    }
    
    /// <summary>
    /// Whether given interceptor is an async interceptor.
    /// </summary>
    private static bool IsInterceptorAsync(Type interceptor) => interceptor.GetInterfaces().Any(x => x == typeof(IAsyncInterceptor));
}
