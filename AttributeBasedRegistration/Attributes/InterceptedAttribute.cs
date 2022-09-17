namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Marks the service implementation for iterception with specified interceptors and interception strategy.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class InterceptedAttribute : Attribute
{
    /// <summary>
    /// Interceptor's types.
    /// </summary>
    public Type[] Interceptors { get; private set; }

    /// <summary>
    /// Type of interception.
    /// </summary>
    public InterceptionStrategy InterceptionStrategy { get; private set; }
    

    /// <summary>
    /// Defines with what interceptors should the service be intercepted.
    /// </summary>
    public InterceptedAttribute(InterceptionStrategy interceptionStrategy, params Type[] interceptors)
    {
        Interceptors = interceptors ?? throw new ArgumentNullException(nameof(interceptors));

        InterceptionStrategy = interceptionStrategy;
    }
}
