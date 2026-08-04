// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for LogCommunicationEntryCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Result of a successful LogCommunicationEntryCommand execution.</summary>
/// <param name="CommunicationLogEntryId">Identity of the newly recorded CommunicationLogEntry aggregate.</param>
public sealed record CommunicationLogEntryActionResult(
    Guid CommunicationLogEntryId);
