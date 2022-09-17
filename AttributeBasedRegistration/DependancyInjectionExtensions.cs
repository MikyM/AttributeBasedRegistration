using System.Reflection;
using AttributeBasedRegistration.Attributes;
using Autofac;
using Autofac.Builder;
using Autofac.Core.Activators.Reflection;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
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
        => AddAttributeDefinedServices(builder, assembliesContainingTypesToScan.Select(x => x.Assembly).Distinct(),
            options);
    
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
                var serviceAttr = type.GetCustomAttribute<ServiceImplementationAttribute>(false);
                if (serviceAttr is null)
                    throw new InvalidOperationException("Something went wrong while filtering types for registration");

                var interceptAttr = type.GetCustomAttribute<InterceptedAttribute>(false);
                var ctorAttr = type.GetCustomAttribute<FindConstructorsWithAttribute>(false);
                var decAttrs = type.GetCustomAttributes<DecoratedAttribute>(false).ToList();

                if (ctorAttr is not null && interceptAttr is not null)
                    throw new InvalidOperationException(
                        "Using a custom constructor finder will prevent interception from happening");

                var scope = serviceAttr.ServiceLifetime ?? config.DefaultServiceLifetime;

                var registerAsTypes = serviceAttr.ServiceTypes?
                    .Distinct()
                    .ToList() ?? new List<Type>();
                
                var shouldAsSelf = serviceAttr.RegistrationStrategy is RegistrationStrategy.AsSelf &&
                                   registerAsTypes.All(x => x != type);
                
                var shouldAsInterfaces = !registerAsTypes.Any() ||
                                         serviceAttr.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces;
                
                if (!shouldAsInterfaces && !registerAsTypes.Any())
                    shouldAsSelf = true;

                IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>?
                    registrationGenericBuilder = null;
                IRegistrationBuilder<object, ReflectionActivatorData, SingleRegistrationStyle>?
                    registrationBuilder = null;

                if (type.IsGenericType && type.IsGenericTypeDefinition)
                {
                    if (interceptAttr is not null)
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
                    if (interceptAttr is not null)
                    {
                        registrationBuilder = interceptAttr.InterceptionStrategy switch
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
                            _ => throw new ArgumentOutOfRangeException(nameof(interceptAttr.InterceptionStrategy))
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
                            registrationBuilder?.InstancePerMatchingLifetimeScope(serviceAttr?.Tags?.ToArray() ??
                                Array.Empty<object>());
                        registrationGenericBuilder =
                            registrationGenericBuilder?.InstancePerMatchingLifetimeScope(
                                serviceAttr?.Tags?.ToArray() ?? Array.Empty<object>());
                        break;
                    case ServiceLifetime.InstancePerOwned:
                        if (serviceAttr?.OwnedByType is null) throw new InvalidOperationException("Owned type was null");

                        registrationBuilder = registrationBuilder?.InstancePerOwned(serviceAttr.OwnedByType);
                        registrationGenericBuilder = registrationGenericBuilder?.InstancePerOwned(serviceAttr.OwnedByType);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(scope));
                }

                if (interceptAttr is not null)
                    foreach (var interceptor in interceptAttr.Interceptors)
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
                    type.GetInterfaces().Where(x => x.IsDirectAncestor(type)).ToList().ForEach(x => serviceTypes.Add(x));

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
        => AddAttributeDefinedServices(serviceCollection, assembliesContainingTypesToScan.Select(x => x.Assembly).Distinct(), options);

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
                var serviceAttr = type.GetCustomAttribute<ServiceImplementationAttribute>(false);
                if (serviceAttr is null)
                    throw new InvalidOperationException("Something went wrong while filtering types for registration");
                
                var scope = serviceAttr.ServiceLifetime ?? config.DefaultServiceLifetime;
                
                var registerAsTypes = serviceAttr.ServiceTypes?
                    .Distinct()
                    .ToList() ?? new List<Type>();
                
                var shouldAsSelf = serviceAttr.RegistrationStrategy is RegistrationStrategy.AsSelf &&
                                   registerAsTypes.All(x => x != type);
                
                var shouldAsInterfaces = !registerAsTypes.Any() ||
                                         serviceAttr.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces;

                if (!shouldAsInterfaces && !registerAsTypes.Any())
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
                        break;
                    case ServiceLifetime.InstancePerRequest:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.AddScoped(x, type));
                        if (shouldAsSelf)
                            serviceCollection.AddScoped(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.AddScoped(x, type));
                        break;
                    case ServiceLifetime.InstancePerLifetimeScope:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.AddScoped(x, type));
                        if (shouldAsSelf)
                            serviceCollection.AddScoped(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.AddScoped(x, type));
                        break;
                    case ServiceLifetime.InstancePerDependency:
                        if (shouldAsInterfaces)
                            interfaces.ForEach(x => serviceCollection.AddTransient(x, type));
                        if (shouldAsSelf)
                            serviceCollection.AddTransient(type);
                        if (registerAsTypes.Any())
                            registerAsTypes.ForEach(x => serviceCollection.AddTransient(x, type));
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
