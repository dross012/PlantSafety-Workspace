// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           EventBusException.cs
//  Created:        2026-07-27
// ============================================================================
namespace Shared.EventBus.Infrastructure.Exceptions;

/// <summary>Thrown when publishing or dispatching an event fails at the infrastructure level.</summary>
public sealed class EventBusException : Exception
{
    public EventBusException(string message) : base(message) { }
    public EventBusException(string message, Exception innerException) : base(message, innerException) { }
}
