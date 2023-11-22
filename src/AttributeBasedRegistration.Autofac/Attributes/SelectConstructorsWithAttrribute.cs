using AttributeBasedRegistration.Attributes.Abstractions;
using Autofac.Core.Activators.Reflection;

namespace AttributeBasedRegistration.Autofac.Attributes;

/// <summary>
/// Marks a registration to use a specific constructor selector.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SelectConstructorsWithAttribute : Attribute, ISelectConstructorsWithAttribute
{
    /// <summary>
    /// Type of the ctor selector.
    /// </summary>
    public Type ConstructorSelector { get; private set; }
    
    /// <summary>
    /// Creates a new instance of the ctor selector attribute.
    /// </summary>
    /// <param name="constructorSelector">The ctor selector.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public SelectConstructorsWithAttribute(Type constructorSelector)
    {
        if (constructorSelector is null) throw new ArgumentNullException(nameof(constructorSelector));
        if (!constructorSelector.IsAssignableTo(typeof(IConstructorSelector)))
            throw new InvalidOperationException("Selector must implement IConstructorSelector");
        ConstructorSelector = constructorSelector;
    }
}

#if NET7_0_OR_GREATER
/// <summary>
/// Marks a registration to use a specific constructor selector.
/// </summary>
/// <typeparam name="TCtorSelecter">Type of the ctor selector.</typeparam>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SelectConstructorsWithAttribute<TCtorSelecter> : Attribute, ISelectConstructorsWithAttribute where TCtorSelecter : class
{
    /// <summary>
    /// Type of the ctor selector.
    /// </summary>
    public Type ConstructorSelector { get; set; }
    
    /// <summary>
    /// Creates a new instance of the ctor selector attribute.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public SelectConstructorsWithAttribute()
    {
        var constructorSelector = typeof(TCtorSelecter);
        if (constructorSelector is null) 
            throw new ArgumentNullException(nameof(constructorSelector));
        if (!constructorSelector.IsAssignableTo(typeof(IConstructorSelector)))
            throw new InvalidOperationException("Selector must implement IConstructorSelector");
        ConstructorSelector = constructorSelector;
    }
}
#endif
