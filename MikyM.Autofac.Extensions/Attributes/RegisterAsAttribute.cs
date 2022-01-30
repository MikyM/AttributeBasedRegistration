namespace MikyM.Autofac.Extensions.Attributes;

/// <summary>
/// Defines as what should the service be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RegisterAsAttribute : Attribute
{
    public Type? RegisterAsType { get; private set; }

    public RegisterAs? RegisterAsOption { get; private set; }

    public RegisterAsAttribute(Type registerAs)
    {
        RegisterAsType = registerAs ?? throw new ArgumentNullException(nameof(registerAs));
    }

    public RegisterAsAttribute(RegisterAs registerAs)
    {
        RegisterAsOption = registerAs;
    }
}