// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateSafetyPolicyCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyPolicies.Commands;

/// <summary>Result of a successful <see cref="CreateSafetyPolicyCommand"/> execution.</summary>
/// <param name="SafetyPolicyId">Identity of the newly created SafetyPolicy aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateSafetyPolicyResult(
    Guid SafetyPolicyId,
    string Status);