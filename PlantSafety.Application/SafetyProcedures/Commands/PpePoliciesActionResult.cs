// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for PpePolicies lifecycle commands ("SafetyProcedures.PpePolicies").
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Result of a successful PpePolicies lifecycle command execution.</summary>
/// <param name="PpePoliciesId">Identity of the affected PpePolicies aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PpePoliciesActionResult(
    Guid PpePoliciesId,
    string Status);