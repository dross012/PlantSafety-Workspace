// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           CreatePolicyResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreatePolicyCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Policy.Commands;

/// <summary>Result of a successful <see cref="CreatePolicyCommand"/> execution.</summary>
/// <param name="PolicyId">Identity of the newly created Policy aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreatePolicyResult(
    Guid PolicyId,
    string Status);
