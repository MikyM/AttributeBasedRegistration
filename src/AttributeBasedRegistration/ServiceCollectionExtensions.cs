using System.Reflection;
using AttributeBasedRegistration.Attributes.Abstractions;
using AttributeBasedRegistration.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AttributeBasedRegistration;

/// <summary>
/// Extensions.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="assembliesContainingTypesToScan">Assemblies containing types to scan for services.</param>
    /// <param name="options">Optional configuration.</param>
    /// <returns>Current <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection,
        IEnumerable<Type> assembliesContainingTypesToScan, Action<AttributeRegistrationOptions>? options = null)
        => AddAttributeDefinedServices(serviceCollection, assembliesContainingTypesToScan.Select(x => x.Assembly),
            options);
    
    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="assembliesContainingTypesToScan">Assemblies containing types to scan for services.</param>
    /// <returns>Current <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, params Type[] assembliesContainingTypesToScan)
        => AddAttributeDefinedServices(serviceCollection, assembliesContainingTypesToScan, null);
    
    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="options">Configuration.</param>
    /// <param name="assembliesContainingTypesToScan">Assemblies containing types to scan for services.</param>
    /// <returns>Current <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, Action<AttributeRegistrationOptions> options, params Type[] assembliesContainingTypesToScan)
        => AddAttributeDefinedServices(serviceCollection, assembliesContainingTypesToScan, options);
    
    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="options">Configuration.</param>
    /// <param name="assembliesToScan">Assemblies to scan for services.</param>
    /// <returns>Current <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, Action<AttributeRegistrationOptions> options, params Assembly[] assembliesToScan)
        => AddAttributeDefinedServices(serviceCollection, assembliesToScan, options);
    
    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="assembliesToScan">Assemblies to scan for services.</param>
    /// <returns>Current <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddAttributeDefinedServices(this IServiceCollection serviceCollection, params Assembly[] assembliesToScan)
        => AddAttributeDefinedServices(serviceCollection, assembliesToScan, null);

    /// <summary>
    /// Registers services with <see cref="IServiceCollection"/> via attributes.
    /// </summary>
    /// <param name="serviceCollection">Current service collection instance.</param>
    /// <param name="assembliesToScan">Assemblies to scan for services.</param>
    /// <param name="options">Optional configuration.</param>
    /// <returns>Current <see cref="IServiceCollection"/> instance.</returns>
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
    /// Adds the identifier services of the root DI scope making <see cref="ServiceCollectionExtensions.IsRootScope(IServiceProvider)"/> available.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    public static IServiceCollection AddRootScopeIdentifier(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<RootScopeWrapper>();
        serviceCollection.AddHostedService<RootScopeWrapperStarter>();

        return serviceCollection;
    }
    
    /// <summary>
    /// Checks whether the given scope is a root scope.
    /// </summary>
    /// <param name="serviceProvider">Root scope candidate.</param>
    /// <returns>True if the given scope is the root scope, otherwise false.</returns>
    public static bool IsRootScope(this IServiceProvider serviceProvider)
    {
        var trueRoot = serviceProvider.GetService<RootScopeWrapper>()?.ServiceProvider;
        if (trueRoot is null)
            throw new InvalidOperationException(
                "You must register root scope identifiers with AddRootScopeIdentifier extension method to be able to use IsRootScope method");

        return serviceProvider == trueRoot;
    }
}
