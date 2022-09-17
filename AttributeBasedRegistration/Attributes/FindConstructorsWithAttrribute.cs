using Autofac.Core.Activators.Reflection;

namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// 
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class FindConstructorsWithAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public Type ConstructorFinder { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="constructorFinder"></param>
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
