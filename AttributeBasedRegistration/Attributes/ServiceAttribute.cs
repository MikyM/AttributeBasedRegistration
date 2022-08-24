namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Marks a class for registration as a service implementation.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceAttribute : Attribute
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public ServiceAttribute()
    {
    }
}
