namespace AttributeBasedRegistration;

/// <summary>
/// Defines a lifetime for a particular service if desired to be different than the default.
/// </summary>
/// <remarks>This only has effect in external libraries that implement support for this attribute outside of the scope of this library.</remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[PublicAPI]
public sealed class LifetimeAttribute : Attribute
{
    /// <summary>
    /// Lifetime.
    /// </summary>
    public ServiceLifetime Lifetime { get; private set; }

    /// <summary>
    /// Creates a new instance of the lifetime attribute.
    /// </summary>
    /// <param name="lifetime">The desired lifetime.</param>
    public LifetimeAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}
