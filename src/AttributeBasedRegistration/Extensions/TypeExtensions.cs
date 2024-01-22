using AttributeBasedRegistration.Attributes.Abstractions;

namespace AttributeBasedRegistration.Extensions;

/// <summary>
/// Type extensions.
/// </summary>
[PublicAPI]
public static class TypeExtensions
{
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
    /// Gets all attributes that can be cast to the given type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <typeparam name="TAttribute">Searched attribute type.</typeparam>
    /// <returns>A collection of attributes.</returns>
    public static IEnumerable<TAttribute> GetAttributesOfType<TAttribute>(this Type type) where TAttribute : class
        => type.GetCustomAttributes(false).Where(y => y is TAttribute).Cast<TAttribute>();
    
    /// <summary>
    /// Gets the first found attribute that can be cast to the given type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <typeparam name="TAttribute">Searched attribute type.</typeparam>
    /// <returns>Found attribute or null.</returns>
    public static TAttribute? GetAttributeOfType<TAttribute>(this Type type) where TAttribute : class
        => (TAttribute?)type.GetCustomAttributes(false).FirstOrDefault(y => y is TAttribute);
    
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
        => (TAttribute?)type.GetCustomAttributes(false).FirstOrDefault(y => y is TAttribute);
    
        /// <summary>
    /// Extended get interfaces method.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="includeInherited"></param>
    /// <param name="fullSet"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited, bool fullSet = true)
    {
        if ((includeInherited || type.BaseType is null) && fullSet)
            return type.GetInterfaces();

        var allInterfaces = type.GetInterfaces();

        switch (includeInherited)
        {
            case false when !fullSet:
                return type.GetInterfaces().Except(allInterfaces.SelectMany(x => x.GetInterfaces()));
            case true when type.BaseType is not null && !fullSet:
            {
                var res = type.GetInterfaces().Except(allInterfaces.SelectMany(x => x.GetInterfaces())).ToList();
                res.AddRange(type.BaseType.GetInterfaces());

                return res;
            }
            default:
                return type.GetInterfaces().Except(allInterfaces.SelectMany(x => x.GetInterfaces())).ToList();
        }
    }
    
    /// <summary>
    /// Gets the types name while discarding anything that comes after "`" for generic types.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Types name.</returns>
    public static string GetName(this Type type)
    {
        return !type.IsGenericType ? type.Name : type.Name.Split('`').First();
    }

    /// <summary>
    /// Determines whether the given type is a closed <see cref="Nullable{TValue}"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>true if the type is a closed Nullable; otherwise, false.</returns>
    public static bool IsNullable(this Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        return type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Gets a dictionary with interface implementation pairs that implement a given base interface.
    /// </summary>
    /// <param name="interfaceToSearchFor">Base interface to search for.</param>
    public static Dictionary<Type, Type?> GetInterfaceImplementationPairs(this Type interfaceToSearchFor)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var dict = assemblies
            .SelectMany(x => x.GetTypes()
                .Where(t => interfaceToSearchFor.IsDirectAncestor(t) &&
                            t.IsInterface))
            .ToDictionary(intr => intr,
                intr => assemblies.SelectMany(impl => impl.GetTypes())
                    .FirstOrDefault(impl =>
                        impl.IsAssignableToWithGenerics(intr) && impl.IsClass &&
                        intr.IsDirectAncestor(impl)));

        return dict;
    }

    /// <summary>
    /// Checks whether the given type is assignable to another type supporting generic types.
    /// </summary>
    /// <param name="givenType">Type to check.</param>
    /// <param name="genericType">Type to compare with.</param>
    /// <returns>True if the given type is assignable to another type, otherwise false.</returns>
    public static bool IsAssignableToWithGenerics(this Type givenType, Type genericType)
    {
        if (!genericType.IsGenericType)
            return givenType.IsAssignableTo(genericType);

        var interfaceTypes = givenType.GetInterfaces();

        if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            return true;
        
        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        Type? baseType = givenType.BaseType;
        if (baseType == null) return false;

        return IsAssignableToWithGenerics(baseType, genericType);
    }

    /// <summary>
    /// Retrieves the type inheritance tree
    /// </summary>
    /// <param name="type">The type to find tree for.</param>
    /// <returns>The inheritance tree.</returns>
    public static InheritanceTree GetTypeInheritance(this Type type)
    {
        //get all the interfaces for this type
        var interfaces = type.GetInterfaces();

        //get all the interfaces for the ancestor interfaces
        var baseInterfaces = interfaces.SelectMany(i => i.GetInterfaces());

        //filter based on only the direct interfaces
        var directInterfaces = interfaces.Where(i => baseInterfaces.All(b => b != i));

        return new InheritanceTree(type, directInterfaces.Select(GetTypeInheritance).ToList());
    }

    /// <summary>
    /// Check if a type is a direct ancestor of given type
    /// </summary>
    public static bool IsDirectAncestor(this Type ancestorCandidate, Type type)
        => type.GetTypeInheritance().IsDirectAncestor(ancestorCandidate);

    /// <summary>
    /// Gets the direct ancestors of a given type.
    /// </summary>
    public static IEnumerable<Type> GetDirectAncestors(this Type type, bool onlyInterfaces = false)
        => onlyInterfaces
            ? type.GetTypeInheritance().Ancestors.Select(x => x.Node).Where(x => x.IsInterface)
            : type.GetTypeInheritance().Ancestors.Select(x => x.Node);

    /// <summary>
    /// Gets interface by naming convention.
    /// </summary>
    public static Type? GetInterfaceByNamingConvention(this Type type)
        => type.GetInterface($"I{type.Name}");
    
    /// <summary>
    /// Gets the direct ancestors of a given type.
    /// </summary>
    public static IEnumerable<Type> GetDirectInterfaceAncestors(this Type type)
        => GetDirectAncestors(type, true);
    
    /// <summary>
    /// Gets the direct ancestors of a given type.
    /// </summary>
    public static IEnumerable<Type> GetDirectClassAncestors(this Type type, bool skipAbstract = false)
        => skipAbstract
            ? type.GetTypeInheritance().Ancestors.Select(x => x.Node).Where(x => x.IsClass && !x.IsAbstract)
            : type.GetTypeInheritance().Ancestors.Select(x => x.Node).Where(x => x.IsClass);
}
