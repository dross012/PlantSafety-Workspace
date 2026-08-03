// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.DeadLetter
//  Layer:          Infrastructure
//  File:           DeadLetterConsumerRegistration.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
//  SchemaVersion:  2026.1
//  Description:    Registers the single, message-type-agnostic DeadLetterFaultConsumer{TMessage}
//                  open generic consumer with the MassTransit configurator, mirroring the
//                  per-module *ConsumerRegistration.Register(cfg) convention established elsewhere
//                  under Messaging.Consumers. Called from InfrastructureServiceRegistration
//                  alongside every other *ConsumerRegistration.Register(x) call. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.DeadLetter;

/// <summary>
/// Registers the generic MassTransit dead-letter fault consumer with the bus configurator.
/// </summary>
public static class DeadLetterConsumerRegistration
{
    /// <summary>
    /// Adds the open-generic <see cref="DeadLetterFaultConsumer{TMessage}"/> to the MassTransit
    /// configurator. MassTransit resolves and binds the closed generic consumer type for every
    /// message type that has at least one registered consumer, so this single call covers every
    /// existing and future consumer's fault path with no per-event-type registration required.
    /// </summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer(typeof(DeadLetterFaultConsumer<>));
    }
}
