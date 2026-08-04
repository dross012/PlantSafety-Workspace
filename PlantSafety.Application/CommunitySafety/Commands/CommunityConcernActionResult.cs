// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for CommunityConcern commands ("CommunitySafety.CommunityConcern").
// SchemaVersion: 1

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Result of a successful CommunityConcern command execution.</summary>
/// <param name="CommunityConcernId">Identity of the affected CommunityConcern aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record CommunityConcernActionResult(
    Guid CommunityConcernId,
    string Status);
