// ============================================================================
//  Project:        Shared.ErrorHandling.Infrastructure
//  Module:         Governance
//  File:           ErrorGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 12 (Unified Contract & Schema Modernization)
//                  deliverable. ErrorEnvelope (Phase 2.3) is already the
//                  unified error shape this program uses everywhere --
//                  ErrorCode, Category (the closed ErrorCategory enum),
//                  UserMessage, DeveloperMessage, CorrelationId, Module,
//                  OccurredAtUtc -- built by ErrorEnvelopeFactory.FromException
//                  (which already maps UnauthorizedAccessException ->
//                  ErrorCategory.Unauthorized via ExceptionClassifier, per
//                  this phase's own requirement) and consumed by
//                  GlobalErrorHandler on every unhandled exception. No new
//                  "UnifiedErrorBase"/"ErrorBase" type was introduced --
//                  ErrorEnvelope already satisfies that role; a parallel
//                  type would be exactly the "second, competing error
//                  system" this program's own precedent (Phase 6's
//                  IModuleEventGateway vs. Shared.EventBus discovery)
//                  warns against repeating.
//
//                  This file adds the one governance gap that didn't exist
//                  before: pure validation that a given ErrorEnvelope's own
//                  shape is well-formed (non-empty ErrorCode/CorrelationId/
//                  Module, non-default OccurredAtUtc, Category is a defined
//                  ErrorCategory value -- never a new category).
// ============================================================================
using Shared.ErrorHandling.Abstractions.Envelope;

namespace Shared.ErrorHandling.Infrastructure.Governance;

/// <summary>Pure validation rules for an ErrorEnvelope's shape.</summary>
public static class ErrorGovernanceRules
{
    /// <summary>
    /// Validates <paramref name="envelope"/>. Reports "MissingErrorEnvelope" when null;
    /// "MissingErrorCode"/"MissingCorrelationId"/"MissingModule" when the respective string is
    /// empty/whitespace; "InvalidTimestamp" when OccurredAtUtc is default(DateTime);
    /// "UndefinedErrorCategory" when Category is not one of the closed ErrorCategory values.
    /// </summary>
    public static IReadOnlyList<string> Validate(ErrorEnvelope? envelope)
    {
        var violations = new List<string>();

        if (envelope is null)
        {
            violations.Add("MissingErrorEnvelope");
            return violations;
        }

        if (string.IsNullOrWhiteSpace(envelope.ErrorCode))
        {
            violations.Add("MissingErrorCode");
        }

        if (string.IsNullOrWhiteSpace(envelope.CorrelationId))
        {
            violations.Add("MissingCorrelationId");
        }

        if (string.IsNullOrWhiteSpace(envelope.Module))
        {
            violations.Add("MissingModule");
        }

        if (envelope.OccurredAtUtc == default)
        {
            violations.Add("InvalidTimestamp");
        }

        if (!Enum.IsDefined(envelope.Category))
        {
            violations.Add("UndefinedErrorCategory");
        }

        return violations;
    }
}
