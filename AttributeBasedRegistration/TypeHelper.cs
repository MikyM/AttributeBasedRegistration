using AttributeBasedRegistration.Attributes.Abstractions;

namespace AttributeBasedRegistration;

internal static class TypeHelper
{
       internal static List<Type> GetServiceTypes(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes)
        => asAttributes.Where(x => x.ServiceTypes is not null)
            .SelectMany(x => x.ServiceTypes ?? Type.EmptyTypes)
            .Concat(implementationAttribute?.ServiceTypes ?? Type.EmptyTypes)
            .Distinct()
            .ToList();

    internal static bool ShouldAsInterfaces(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes)
        => asAttributes.Any(x =>
               x.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces) ||
           implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsImplementedInterfaces;
    
    internal static bool ShouldAsSelf(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes, IEnumerable<Type> serviceTypes)
        => (asAttributes.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsSelf) ||
            implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsSelf) &&
           serviceTypes.All(y => y != type);
    
    internal static bool ShouldAsDirectAncestors(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes)
        => asAttributes.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces) ||
           implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsDirectAncestorInterfaces;
    
    internal static bool ShouldUsingNamingConvention(this Type type, IServiceImplementationAttribute implementationAttribute, IEnumerable<IRegisterAsAttribute> asAttributes)
        => asAttributes.Any(x => x.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface) ||
           implementationAttribute?.RegistrationStrategy is RegistrationStrategy.AsConventionNamedInterface;

}
