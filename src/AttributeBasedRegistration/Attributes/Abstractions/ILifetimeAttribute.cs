namespace AttributeBasedRegistration.Attributes.Abstractions;

/// <summary>
/// Represents a Lifetime attribute.
/// </summary>\
[PublicAPI]
public interface ILifetimeAttribute : IRegistrationAttribute
{
    /// <summary>
    /// Lifetime to use for the registration.
    /// </summary>
    ServiceLifetime ServiceLifetime { get; }

    /// <summary>
    /// Type to use for owned registrations.
    /// </summary>
    Type? Owned { get; }

    /// <summary>
    /// Tags to use for tagged registrations.
    /// </summary>
    IEnumerable<object>? Tags { get; }
}
