// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Envelope
//  File:           ErrorEnvelope.cs
//  Created:        2026-08-09
//  Description:    Phase 2.3 deliverable: the unified error shape every
//                  module's failures are standardized into. UserMessage is
//                  always safe to show an end user; DeveloperMessage may
//                  contain implementation detail (the real exception
//                  message) and is meant for logs/diagnostics, not the UI.
// ============================================================================
namespace Shared.ErrorHandling.Abstractions.Envelope;

/// <summary>The unified error shape every module's failures are standardized into.</summary>
public sealed record ErrorEnvelope(
    string ErrorCode,
    ErrorCategory Category,
    string UserMessage,
    string DeveloperMessage,
    string CorrelationId,
    string Module,
    DateTime OccurredAtUtc);
