// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           MessagingException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a messaging publish or consume operation fails.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Exceptions;

/// <summary>Thrown when a messaging (Service Bus) publish or consume operation fails.</summary>
public sealed class MessagingException : InfrastructureException
{
    /// <summary>Integration event type that failed to publish or consume.</summary>
    public string EventType { get; }

    /// <summary>Creates a messaging exception for the given event type.</summary>
    public MessagingException(string eventType, string correlationId, Exception? inner = null)
        : base($"Messaging operation failed for event type '{eventType}'.",
               "Messaging.PublishFailed", correlationId, inner)
    {
        EventType = eventType;
    }
}
