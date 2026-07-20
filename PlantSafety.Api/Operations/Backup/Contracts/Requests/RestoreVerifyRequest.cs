// ============================================================================
// File: RestoreVerifyRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-19
// Description: Request body for POST /api/v1/operations/backup/restore-verify. This
//   triggers ONLY the safe, isolated msdb.dbo.usp_PlantSafety_RestoreTest -- never a
//   live production restore. See DR-manual-restore-runbook.md for production restore.
// ============================================================================
namespace PlantSafety.Api.Operations.Backup.Contracts.Requests;

/// <summary>Request body to run a safe restore-verify against a database's latest FULL backup.</summary>
/// <param name="DatabaseName">The PlantSafety.* database to restore-verify.</param>
public sealed record RestoreVerifyRequest(string DatabaseName);
