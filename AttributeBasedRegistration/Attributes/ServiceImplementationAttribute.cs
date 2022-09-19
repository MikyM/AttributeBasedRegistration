namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Marks a class for registration as a service implementation.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ServiceImplementationAttribute : Attribute
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public ServiceImplementationAttribute()
    {
    }
}
