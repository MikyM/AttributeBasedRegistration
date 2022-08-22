# MikyM.Autofac.Extensions

[![Build Status](https://github.com/MikyM/MikyM.Autofac.Extensions/actions/workflows/release.yml/badge.svg)](https://github.com/MikyM/MikyM.Autofac.Extensions/actions)

Library containing various extensions to Autofac but mainly ability to register services using attributes.

## Features

Set of attributes allowing automatic registration:

- [Service] - marks the class as an implementation of a service
- [RegisterAs] - defines the service type to register, supports bare-bone types and Autofac's `AsSelf()` and `AsImplementedInterfaces()`
- [Lifetime] - defines the lifetime with which the service should be registered
- [EnableInterception] - enables interception on the service, in theory supports intercepting classes and interfaces (or both) (Intercept enum) though intercepting classes sometimes suffers from weird Castle.Core bugs thus when using interception it is encouraged to use interface service types
- [InterceptedBy] - defines types that should intercept calls to the service
- [FindConstructorsWith] - defines a constructor finder to use during creation of the service instance, supports only parameterless ctors and can't be used in conjunction with interceptors
- [DecoratedBy] - defines types that decorate this service

## Installation

Since the library utilizes Autofac, base Autofac configuration is required - [Autofac's docs](https://autofac.readthedocs.io/en/latest/index.html).

To pick up and register services via attributes use the extensions method on ContainerBuilder provided by the library:

```csharp
builder.AddAttributeDefinedServices();
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
