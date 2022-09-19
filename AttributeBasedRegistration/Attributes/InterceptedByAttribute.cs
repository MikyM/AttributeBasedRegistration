using Castle.DynamicProxy;

namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines with what interceptors should the service be intercepted.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class InterceptedByAttribute : Attribute
{
    /// <summary>
    /// Interceptor's type.
    /// </summary>
    public Type[] Interceptors { get; private set; }

    /// <summary>
    /// Defines with what interceptors should the service be intercepted.
    /// </summary>
    public InterceptedByAttribute(params Type[] interceptors)
    {
        if (interceptors.Length == 0)
            throw new InvalidOperationException("You must pass at least one interceptor");

        foreach (var i in interceptors)
        {
            if (i.GetInterfaces().All(x => x != typeof(IAsyncInterceptor) && x != typeof(IInterceptor)))
                throw new InvalidOperationException($"{i.Name} does not implement any interceptor interface");
        }

        Interceptors = interceptors ?? throw new ArgumentNullException(nameof(interceptors));
    }
}
