using Autofac.Core.Activators.Reflection;

namespace MikyM.Autofac.Extensions.Attributes;

[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public class FindConstructorsWithAttribute : Attribute
{
    public Type ConstructorFinder { get; set; }
    public FindConstructorsWithAttribute(Type constructorFinder)
    {
        if (constructorFinder is null) throw new ArgumentNullException(nameof(constructorFinder));
        if (!constructorFinder.IsAssignableTo(typeof(IConstructorFinder)))
            throw new InvalidOperationException("Invalid constructor finder type");
        ConstructorFinder = constructorFinder;
    }
}
