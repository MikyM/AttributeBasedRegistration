using System.Reflection;
using AttributeBasedRegistration.Attributes;
using AttributeBasedRegistration.Attributes.Abstractions;
using AttributeBasedRegistration.Extensions;
using Autofac;
using Autofac.Builder;
using Autofac.Core.Activators.Reflection;
using Autofac.Extensions.DependencyInjection;
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
    /// <param name="assembliesContainingTypesToScan">Assemblies containing types to scan for services.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static ContainerBuilder AddAttributeDefinedServices(this ContainerBuilder builder, params Type[] assembliesContainingTypesToScan)
        => AddAttributeDefinedServices(builder, assembliesContainingTypesToScan, null);
    
    /// <summary>
    /// Registers services with <see cref="ContainerBuilder"/> via attributes.
    /// </summary>
    /// <param name="builder">Current builder instance.</param>
    /// <param name="assembliesToScan">Assemblies to scan for services.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static ContainerBuilder AddAttributeDefinedServices(this ContainerBuilder builder, params Assembly[] assembliesToScan)
        => AddAttributeDefinedServices(builder, assembliesToScan, null);
    
    /// <summary>
    /// Registers services with <see cref="ContainerBuilder"/> via attributes.
    /// </summary>
    /// <param name="builder">Current builder instance.</param>
    /// <param name="options">Configuration.</param>
    /// <param name="assembliesContainingTypesToScan">Assemblies containing types to scan for services.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static ContainerBuilder AddAttributeDefinedServices(this ContainerBuilder builder, Action<AttributeRegistrationOptions> options, params Type[] assembliesContainingTypesToScan)
        => AddAttributeDefinedServices(builder, assembliesContainingTypesToScan, options);
    
    /// <summary>
    /// Registers services with <see cref="ContainerBuilder"/> via attributes.
    /// </summary>
    /// <param name="builder">Current builder instance.</param>
    /// <param name="options">Configuration.</param>
    /// <param name="assembliesToScan">Assemblies to scan for services.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static ContainerBuilder AddAttributeDefinedServices(this ContainerBuilder builder, Action<AttributeRegistrationOptions> options, params Assembly[] assembliesToScan)
        => AddAttributeDefinedServices(builder, assembliesToScan, options);

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
                var implementationAttributes = type.GetRegistrationAttributesOfType<IServiceImplementationAttribute>().ToArray();
                if (!implementationAttributes.Any())
                    throw new InvalidOperationException();
                if (implementationAttributes.Length > 1)
                    throw new InvalidOperationException($"Only a single implementation attribute is allowed on a type, type: {type.Name}");
                var implementationAttribute = implementationAttributes.First();

                var asAttributes = type.GetRegistrationAttributesOfType<IRegisterAsAttribute>().ToArray();
                var constructorAttributes = type.GetRegistrationAttributesOfType<IFindConstructorsWithAttribute>().ToArray();
                var enableInterceptionAttribute = type.GetRegistrationAttributesOfType<IEnableInterceptionAttribute>().FirstOrDefault();

                if (constructorAttributes.Any() && enableInterceptionAttribute is not null)
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
                
                registrationBuilder.HandleConstructorSelecting(type);
                registrationGenericBuilder.HandleConstructorSelecting(type);

                registrationBuilder.HandleInterceptors(builder, type);
                registrationGenericBuilder.HandleInterceptors(builder, type);
                
                builder.HandleDecoration(type, shouldAsSelf, registerAsTypes, shouldAsInterfaces,
                    shouldAsDirectAncestors, shouldUsingNamingConvention);
            }
        }

        return builder;
    }

    private static List<Type> GetServiceTypes(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes)
        => asAttributes.Where(x => x.ServiceTypes is not null)
            .SelectMany(x => x.ServiceTypes ?? Type.EmptyTypes)
            .Concat(implementationAttribute?.ServiceTypes ?? Type.EmptyTypes)
            .Distinct()
            .ToList();
    
    private static bool ShouldAsInterfaces(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes)
        => asAttributes.Any(x =>
               x.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces) ||
           implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces;
    
    private static bool ShouldAsSelf(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes, IEnumerable<Type> serviceTypes)
        => (asAttributes.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsSelf) ||
            implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsSelf) &&
           serviceTypes.All(y => y != type);
    
    private static bool ShouldAsDirectAncestors(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes)
        => asAttributes.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces) ||
           implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces;
    
    private static bool ShouldUsingNamingConvention(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes)
        => asAttributes.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface) ||
           implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface;

    private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>?
        HandleLifetime<TRegistrationStyle, TActivatorData>(
            this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>? builder, Type type,
            IServiceImplementationAttribute implementationAttribute, AttributeRegistrationOptions options)
        where TActivatorData : ReflectionActivatorData
    {
        var lifetimeAttributes = type.GetRegistrationAttributesOfType<ILifetimeAttribute>().ToArray();

        if (lifetimeAttributes.Length > 1)
            throw new InvalidOperationException($"Only a single Lifetime attribute is allowed on a type, type: {type.Name}");

        var lifetimeAttribute = lifetimeAttributes.FirstOrDefault();
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
        var enableInterceptionAttributes = type.GetRegistrationAttributesOfType<IEnableInterceptionAttribute>().ToArray();
        if (enableInterceptionAttributes.Length > 1)
            throw new InvalidOperationException($"Only a single enable interception attribute is allowed on a type, type: {type.Name}");
        var enableInterceptionAttribute = enableInterceptionAttributes.FirstOrDefault();
        
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
        var enableInterceptionAttributes = type.GetRegistrationAttributesOfType<IEnableInterceptionAttribute>().ToArray();
        if (enableInterceptionAttributes.Length > 1)
            throw new InvalidOperationException($"Only a single enable interception attribute is allowed on a type, type: {type.Name}");
        if (enableInterceptionAttributes.Length == 0)
            return builder;
        
        var interceptedByAttributes = type.GetRegistrationAttributesOfType<IInterceptedByAttribute>().ToArray();
        
        if (interceptedByAttributes.GroupBy(x => x.RegistrationOrder).FirstOrDefault(x => x.Count() > 1) is not null)
            throw new InvalidOperationException($"Duplicated interceptor registration order on type {type.Name}");

        if (interceptedByAttributes.GroupBy(x => x.Interceptor)
                .FirstOrDefault(x => x.Count() > 1) is not null)
            throw new InvalidOperationException($"Duplicated interceptor type on type {type.Name}");

        foreach (var interceptor in interceptedByAttributes.OrderByDescending(x => x.RegistrationOrder)
                     .Select(x => x.Interceptor)
                     .Distinct())
        {
            if (interceptor is null)
                throw new InvalidOperationException();
            
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
        var lifetimeAttributes = type.GetRegistrationAttributesOfType<ILifetimeAttribute>().ToArray();
        if (lifetimeAttributes.Length > 1)
            throw new InvalidOperationException($"Only a single lifetime attribute is allowed on a type, type: {type.Name}");

        var lifetimeAttribute = lifetimeAttributes.FirstOrDefault();
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
    /// Registers the adapter (<see cref="AsyncInterceptorAdapter{TAsyncInterceptor}"/>) for async interceptors.
    /// </summary>
    /// <param name="builder">Container builder.</param>
    public static void RegisterAsyncInterceptorAdapter(this ContainerBuilder builder)
        => builder.RegisterGeneric(typeof(AsyncInterceptorAdapter<>)).InstancePerDependency();

    /// <summary>
    /// Registers all interceptors (sync and async) within provided assemblies.
    /// </summary>
    /// <remarks>
    /// Takes <see cref="LifetimeAttribute"/> into account.
    /// All registrations are <see cref="RegistrationStrategy.AsSelf"/>.
    /// <see cref="ServiceLifetime.InstancePerDependency"/> will be used as a default lifetime.
    /// </remarks>
    /// <param name="containerBuilder">Container builder.</param>
    /// <param name="assembliesToScan">Assemblies to scan for interceptors.</param>
    /// <returns>Container builder instance with registered interceptors.</returns>
    public static ContainerBuilder RegisterInterceptors(this ContainerBuilder containerBuilder,
        params Assembly[] assembliesToScan)
        => RegisterInterceptors(containerBuilder, assembliesToScan, ServiceLifetime.InstancePerDependency);

    /// <summary>
    /// Registers all interceptors (sync and async) within provided assemblies.
    /// </summary>
    /// <remarks>
    /// Takes <see cref="LifetimeAttribute"/> into account.
    /// All registrations are <see cref="RegistrationStrategy.AsSelf"/>.
    /// <see cref="ServiceLifetime.InstancePerDependency"/> will be used as a default lifetime.
    /// </remarks>
    /// <param name="containerBuilder">Container builder.</param>
    /// <param name="defaultLifetime">Default interceptor lifetime.</param>
    /// <param name="assembliesToScan">Assemblies to scan for interceptors.</param>
    /// <returns>Container builder instance with registered interceptors.</returns>
    public static ContainerBuilder RegisterInterceptors(this ContainerBuilder containerBuilder, ServiceLifetime defaultLifetime,
        params Assembly[] assembliesToScan)
        => RegisterInterceptors(containerBuilder, assembliesToScan, defaultLifetime);
    
    /// <summary>
    /// Registers all interceptors (sync and async) within provided assemblies.
    /// </summary>
    /// <remarks>
    /// Takes <see cref="LifetimeAttribute"/> into account.
    /// All registrations are <see cref="RegistrationStrategy.AsSelf"/>.
    /// <see cref="ServiceLifetime.InstancePerDependency"/> will be used as a default lifetime.
    /// </remarks>
    /// <param name="containerBuilder">Container builder.</param>
    /// <param name="assembliesContainingTypesToScan">Assemblies that contain given types to scan for interceptors.</param>
    /// <returns>Container builder instance with registered interceptors.</returns>
    public static ContainerBuilder RegisterInterceptors(this ContainerBuilder containerBuilder,
        params Type[] assembliesContainingTypesToScan)
        => RegisterInterceptors(containerBuilder, assembliesContainingTypesToScan, ServiceLifetime.InstancePerDependency);

    /// <summary>
    /// Registers all interceptors (sync and async) within provided assemblies.
    /// </summary>
    /// <remarks>
    /// Takes <see cref="LifetimeAttribute"/> into account.
    /// All registrations are <see cref="RegistrationStrategy.AsSelf"/>.
    /// <see cref="ServiceLifetime.InstancePerDependency"/> will be used as a default lifetime.
    /// </remarks>
    /// <param name="containerBuilder">Container builder.</param>
    /// <param name="defaultLifetime">Default interceptor lifetime.</param>
    /// <param name="assembliesContainingTypesToScan">Assemblies that contain given types to scan for interceptors.</param>
    /// <returns>Container builder instance with registered interceptors.</returns>
    public static ContainerBuilder RegisterInterceptors(this ContainerBuilder containerBuilder, ServiceLifetime defaultLifetime,
        params Type[] assembliesContainingTypesToScan)
        => RegisterInterceptors(containerBuilder, assembliesContainingTypesToScan, defaultLifetime);

    /// <summary>
    /// Registers all interceptors (sync and async) within provided assemblies.
    /// </summary>
    /// <remarks>
    /// Takes <see cref="LifetimeAttribute"/> into account.
    /// All registrations are <see cref="RegistrationStrategy.AsSelf"/>.
    /// </remarks>
    /// <param name="containerBuilder">Container builder.</param>
    /// <param name="assembliesContainingTypesToScan">Assemblies that contain given types to scan for interceptors.</param>
    /// <param name="defaultLifetime">Default interceptor lifetime.</param>
    /// <returns>Container builder instance with registered interceptors.</returns>
    public static ContainerBuilder RegisterInterceptors(this ContainerBuilder containerBuilder,
        IEnumerable<Type> assembliesContainingTypesToScan,
        ServiceLifetime defaultLifetime = ServiceLifetime.InstancePerDependency)
        => RegisterInterceptors(containerBuilder, assembliesContainingTypesToScan.Select(x => x.Assembly),
            defaultLifetime);
        
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
        containerBuilder.RegisterAsyncInterceptorAdapter();
        
        foreach (var assembly in assembliesToScan)
        {
            var interceptors = assembly.GetTypes().Where(x => x.IsInterceptorImplementation() && !x.ShouldSkipRegistration<ISkipInterceptorRegistrationAttribute>());

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
        var findCtorAttributes = type.GetRegistrationAttributesOfType<IFindConstructorsWithAttribute>().ToArray();
        if (!findCtorAttributes.Any())
            return builder;

        if (findCtorAttributes.Length > 1)
            throw new InvalidOperationException($"Only one ctor finder can be set on a type - {type.Name}");

        var instance = Activator.CreateInstance(findCtorAttributes.First().ConstructorFinder);
        if (instance is null)
            throw new InvalidOperationException(
                $"Couldn't create an instance of a custom ctor finder for type {type.Name}, only finders with parameterless ctors are supported");


        return builder?.FindConstructorsWith((IConstructorFinder)instance);
    }
    
    private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>?
        HandleConstructorSelecting<TRegistrationStyle, TActivatorData>(
            this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle>? builder, Type type)
        where TActivatorData : ReflectionActivatorData
    {
        var selectCtorAttributes = type.GetRegistrationAttributesOfType<ISelectConstructorsWithAttribute>().ToArray();
        if (!selectCtorAttributes.Any())
            return builder;

        if (selectCtorAttributes.Length > 1)
            throw new InvalidOperationException($"Only one ctor selector can be set on a type - {type.Name}");

        var instance = Activator.CreateInstance(selectCtorAttributes.First().ConstructorSelector);
        if (instance is null)
            throw new InvalidOperationException(
                $"Couldn't create an instance of a custom ctor selector for type {type.Name}, only selectors with parameterless ctors are supported");


        return builder?.UsingConstructor((IConstructorSelector)instance);
    }

    private static ContainerBuilder HandleDecoration(this ContainerBuilder builder, Type type, bool shouldAsSelf, List<Type> registerAsTypes, bool shouldAsInterfaces,
        bool shouldAsDirectAncestors, bool shouldUsingNamingConvention)
    {
        var decoratorAttributes = type.GetRegistrationAttributesOfType<IDecoratedByAttribute>().ToArray();
        if (!decoratorAttributes.Any())
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

        
        if (decoratorAttributes.GroupBy(x => x.RegistrationOrder).FirstOrDefault(x => x.Count() > 1) is not null)
            throw new InvalidOperationException($"Duplicated decorator registration order on type {type.Name}");

        if (decoratorAttributes.GroupBy(x => x.Decorator)
                .FirstOrDefault(x => x.Count() > 1) is not null)
            throw new InvalidOperationException($"Duplicated decorator type on type {type.Name}");
        
        foreach (var attribute in decoratorAttributes.OrderBy(x => x.RegistrationOrder))
        {
            if (attribute.Decorator.ShouldSkipRegistration<ISkipDecoratorRegistrationAttribute>())
                continue;
            
            if (attribute.Decorator.IsGenericType && attribute.Decorator.IsGenericTypeDefinition)
            {
                foreach (var serviceType in serviceTypes)
                    builder.RegisterGenericDecorator(attribute.Decorator, serviceType);
            }
            else
            {
                foreach (var serviceType in serviceTypes)
                    builder.RegisterDecorator(attribute.Decorator, serviceType);
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
        if (typeof(T).IsAsyncInterceptor())
            builder.RegisterAsyncInterceptorAdapter();
        
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
        if (typeof(T).IsAsyncInterceptor())
            builder.RegisterAsyncInterceptorAdapter();
        
        builder.RegisterInstance(instance);

        return builder;
    }

    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
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
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="assembliesContainingTypesToScan">Assemblies containing types to scan for services.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, params Type[] assembliesContainingTypesToScan)
        => AddAttributeDefinedServices(serviceCollection, assembliesContainingTypesToScan, null);
    
    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="options">Configuration.</param>
    /// <param name="assembliesContainingTypesToScan">Assemblies containing types to scan for services.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, Action<AttributeRegistrationOptions> options, params Type[] assembliesContainingTypesToScan)
        => AddAttributeDefinedServices(serviceCollection, assembliesContainingTypesToScan, options);
    
    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="options">Configuration.</param>
    /// <param name="assembliesToScan">Assemblies to scan for services.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, Action<AttributeRegistrationOptions> options, params Assembly[] assembliesToScan)
        => AddAttributeDefinedServices(serviceCollection, assembliesToScan, options);
    
    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="assembliesToScan">Assemblies to scan for services.</param>
    /// <returns>Current <see cref="ContainerBuilder"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, params Assembly[] assembliesToScan)
        => AddAttributeDefinedServices(serviceCollection, assembliesToScan, null);

    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
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
                var implementationAttributes = type.GetRegistrationAttributesOfType<IServiceImplementationAttribute>().ToArray();
                if (!implementationAttributes.Any())
                    throw new InvalidOperationException();
                if (implementationAttributes.Length > 1)
                    throw new InvalidOperationException($"Only a single implementation attribute is allowed on a type, type: {type.Name}");
                var implementationAttribute = implementationAttributes.First();

                var asAttributes = type.GetRegistrationAttributesOfType<IRegisterAsAttribute>().ToArray();
                var lifetimeAttributes = type.GetRegistrationAttributesOfType<ILifetimeAttribute>().ToArray();
                if (lifetimeAttributes.Length > 1)
                    throw new InvalidOperationException($"Only a single lifetime attribute is allowed on a type, type: {type.Name}");

                var lifetimeAttribute = lifetimeAttributes.FirstOrDefault();
                var lifetime = lifetimeAttribute?.ServiceLifetime ?? implementationAttribute.ServiceLifetime ?? config.DefaultServiceLifetime;

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
    /// Adds the identifier services of the root DI scope making <see cref="DependancyInjectionExtensions.IsRootScope(IServiceProvider)"/> available.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    public static IServiceCollection AddRootScopeIdentifier(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<RootScopeWrapper>();
        serviceCollection.AddHostedService<RootScopeWrapperStarter>();

        return serviceCollection;
    }

    /// <summary>
    /// Whether given interceptor is an async interceptor.
    /// </summary>
    private static bool IsAsyncInterceptor(this Type interceptorCandidate) => interceptorCandidate.GetInterfaces().Any(x => x == typeof(IAsyncInterceptor));
    
    /// <summary>
    /// Checks whether the given scope is a root scope.
    /// </summary>
    /// <param name="serviceProvider">Root scope candidate.</param>
    /// <returns>True if the given scope is the root scope, otherwise false.</returns>
    public static bool IsRootScope(this IServiceProvider serviceProvider)
    {
        if (serviceProvider is AutofacServiceProvider asp && asp.LifetimeScope.Tag is "root")
            return true;
        
        var trueRoot = serviceProvider.GetService<RootScopeWrapper>()?.ServiceProvider;
        if (trueRoot is null)
            throw new InvalidOperationException(
                "You must register root scope identifiers with AddRootScopeIdentifier extension method to be able to use IsRootScope method");

        return serviceProvider == trueRoot;
    }
    
    /// <summary>
    /// Checks whether the given scope is a root scope.
    /// </summary>
    /// <param name="lifetimeScope">Root scope candidate.</param>
    /// <returns>True if the given scope is the root scope, otherwise false.</returns>
    public static bool IsRootScope(this ILifetimeScope lifetimeScope)
        => lifetimeScope.Tag is "root";
}
