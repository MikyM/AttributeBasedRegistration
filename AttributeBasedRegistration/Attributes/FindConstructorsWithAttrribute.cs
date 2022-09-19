using Autofac.Core.Activators.Reflection;

namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Marks a registration to use a specific constructor finder.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FindConstructorsWithAttribute : Attribute
{
    /// <summary>
    /// Type of the ctor finder.
    /// </summary>
    public Type ConstructorFinder { get; set; }
    
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
            throw new InvalidOperationException("Invalid constructor finder type");
        ConstructorFinder = constructorFinder;
    }
}
