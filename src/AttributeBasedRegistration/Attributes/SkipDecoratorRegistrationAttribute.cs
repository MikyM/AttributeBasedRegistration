using AttributeBasedRegistration.Attributes.Abstractions;

namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Instructs the registering logic to skip this decorator from being automatically registered.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SkipDecoratorRegistrationAttribute : Attribute, ISkipDecoratorRegistrationAttribute
{
}
