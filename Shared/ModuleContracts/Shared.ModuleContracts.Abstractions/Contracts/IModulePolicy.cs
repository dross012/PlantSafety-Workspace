// ============================================================================
//  Project:        Shared.ModuleContracts
//  Module:         Abstractions.Contracts
//  File:           IModulePolicy.cs
//  Created:        2026-08-09
//  Description:    A policy is a deterministic, side-effect-free rule
//                  evaluated against a module's own data before a command
//                  is allowed to proceed -- e.g. "a facility cannot be
//                  decommissioned while it has active machines." Kept
//                  separate from the command handler itself so the rule
//                  can be unit-tested and reused independent of the
//                  handler's persistence/orchestration concerns.
// ============================================================================
namespace Shared.ModuleContracts.Abstractions.Contracts;

/// <summary>A deterministic policy rule evaluated against a module's own data.</summary>
public interface IModulePolicy<in TContext>
{
    /// <summary>Evaluates the policy, returning a human-readable violation reason if it fails, or null if it passes.</summary>
    string? Evaluate(TContext context);
}
