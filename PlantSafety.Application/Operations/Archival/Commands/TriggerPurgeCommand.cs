// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to trigger an on-demand retention purge for a single target,
//   or all 5 targets when Target is null. Runs the SAME already-scheduled, already-live
//   stored procedures early (see IArchivalRepository for the safety rationale).
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Archival.Models;

namespace PlantSafety.Application.Operations.Archival.Commands;

/// <summary>Triggers an on-demand retention purge.</summary>
/// <param name="Target">The single target to purge, or null to purge all 5 targets.</param>
public sealed record TriggerPurgeCommand(PurgeTarget? Target) : IRequest<Result<IReadOnlyList<PurgeTriggerResultDto>>>;
