// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Contracts
//  File:           ContractValidationRules.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Pure validation of a
//                  ContractMetadata's shape -- mirrors AuthorizationCheck
//                  (Phase 9)'s own pattern: a static method, no I/O, no
//                  side effects, returns a violation-name list (empty when
//                  valid).
// ============================================================================
namespace Shared.Authorization.Abstractions.Contracts;

/// <summary>Pure validation rules for a ContractMetadata's shape.</summary>
public static class ContractValidationRules
{
    /// <summary>
    /// Validates <paramref name="metadata"/>. Reports "MissingContractMetadata" when null;
    /// "MissingCallerRole"/"MissingCorrelationId"/"MissingTenantId" when the respective string is
    /// empty/whitespace; "InvalidTimestamp" when TimestampUtc is default(DateTime).
    /// </summary>
    public static IReadOnlyList<string> Validate(ContractMetadata? metadata)
    {
        var violations = new List<string>();

        if (metadata is null)
        {
            violations.Add("MissingContractMetadata");
            return violations;
        }

        if (string.IsNullOrWhiteSpace(metadata.CallerRole))
        {
            violations.Add("MissingCallerRole");
        }

        if (string.IsNullOrWhiteSpace(metadata.CorrelationId))
        {
            violations.Add("MissingCorrelationId");
        }

        if (string.IsNullOrWhiteSpace(metadata.TenantId))
        {
            violations.Add("MissingTenantId");
        }

        if (metadata.TimestampUtc == default)
        {
            violations.Add("InvalidTimestamp");
        }

        return violations;
    }
}
