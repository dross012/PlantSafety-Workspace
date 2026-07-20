// ============================================================================
// File: TriggerBackupRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-19
// Description: Request body for POST /api/v1/operations/backup/trigger.
// ============================================================================
using PlantSafety.Application.Operations.Backup.Models;

namespace PlantSafety.Api.Operations.Backup.Contracts.Requests;

/// <summary>Request body to trigger an on-demand backup.</summary>
/// <param name="BackupType">Full | Diff | Log.</param>
public sealed record TriggerBackupRequest(BackupType BackupType);
