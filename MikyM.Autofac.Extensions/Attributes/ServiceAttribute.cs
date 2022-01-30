namespace MikyM.Autofac.Extensions.Attributes;

/// <summary>
/// Marks a class for registration as a service
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceAttribute : Attribute
{
    public ServiceAttribute()
    {
    }
}