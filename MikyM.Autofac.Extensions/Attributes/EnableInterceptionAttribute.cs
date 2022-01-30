namespace MikyM.Autofac.Extensions.Attributes;

/// <summary>
/// Defines whether to enable interception for this registration
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class EnableInterceptionAttribute : Attribute
{
    public Intercept Intercept { get; private set; }
    public EnableInterceptionAttribute(Intercept intercept)
    {
        Intercept = intercept;
    }
}
