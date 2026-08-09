// ============================================================================
//  Project:        Shared.ModuleContracts
//  Module:         Abstractions.Events
//  File:           IModuleEventGateway.cs
//  Created:        2026-08-09
//  Description:    Phase 2.7 deliverable: the event gateway. The one
//                  approved seam for publishing a module event and for
//                  subscribing to another module's events -- replaces a
//                  direct cross-module method call ("tell module A to
//                  also do X") with "publish that this happened, let
//                  interested subscribers react."
// ============================================================================
namespace Shared.ModuleContracts.Abstractions.Events;

/// <summary>The one approved seam for publishing and subscribing to module events.</summary>
public interface IModuleEventGateway
{
    /// <summary>Publishes an event to every subscriber registered for its type.</summary>
    Task PublishAsync<TEvent>(TEvent moduleEvent, CancellationToken cancellationToken = default) where TEvent : IModuleEvent;

    /// <summary>Subscribes a handler to every future event of the given type. Returns a token to unsubscribe.</summary>
    IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IModuleEvent;
}
