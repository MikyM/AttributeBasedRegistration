namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines whether to enable interception for this registration.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EnableInterceptionAttribute : Attribute
{
    /// <summary>
    /// Type of interception.
    /// </summary>
    public InterceptionStrategy InterceptionStrategy { get; private set; }

    /// <summary>
    /// Defines whether to enable interception for this registration.
    /// </summary>
    public EnableInterceptionAttribute(InterceptionStrategy interceptionStrategy)
    {
        InterceptionStrategy = interceptionStrategy;
    }
}
