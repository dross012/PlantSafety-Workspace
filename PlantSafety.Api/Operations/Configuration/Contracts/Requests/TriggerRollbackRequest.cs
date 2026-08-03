// ============================================================================
// File: TriggerRollbackRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: Request body for POST /api/v1/operations/configuration/{databaseKey}/feature-flags/{flagName}/rollback.
// ============================================================================
namespace PlantSafety.Api.Operations.Configuration.Contracts.Requests;

/// <summary>Request body to trigger a real feature-flag rollback for a bad release.</summary>
/// <param name="Reason">The reason for the rollback.</param>
/// <param name="HardDisable">When true, also forces the global flag's IsEnabled to false.</param>
public sealed record TriggerRollbackRequest(string Reason, bool HardDisable = false);
