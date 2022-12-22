﻿using AttributeBasedRegistration.Attributes.Abstractions;
using Castle.DynamicProxy;
using MikyM.Utilities.Extensions;

namespace AttributeBasedRegistration.Extensions;

/// <summary>
/// Type extensions.
/// </summary>
[PublicAPI]
public static class TypeExtensions
{
    /// <summary>
    /// Whether given interceptor is an async interceptor.
    /// </summary>
    public static bool IsAsyncInterceptor(this Type interceptorCandidate) => interceptorCandidate.GetInterfaces().Any(x => x == typeof(IAsyncInterceptor));
    
    /// <summary>
    /// Checks whether the current type is a service implementation (has a <see cref="IServiceImplementationAttribute"/> attribute).
    /// </summary>
    /// <param name="type">Candidate.</param>
    /// <returns>True if type is a service implementation, otherwise false.</returns>
    public static bool IsServiceImplementation(this Type type)
        => type.GetCustomAttributes(false).Any(y => y is IServiceImplementationAttribute) &&
           type.IsClass && !type.IsAbstract;

    /// <summary>
    /// Checks whether the automatic registration process should be skipped by checking if there is a <see cref="ISkipRegistrationAttribute"/> defined on type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns>True if automatic registration should be skipped, otherwise false.</returns>
    public static bool ShouldSkipRegistration(this Type type)
        => type.GetCustomAttributes(false).Any(y => y is ISkipRegistrationAttribute);
        
    /// <summary>
    /// Checks whether the automatic registration process should be skipped by checking if there is an attribute of a given type defined on the target type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <typeparam name="TSkipAttribute">Type of attribute to look for.</typeparam>
    /// <returns>True if automatic registration should be skipped, otherwise false.</returns>
    public static bool ShouldSkipRegistration<TSkipAttribute>(this Type type) where TSkipAttribute : ISkipRegistrationAttribute
        => type.GetCustomAttributes(false).Any(y => y is TSkipAttribute);

    /// <summary>
    /// Gets all registration attributes that can be cast to the given type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <typeparam name="TAttribute">Searched attribute type.</typeparam>
    /// <returns>A collection of attributes.</returns>
    public static IEnumerable<TAttribute> GetRegistrationAttributesOfType<TAttribute>(this Type type) where TAttribute : class, IRegistrationAttribute
        => type.GetCustomAttributes(false).Where(y => y is TAttribute).Cast<TAttribute>();
    
    /// <summary>
    /// Gets the first found attribute that can be cast to the given type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <typeparam name="TAttribute">Searched attribute type.</typeparam>
    /// <returns>Found attribute or null.</returns>
    public static TAttribute? GetRegistrationAttributeOfType<TAttribute>(this Type type) where TAttribute : class, IRegistrationAttribute
        => type.GetCustomAttributes(false).FirstOrDefault(y => y is TAttribute)?.CastTo<TAttribute?>();

    /// <summary>
    /// Checks whether the current type is an interceptor implementation (implements either <see cref="IInterceptor"/> or <see cref="IAsyncInterceptor"/>).
    /// </summary>
    /// <param name="type">Candidate.</param>
    /// <returns>True if type is an interceptor implementation, otherwise false.</returns>
    public static bool IsInterceptorImplementation(this Type type)
        => type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(x => x == typeof(IInterceptor) || x == typeof(IAsyncInterceptor));
}
