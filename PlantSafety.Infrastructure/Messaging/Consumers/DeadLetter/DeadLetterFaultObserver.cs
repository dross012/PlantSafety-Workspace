// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.DeadLetter
//  Layer:          Infrastructure
//  File:           DeadLetterFaultObserver.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6 hardening)
//  SchemaVersion:  2026.1
//  Description:    Global, message-type-agnostic MassTransit fault sink implemented as an
//                  IConsumeObserver (NOT an open-generic IConsumer<Fault<TMessage>>). Registered
//                  ONCE, as a normal singleton, and connected via
//                  cfg.ConnectConsumeObserver(...) inside x.UsingRabbitMq(...)
//                  (InfrastructureServiceRegistration.cs) -- covers every existing/future
//                  consumer's fault path with no per-event-type or open-generic registration.
//                  Replaces the prior DeadLetterFaultConsumer<TMessage> (open-generic
//                  IConsumer<Fault<TMessage>>) approach, which registered an internal MassTransit
//                  RegisterConsumer<> marker type containing an unbound generic parameter that
//                  PlantSafety.Api's strict UseDefaultServiceProvider(ValidateOnBuild = true)
//                  eagerly tried to Activator.CreateInstance at startup, crashing with
//                  "Cannot create an instance of ... because Type.ContainsGenericParameters is
//                  true." IConsumeObserver.ConsumeFault<T> fires once per permanently-faulted
//                  message (after cfg.UseMessageRetry(...) exhausts its retry budget) -- the same
//                  semantics the old Fault<T> consumption relied on -- without ever registering an
//                  open generic type in the DI container. See DeadLetterEventPersister for the
//                  actual persistence logic (extracted, reusable, non-generic-registration).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.DeadLetter;

/// <summary>
/// Global MassTransit consume observer that persists every permanently-faulted message (of any
/// type) to <c>[System].[DeadLetterEvents]</c> via <see cref="IDeadLetterEventPersister"/>.
/// </summary>
public sealed class DeadLetterFaultObserver(IDeadLetterEventPersister persister) : IConsumeObserver
{
    /// <inheritdoc/>
    /// <remarks>Not needed for dead-letter persistence -- intentionally a no-op.</remarks>
    public Task PreConsume<T>(ConsumeContext<T> context) where T : class => Task.CompletedTask;

    /// <inheritdoc/>
    /// <remarks>Not needed for dead-letter persistence -- intentionally a no-op.</remarks>
    public Task PostConsume<T>(ConsumeContext<T> context) where T : class => Task.CompletedTask;

    /// <inheritdoc/>
    /// <remarks>
    /// Fires once per message type <typeparamref name="T"/> after MassTransit's retry pipeline
    /// (<c>cfg.UseMessageRetry(...)</c>) has exhausted every configured attempt -- the same point
    /// at which MassTransit's built-in <c>Fault{T}</c> would previously have been published and
    /// consumed. Delegates straight to <see cref="IDeadLetterEventPersister"/>, which never throws
    /// back into this callback (all persistence failures are caught and logged internally).
    /// </remarks>
    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class =>
        persister.PersistAsync(context, exception);
}
