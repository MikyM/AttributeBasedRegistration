using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace AttributeBasedRegistration.Autofac;

/// <summary>
/// Constructor finder that finds all constructors.
/// </summary>
[PublicAPI]
public sealed class AllConstructorsFinder : IConstructorFinder
{
    private static readonly ConcurrentDictionary<Type, ConstructorInfo[]> Cache = new();


    /// <inheritdoc />
    public ConstructorInfo[] FindConstructors(Type targetType)
    {
        var result = Cache.GetOrAdd(targetType,
            t => t.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic).ToArray());

        return result.Length > 0 ? result : throw new NoConstructorsFoundException(targetType, this);
    }
}
