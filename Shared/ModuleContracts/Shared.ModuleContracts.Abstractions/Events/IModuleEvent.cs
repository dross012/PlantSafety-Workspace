// ============================================================================
//  Project:        Shared.ModuleContracts
//  Module:         Abstractions.Events
//  File:           IModuleEvent.cs
//  Created:        2026-08-09
//  Description:    Marker for a fact published by one module that other
//                  modules may react to -- the replacement for a direct
//                  method call from module B into module A when B only
//                  needs to know "A happened," not to command A directly.
// ============================================================================
namespace Shared.ModuleContracts.Abstractions.Events;

/// <summary>Marker for a fact published by one module, published via IModuleEventGateway.</summary>
public interface IModuleEvent
{
    /// <summary>The module that published this event.</summary>
    string SourceModule { get; }

    /// <summary>When the event occurred, in UTC.</summary>
    DateTime OccurredAtUtc { get; }
}
