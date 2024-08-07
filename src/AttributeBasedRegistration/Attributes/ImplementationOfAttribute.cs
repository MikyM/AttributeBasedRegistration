﻿// using AttributeBasedRegistration.Attributes.Abstractions;
//
// namespace AttributeBasedRegistration.Attributes;
//
// /// <summary>
// /// Marks a class for registration as a service implementation.
// /// </summary>
// [PublicAPI]
// [AttributeUsage(AttributeTargets.Class, Inherited = false)]
// public sealed class ImplementationOfAttribute : Attribute, IImplementationOfAttribute
// {
//     /// <summary>
//     /// Lifetime to use for the registration if specified.
//     /// </summary>
//     public ServiceLifetime? ServiceLifetime { get; private set; }
//     
//     /// <summary>
//     /// Services types if specified.
//     /// </summary>
//     public Type[]? ServiceTypes { get; private set; }
//     
//     /// <summary>
//     /// Registration strategy if specified.
//     /// </summary>
//     public RegistrationStrategy? RegistrationStrategy { get; private set; }
//     
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     public ImplementationOfAttribute()
//     {
//     }
//
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="serviceLifetime">Lifetime for this registration.</param>
//     public ImplementationOfAttribute(ServiceLifetime serviceLifetime)
//     {
//         ServiceLifetime = serviceLifetime;
//     }
//
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="serviceLifetime">Lifetime for this registration.</param>
//     /// <param name="registrationStrategy">Registration strategy.</param>
//     public ImplementationOfAttribute(ServiceLifetime serviceLifetime, RegistrationStrategy registrationStrategy)
//     {
//         ServiceLifetime = serviceLifetime;
//         RegistrationStrategy = registrationStrategy;
//     }
//     
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="registrationStrategy">Registration strategy.</param>
//     public ImplementationOfAttribute(RegistrationStrategy registrationStrategy)
//     {
//         RegistrationStrategy = registrationStrategy;
//     }
//     
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="serviceTypes">Service types.</param>
//     public ImplementationOfAttribute(params Type[] serviceTypes)
//     {
//         ServiceTypes = serviceTypes;
//     }
//     
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="serviceLifetime">Lifetime for this registration.</param>
//     /// <param name="serviceTypes">Service types.</param>
//     public ImplementationOfAttribute(ServiceLifetime serviceLifetime, params Type[] serviceTypes)
//     {
//         ServiceTypes = serviceTypes;
//         ServiceLifetime = serviceLifetime;
//     }
// }
//
// #if NET7_0_OR_GREATER
// /// <summary>
// /// Marks a class for registration as a service implementation.
// /// </summary>
// /// <typeparam name="TService">Service type.</typeparam>
// /// <remarks> A newer equivalent of <see cref="ServiceImplementationAttribute"/>.</remarks>
// [PublicAPI]
// [AttributeUsage(AttributeTargets.Class, Inherited = false)]
// public sealed class ImplementationOfAttribute<TService> : Attribute, IImplementationOfAttribute where TService : class
// {
//     /// <summary>
//     /// Lifetime to use for the registration if specified.
//     /// </summary>
//     public ServiceLifetime? ServiceLifetime { get; private set; }
//     
//     /// <summary>
//     /// Services types if specified.
//     /// </summary>
//     public Type[]? ServiceTypes { get; private set; }
//     
//     /// <summary>
//     /// Registration strategy if specified.
//     /// </summary>
//     public RegistrationStrategy? RegistrationStrategy { get; private set; }
//     
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     public ImplementationOfAttribute()
//     {
//         ServiceTypes = new[] { typeof(TService) };
//     }
//
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="serviceLifetime">Lifetime for this registration.</param>
//     public ImplementationOfAttribute(ServiceLifetime serviceLifetime)
//     {
//         ServiceTypes = new[] { typeof(TService) };
//         ServiceLifetime = serviceLifetime;
//     }
// }
//
// /// <summary>
// /// Marks a class for registration as a service implementation.
// /// </summary>
// /// <typeparam name="TService1">Service type 1.</typeparam>
// /// <typeparam name="TService2">Service type 2.</typeparam>
// /// <remarks> A newer equivalent of <see cref="ServiceImplementationAttribute"/>.</remarks>
// [PublicAPI]
// [AttributeUsage(AttributeTargets.Class, Inherited = false)]
// public sealed class ImplementationOfAttribute<TService1, TService2> : Attribute, IImplementationOfAttribute where TService1 : class  where TService2 : class
// {
//     /// <summary>
//     /// Lifetime to use for the registration if specified.
//     /// </summary>
//     public ServiceLifetime? ServiceLifetime { get; private set; }
//     
//     /// <summary>
//     /// Services types if specified.
//     /// </summary>
//     public Type[]? ServiceTypes { get; private set; }
//     
//     /// <summary>
//     /// Registration strategy if specified.
//     /// </summary>
//     public RegistrationStrategy? RegistrationStrategy { get; private set; }
//     
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     public ImplementationOfAttribute()
//     {
//         ServiceTypes = new[] { typeof(TService1), typeof(TService2) };
//     }
//
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="serviceLifetime">Lifetime for this registration.</param>
//     public ImplementationOfAttribute(ServiceLifetime serviceLifetime)
//     {
//         ServiceTypes = new[] { typeof(TService1), typeof(TService2) };
//         ServiceLifetime = serviceLifetime;
//     }
// }
//
// /// <summary>
// /// Marks a class for registration as a service implementation.
// /// </summary>
// /// <typeparam name="TService1">Service type 1.</typeparam>
// /// <typeparam name="TService2">Service type 2.</typeparam>
// /// <typeparam name="TService3">Service type 3.</typeparam>
// /// <remarks> A newer equivalent of <see cref="ServiceImplementationAttribute"/>.</remarks>
// [PublicAPI]
// [AttributeUsage(AttributeTargets.Class, Inherited = false)]
// public sealed class ImplementationOfAttribute<TService1, TService2, TService3> : Attribute, IImplementationOfAttribute where TService1 : class
//     where TService2 : class
//     where TService3 : class
// {
//     /// <summary>
//     /// Lifetime to use for the registration if specified.
//     /// </summary>
//     public ServiceLifetime? ServiceLifetime { get; private set; }
//     
//     /// <summary>
//     /// Services types if specified.
//     /// </summary>
//     public Type[]? ServiceTypes { get; private set; }
//     
//     /// <summary>
//     /// Registration strategy if specified.
//     /// </summary>
//     public RegistrationStrategy? RegistrationStrategy { get; private set; }
//     
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     public ImplementationOfAttribute()
//     {
//         ServiceTypes = new[] { typeof(TService1), typeof(TService2), typeof(TService3) };
//     }
//
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="serviceLifetime">Lifetime for this registration.</param>
//     public ImplementationOfAttribute(ServiceLifetime serviceLifetime)
//     {
//         ServiceTypes = new[] { typeof(TService1), typeof(TService2), typeof(TService3) };
//         ServiceLifetime = serviceLifetime;
//     }
// }
//
// /// <summary>
// /// Marks a class for registration as a service implementation.
// /// </summary>
// /// <typeparam name="TService1">Service type 1.</typeparam>
// /// <typeparam name="TService2">Service type 2.</typeparam>
// /// <typeparam name="TService3">Service type 3.</typeparam>
// /// <typeparam name="TService4">Service type 4.</typeparam>
// /// <remarks> A newer equivalent of <see cref="ServiceImplementationAttribute"/>.</remarks>
// [PublicAPI]
// [AttributeUsage(AttributeTargets.Class, Inherited = false)]
// public sealed class ImplementationOfAttribute<TService1, TService2, TService3, TService4> : Attribute, IImplementationOfAttribute where TService1 : class
//     where TService2 : class
//     where TService3 : class
//     where TService4 : class
// {
//     /// <summary>
//     /// Lifetime to use for the registration if specified.
//     /// </summary>
//     public ServiceLifetime? ServiceLifetime { get; private set; }
//     
//     /// <summary>
//     /// Services types if specified.
//     /// </summary>
//     public Type[]? ServiceTypes { get; private set; }
//     
//     /// <summary>
//     /// Registration strategy if specified.
//     /// </summary>
//     public RegistrationStrategy? RegistrationStrategy { get; private set; }
//     
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     public ImplementationOfAttribute()
//     {
//         ServiceTypes = new[] { typeof(TService1), typeof(TService2), typeof(TService3), typeof(TService4) };
//     }
//
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="serviceLifetime">Lifetime for this registration.</param>
//     public ImplementationOfAttribute(ServiceLifetime serviceLifetime)
//     {
//         ServiceTypes = new[] { typeof(TService1), typeof(TService2), typeof(TService3), typeof(TService4) };
//         ServiceLifetime = serviceLifetime;
//     }
// }
// #endif
