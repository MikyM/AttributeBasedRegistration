# AttributeBasedRegistration

[![Build Status](https://github.com/MikyM/AttributeBasedRegistration/actions/workflows/release.yml/badge.svg)](https://github.com/MikyM/AttributeBasedRegistration/actions)

Library allowing registering services with Autofac and Microsoft's DI container via attributes. Additionally contains various extensions to Autofac and Microsoft's DI container.

## Features

Set of attributes allowing automatic registration:

- [ServiceImplementation] - marks the class as an implementation of a service
- [RegisterAs] - defines service types or a registration strategy
- [Lifetime] - defines the lifetime with which the service should be registered (Autofac's based - Microsoft's DI equivalent will be used) - some supported only with Autofac
- [EnableInterception] - enables interception on the service, in theory supports intercepting classes and interfaces (or both) (Intercept enum) though intercepting classes sometimes suffers from weird Castle.Core bugs thus when using interception it is encouraged to use interface service types - supported only with Autofac
- [InterceptedBy] - defines interceptor types that should intercept calls to the service - supported only with Autofac
- [FindConstructorsWith] - defines a constructor finder to use during creation of the service instance, supports only parameterless ctors and can't be used in conjunction with interceptors - supported only with Autofac
- [DecoratedBy] - defines types that decorate this service - supported only with Autofac

The usage of the `Lifetime` attribute versus supplying the lifetime to the `ServiceImplementation` is per users taste, the data from the more specific attribute will take precedence.

Similar to the above, using multiple `RegisterAs` or `InterceptedBy` attributes versus one and supplying multiple types is per users taste - types will be aggregated from all attributes.

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

[Service]
[RegisterAs(typeof(ICustomService))]
[Lifetime(Lifetime.InstancePerLifetimeScope)]
[DecoratedBy(typeof(ISomeDecorator), 1)]
[EnableInterception(Intercept.Interface)]
[InterceptedBy(typeof(ISomeInterceptor))]
public class CustomService : ICustomService
{

}

```
