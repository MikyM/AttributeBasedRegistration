namespace AttributeBasedRegistration.Attributes;

/// <summary>
/// Instructs the registering logic to skip this interceptor from being automatically registered.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SkipInterceptorRegistrationAttribute : Attribute
{
}
