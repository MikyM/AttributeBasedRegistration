using AttributeBasedRegistration.Attributes.Abstractions;
using Autofac.Core.Activators.Reflection;

namespace AttributeBasedRegistration.Autofac.Attributes;

/// <summary>
/// Marks a registration to use a specific constructor finder.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FindConstructorsWithAttribute : Attribute, IFindConstructorsWithAttribute
{
    /// <summary>
    /// Type of the ctor finder.
    /// </summary>
    public Type ConstructorFinder { get; private set; }
    
    /// <summary>
    /// Creates a new instance of the ctor finder attribute.
    /// </summary>
    /// <param name="constructorFinder">The ctor finder.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public FindConstructorsWithAttribute(Type constructorFinder)
    {
        if (constructorFinder is null) throw new ArgumentNullException(nameof(constructorFinder));
        if (!constructorFinder.IsAssignableTo(typeof(IConstructorFinder)))
            throw new InvalidOperationException("Finder must implement IConstructorFinder");
        ConstructorFinder = constructorFinder;
    }
}

#if NET7_0
/// <summary>
/// Marks a registration to use a specific constructor finder.
/// </summary>
/// <typeparam name="TCtorFinder">Type of the ctor finder.</typeparam>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FindConstructorsWithAttribute<TCtorFinder> : Attribute, IFindConstructorsWithAttribute where TCtorFinder : class
{
    /// <summary>
    /// Type of the ctor finder.
    /// </summary>
    public Type ConstructorFinder { get; set; }
    
    /// <summary>
    /// Creates a new instance of the ctor finder attribute.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public FindConstructorsWithAttribute()
    {
        var constructorFinder = typeof(TCtorFinder);
        if (constructorFinder is null) 
            throw new ArgumentNullException(nameof(constructorFinder));
        if (!constructorFinder.IsAssignableTo(typeof(IConstructorFinder)))
            throw new InvalidOperationException("Finder must implement IConstructorFinder");
        ConstructorFinder = constructorFinder;
    }
}
#endif
