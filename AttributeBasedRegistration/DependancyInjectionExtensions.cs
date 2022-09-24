using System.Reflection;
using AttributeBasedRegistration.Attributes;
using AttributeBasedRegistration.Extensions;
using Autofac;
using Autofac.Builder;
using Autofac.Core.Activators.Reflection;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        var config = new AttributeRegistrationOptions();
        options?.Invoke(config);

        builder.RegisterGeneric(typeof(AsyncInterceptorAdapter<>)).InstancePerDependency();

        var toScan = assembliesToScan.ToList();

        var iOptions = Options.Create(config);
        builder.RegisterInstance(iOptions).As<IOptions<AttributeRegistrationOptions>>().SingleInstance();
        builder.RegisterInstance(iOptions.Value).As<AttributeRegistrationOptions>().SingleInstance();

        foreach (var assembly in toScan)
        {
            var set = assembly.GetTypes()
                .Where(x => x.IsServiceImplementation())
                .ToList();

            foreach (var type in set)
            {
                var implementationAttribute = type.GetCustomAttribute<ServiceImplementationAttribute>(false);
                if (implementationAttribute is null)
                    throw new InvalidOperationException();
                
                var asAttributes = type.GetCustomAttributes<RegisterAsAttribute>(false).ToList();
                var constructorAttribute = type.GetCustomAttribute<FindConstructorsWithAttribute>(false);
                var enableInterceptionAttribute = type.GetCustomAttribute<EnableInterceptionAttribute>(false);

                if (constructorAttribute is not null && enableInterceptionAttribute is not null)
                    throw new InvalidOperationException(
                        "Using a custom constructor finder will prevent interception from happening");

                var registerAsTypes = type.GetServiceTypes(implementationAttribute, asAttributes);

                var shouldAsSelf = type.ShouldAsSelf(implementationAttribute, asAttributes, registerAsTypes);
                var shouldAsInterfaces = type.ShouldAsInterfaces(implementationAttribute, asAttributes);
                var shouldAsDirectAncestors = type.ShouldAsDirectAncestors(implementationAttribute, asAttributes);
                var shouldUsingNamingConvention = type.ShouldUsingNamingConvention(implementationAttribute, asAttributes);

                if (!shouldAsInterfaces && !registerAsTypes.Any() && !shouldAsDirectAncestors &&
                    !shouldUsingNamingConvention)
                    shouldAsSelf = true;

                var (registrationGenericBuilder, registrationBuilder) =
                    builder.HandleInitialRegistration(type, shouldAsInterfaces);

                registrationBuilder.HandleRegistrationOptions(type, shouldAsSelf,
                    shouldAsDirectAncestors, shouldUsingNamingConvention, registerAsTypes);
                registrationGenericBuilder.HandleRegistrationOptions(type, shouldAsSelf,
                    shouldAsDirectAncestors, shouldUsingNamingConvention, registerAsTypes);

                registrationBuilder.HandleLifetime(type, implementationAttribute, config);
                registrationGenericBuilder.HandleLifetime(type, implementationAttribute, config);

                registrationBuilder.HandleConstructorFinding(type);
                registrationGenericBuilder.HandleConstructorFinding(type);

                registrationBuilder.HandleInterceptors(builder, type);
                registrationGenericBuilder.HandleInterceptors(builder, type);
                
                builder.HandleDecoration(type, shouldAsSelf, registerAsTypes, shouldAsInterfaces,
                    shouldAsDirectAncestors, shouldUsingNamingConvention);
            }
        }

        return builder;
    }
    
    private static bool IsServiceImplementation(this Type type)
        => type.GetCustomAttributes(false).Any(y => y is ServiceImplementationAttribute) &&
           type.IsClass && !type.IsAbstract;
    
    private static bool IsInterceptorImplementation(this Type type)
        => type.IsClass && !type.IsAbstract && type.GetCustomAttribute<SkipInterceptorRegistrationAttribute>() is null && type.GetInterfaces().Any(x => x == typeof(IInterceptor) || x == typeof(IAsyncInterceptor));

    private static List<Type> GetServiceTypes(this Type type, ServiceImplementationAttribute? implementationAttribute, IEnumerable<RegisterAsAttribute> asAttributes)
        => asAttributes.Where(x => x.ServiceTypes is not null)
            .SelectMany(x => x.ServiceTypes ?? Type.EmptyTypes)
            .Concat(implementationAttribute?.ServiceTypes ?? Type.EmptyTypes)
            .Distinct()
            .ToList();
    
    private static bool ShouldAsInterfaces(this Type type, ServiceImplementationAttribute? implementationAttribute, IEnumerable<RegisterAsAttribute> asAttributes)
        => asAttributes.Any(x =>
               x.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces) ||
           implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces;
    
    private static bool ShouldAsSelf(this Type type, ServiceImplementationAttribute? implementationAttribute, IEnumerable<RegisterAsAttribute> asAttributes, IEnumerable<Type> serviceTypes)
        => (asAttributes.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsSelf) ||
            implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsSelf) &&
           serviceTypes.All(y => y != type);
    
    private static bool ShouldAsDirectAncestors(this Type type, ServiceImplementationAttribute? implementationAttribute, IEnumerable<RegisterAsAttribute> asAttributes)
        => asAttributes.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces) ||
           implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces;
    
    private static bool ShouldUsingNamingConvention(this Type type, ServiceImplementationAttribute? implementationAttribute, IEnumerable<RegisterAsAttribute> asAttributes)
        => asAttributes.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface) ||
           implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface;

    private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>?
        HandleLifetime<TRegistrationStyle, TActivatorData>(
            this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>? builder, Type type,
            ServiceImplementationAttribute implementationAttribute, AttributeRegistrationOptions options)
        where TActivatorData : ReflectionActivatorData
    {
        var lifetimeAttribute = type.GetCustomAttribute<LifetimeAttribute>(false);

        var lifetime = lifetimeAttribute?.ServiceLifetime ??
                       implementationAttribute?.ServiceLifetime ?? options.DefaultServiceLifetime;

        switch (lifetime)
        {
            case ServiceLifetime.SingleInstance:
                builder?.SingleInstance();
                break;
            case ServiceLifetime.InstancePerRequest:
                builder?.InstancePerRequest();
                break;
            case ServiceLifetime.InstancePerLifetimeScope:
                builder?.InstancePerLifetimeScope();
                break;
            case ServiceLifetime.InstancePerDependency:
                builder?.InstancePerDependency();
                break;
            case ServiceLifetime.InstancePerMatchingLifetimeScope:
                builder?.InstancePerMatchingLifetimeScope(lifetimeAttribute?.Tags?.ToArray() ??
                                                          Array.Empty<object>());
                break;
            case ServiceLifetime.InstancePerOwned:
                if (lifetimeAttribute?.Owned is null) throw new InvalidOperationException("Owned type was null");

                builder?.InstancePerOwned(lifetimeAttribute.Owned);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime));
        }

        return builder;
    }

    private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>?
        HandleRegistrationOptions<TRegistrationStyle, TActivatorData>(
            this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>? builder, Type type,
            bool shouldAsSelf, bool shouldAsDirectAncestors, bool shouldUsingNamingConvention,
            IEnumerable<Type> registerAsTypes)
        where TActivatorData : ReflectionActivatorData
    {
        if (shouldAsSelf)
            builder?.As(type);

        if (shouldAsDirectAncestors)
            builder?.AsDirectAncestorInterfaces();
                
        if (shouldUsingNamingConvention)
            builder?.AsConventionNamedInterface();

        foreach (var asType in registerAsTypes)
            builder?.As(asType);

        return builder;
    }


    private static (IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>? GenericBuilder,
        IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>? Builder)
        HandleInitialRegistration(
            this ContainerBuilder builder, Type type, bool shouldAsInterfaces)
    {
        var enableInterceptionAttribute = type.GetCustomAttribute<EnableInterceptionAttribute>(false);
        
        IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>?
            registrationGenericBuilder = null;
        IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>?
            registrationBuilder = null;

        if (type.IsGenericType && type.IsGenericTypeDefinition)
        {
            if (enableInterceptionAttribute is not null)
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
            if (enableInterceptionAttribute is not null)
            {
                registrationBuilder = enableInterceptionAttribute.InterceptionStrategy switch
                {
                    InterceptionStrategy.Interface => shouldAsInterfaces
                        ? builder.RegisterType(type).AsImplementedInterfaces().EnableInterfaceInterceptors()
                        : builder.RegisterType(type).EnableInterfaceInterceptors(),
                    InterceptionStrategy.Class => shouldAsInterfaces
                        ? builder.RegisterType(type).AsImplementedInterfaces().EnableClassInterceptors()
                        : builder.RegisterType(type).EnableClassInterceptors(),
                    _ => throw new ArgumentOutOfRangeException(nameof(enableInterceptionAttribute.InterceptionStrategy))
                };
            }
            else
            {
                registrationBuilder = shouldAsInterfaces
                    ? builder.RegisterType(type).AsImplementedInterfaces()
                    : builder.RegisterType(type);
            }
        }

        return (registrationGenericBuilder, registrationBuilder);
    }

    private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>?
        HandleInterceptors<TRegistrationStyle, TActivatorData>(
            this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>? builder, ContainerBuilder containerBuilder, Type type)
        where TActivatorData : ReflectionActivatorData
    {
        var enableAttribute = type.GetCustomAttribute<EnableInterceptionAttribute>(false);
        if (enableAttribute is null)
            return builder;
        
        var interceptedByAttributed = type.GetCustomAttributes<InterceptedByAttribute>(false).ToList();

        foreach (var interceptor in interceptedByAttributed.OrderByDescending(x => x.RegistrationOrder).Select(x => x.Interceptor)
                     .Distinct())
        {
            builder = interceptor.IsAsyncInterceptor()
                ? builder?.InterceptedBy(
                    typeof(AsyncInterceptorAdapter<>).MakeGenericType(interceptor))
                : builder?.InterceptedBy(interceptor);
        }

        return builder;
    }
    
    private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>?
        HandleInterceptorLifetime<TRegistrationStyle, TActivatorData>(
            this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>? builder, Type type, ServiceLifetime defaultLifetime)
        where TActivatorData : ReflectionActivatorData
    {
        var lifetimeAttribute = type.GetCustomAttribute<LifetimeAttribute>(false);

        var lifetime = lifetimeAttribute?.ServiceLifetime ?? defaultLifetime;

        switch (lifetime)
        {
            case ServiceLifetime.SingleInstance:
                builder?.SingleInstance();
                break;
            case ServiceLifetime.InstancePerRequest:
                builder?.InstancePerRequest();
                break;
            case ServiceLifetime.InstancePerLifetimeScope:
                builder?.InstancePerLifetimeScope();
                break;
            case ServiceLifetime.InstancePerDependency:
                builder?.InstancePerDependency();
                break;
            case ServiceLifetime.InstancePerMatchingLifetimeScope:
                builder?.InstancePerMatchingLifetimeScope(lifetimeAttribute?.Tags?.ToArray() ??
                                                          Array.Empty<object>());
                break;
            case ServiceLifetime.InstancePerOwned:
                if (lifetimeAttribute?.Owned is null) throw new InvalidOperationException("Owned type was null");

                builder?.InstancePerOwned(lifetimeAttribute.Owned);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime));
        }

        return builder;
    }


    /// <summary>
    /// Registers all interceptors (sync and async) within provided assemblies.
    /// </summary>
    /// <remarks>
    /// Takes <see cref="LifetimeAttribute"/> into account.
    /// All registrations are <see cref="RegistrationStrategy.AsSelf"/>.
    /// </remarks>
    /// <param name="containerBuilder">Container builder.</param>
    /// <param name="assembliesToScan">Assemblies to scan for interceptors.</param>
    /// <param name="defaultLifetime">Default interceptor lifetime.</param>
    /// <returns>Container builder instance with registered interceptors.</returns>
    public static ContainerBuilder RegisterInterceptors(this ContainerBuilder containerBuilder, IEnumerable<Assembly> assembliesToScan, ServiceLifetime defaultLifetime = ServiceLifetime.InstancePerDependency)
    {
        foreach (var assembly in assembliesToScan)
        {
            var interceptors = assembly.GetTypes().Where(x => x.IsInterceptorImplementation());

            foreach (var interceptor in interceptors)
            {
                if (interceptor.IsGenericType && interceptor.IsGenericTypeDefinition)
                    containerBuilder.RegisterGeneric(interceptor).HandleInterceptorLifetime(interceptor, defaultLifetime);
                else
                    containerBuilder.RegisterType(interceptor).HandleInterceptorLifetime(interceptor, defaultLifetime);
            }
        }

        return containerBuilder;
    }

    private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>?
        HandleConstructorFinding<TRegistrationStyle, TActivatorData>(
            this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>? builder, Type type)
        where TActivatorData : ReflectionActivatorData
    {
        var attribute = type.GetCustomAttribute<FindConstructorsWithAttribute>(false);
        if (attribute is null)
            return builder;

        var instance = Activator.CreateInstance(attribute.ConstructorFinder);

        if (instance is null)
            throw new InvalidOperationException(
                $"Couldn't create an instance of a custom ctor finder of type {attribute.ConstructorFinder.Name}, only finders with parameterless ctors are supported");

        return builder?.FindConstructorsWith((IConstructorFinder)instance);
    }

    private static ContainerBuilder HandleDecoration(this ContainerBuilder builder, Type type, bool shouldAsSelf, List<Type> registerAsTypes, bool shouldAsInterfaces,
        bool shouldAsDirectAncestors, bool shouldUsingNamingConvention)
    {
        var attributes = type.GetCustomAttributes<DecoratedByAttribute>(false).ToList();
        
        if (!attributes.Any())
            return builder;

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
            serviceTypes.Add(type.GetInterfaceByNamingConvention() ??
                             throw new InvalidOperationException("Couldn't find an interface by naming convention"));

        foreach (var decAttr in attributes.OrderBy(x => x.RegistrationOrder))
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

        return builder;
    }

    /// <summary>
    /// Registers an interceptor with <see cref="ContainerBuilder"/>.
    /// </summary>
    /// <param name="builder">Container builder.</param>
    /// <param name="factoryMethod">Factory method for the registration.</param>
    /// <param name="lifetime">Lifetime of the interceptor.</param>
    /// <param name="tags">Optional tags for a <see cref="ServiceLifetime.InstancePerMatchingLifetimeScope"/> registration.</param>
    /// <returns>Current instance of the <see cref="AttributeRegistrationOptions"/>.</returns>
    public static ContainerBuilder RegisterInterceptor<T>(this ContainerBuilder builder, Func<IComponentContext, T> factoryMethod, ServiceLifetime lifetime = ServiceLifetime.InstancePerDependency, IEnumerable<object>? tags = null) where T : notnull
    {
        switch (lifetime)
        {
            case ServiceLifetime.SingleInstance:
                builder.Register(factoryMethod).SingleInstance();
                break;
            case ServiceLifetime.InstancePerRequest:
                builder.Register(factoryMethod).InstancePerRequest();
                break;
            case ServiceLifetime.InstancePerLifetimeScope:
                builder.Register(factoryMethod).InstancePerLifetimeScope();
                break;
            case ServiceLifetime.InstancePerDependency:
                builder.Register(factoryMethod).InstancePerDependency();
                break;
            case ServiceLifetime.InstancePerMatchingLifetimeScope:
                builder.Register(factoryMethod).InstancePerMatchingLifetimeScope(tags ?? Array.Empty<object>());
                break;
            case ServiceLifetime.InstancePerOwned:
                throw new NotSupportedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime));
        }

        return builder;
    }

    /// <summary>
    /// Registers an interceptor with <see cref="ContainerBuilder"/>.
    /// </summary>
    /// <param name="builder">Container builder.</param>
    /// <param name="instance">Interceptor instance to register.</param>
    /// <param name="lifetime">Lifetime of the interceptor.</param>
    /// <returns>Current instance of the <see cref="AttributeRegistrationOptions"/>.</returns>
    public static ContainerBuilder RegisterInterceptorInstance<T>(this ContainerBuilder builder, T instance, ServiceLifetime lifetime = ServiceLifetime.InstancePerDependency) where T : class
    {
        builder.RegisterInstance(instance);

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
        var config = new AttributeRegistrationOptions();
        options?.Invoke(config);
        
        var iOptions = Options.Create(config);
        serviceCollection.AddSingleton(iOptions);
        serviceCollection.AddSingleton(iOptions.Value);

        foreach (var assembly in assembliesToScan)
        {
            var set = assembly.GetTypes()
                .Where(x => x.IsServiceImplementation())
                .ToList();

            foreach (var type in set)
            {
                var implementationAttribute = type.GetCustomAttribute<ServiceImplementationAttribute>(false);
                if (implementationAttribute is null)
                    throw new InvalidOperationException();
                
                var lifetimeAttribute = type.GetCustomAttribute<LifetimeAttribute>(false);
                var asAttributes = type.GetCustomAttributes<RegisterAsAttribute>(false).ToList();
                
                var lifetime = lifetimeAttribute?.ServiceLifetime ?? implementationAttribute?.ServiceLifetime ?? config.DefaultServiceLifetime;

                var registerAsTypes = type.GetServiceTypes(implementationAttribute, asAttributes);

                var shouldAsSelf = type.ShouldAsSelf(implementationAttribute, asAttributes, registerAsTypes);
                var shouldAsInterfaces = type.ShouldAsInterfaces(implementationAttribute, asAttributes);
                var shouldAsDirectAncestors = type.ShouldAsDirectAncestors(implementationAttribute, asAttributes);
                var shouldUsingNamingConvention = type.ShouldUsingNamingConvention(implementationAttribute, asAttributes);

                if (!shouldAsInterfaces && !registerAsTypes.Any() && !shouldAsDirectAncestors &&
                    !shouldUsingNamingConvention)
                    shouldAsSelf = true;

                var interfaces = type.GetInterfaces().ToList();

                switch (lifetime)
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
                        throw new ArgumentOutOfRangeException(nameof(lifetime));
                }
            }
        }

        return serviceCollection;
    }
    
    /// <summary>
    /// Whether given interceptor is an async interceptor.
    /// </summary>
    private static bool IsAsyncInterceptor(this Type interceptorCandidate) => interceptorCandidate.GetInterfaces().Any(x => x == typeof(IAsyncInterceptor));
}
