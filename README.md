# AttributeBasedRegistration

[![Build Status](https://github.com/MikyM/AttributeBasedRegistration/actions/workflows/release.yml/badge.svg)](https://github.com/MikyM/AttributeBasedRegistration/actions)

Library allowing registering services with Autofac and Microsoft's DI container via attributes. Additionally contains various extensions to Autofac and Microsoft's DI container.

## Features

Set of attributes allowing automatic registration:

- [ServiceImplementation] - marks the class as an implementation of a service, defines the service type(s) or a registration strategy and the lifetime of the service
- [Intercepted] - marks the service implementation for interception, defines the interceptor types and the interception strategy - supported only with Autofac
- [FindConstructorsWith] - defines a constructor finder to use during creation of the service instance, supports only parameterless ctors and can't be used in conjunction with interceptors - supported only with Autofac
- [Decorated] - marks the service implementation for decoration with a specified decorator and registration order - supported only with Autofac

## Installation

To pick up and register services via attributes use the extensions method on `ContainerBuilder` or `IServiceCollection` provided by the library:

```csharp
builder.AddAttributeDefinedServices(assembliesToScan);
```

## Example usage

```csharp

public interface ICustomService
{

}

[ServiceImplementation(ServiceLifetime.InstancePerLifetimeScope, typeof(ICustomService))]
[Decorated(1, typeof(ISomeDecorator))]
[Intercepted(InterceptionStrategy.Interface, typeof(ISomeInterceptor))]
public class CustomService : ICustomService
{

}

[ServiceImplementation(ServiceLifetime.InstancePerDependency, RegistrationStrategy.AsSelf)]
[Decorated(1, typeof(ISomeDecorator))]
[Intercepted(InterceptionStrategy.Interface, typeof(ISomeInterceptor))]
public class AnotherCustomService
{

}

```
