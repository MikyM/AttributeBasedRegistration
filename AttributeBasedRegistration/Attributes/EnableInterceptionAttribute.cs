namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Defines whether to enable interception for this registration.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class EnableInterceptionAttribute : Attribute
{
    /// <summary>
    /// Type of interception.
    /// </summary>
    public Intercept Intercept { get; private set; }

    /// <summary>
    /// Defines whether to enable interception for this registration.
    /// </summary>
    public EnableInterceptionAttribute(Intercept intercept)
    {
        Intercept = intercept;
    }
}
