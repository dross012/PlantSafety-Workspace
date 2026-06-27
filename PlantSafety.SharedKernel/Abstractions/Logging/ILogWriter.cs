/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ILogWriter.cs
    DESCRIPTION:        Abstraction for writing formatted log entries to the underlying sink (files, console, SIEM, etc.).
                        Decouples application logging from storage concerns.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Writes formatted log entries to the underlying sink.
/// MUST be deterministic and MUST NOT throw.
/// </summary>
public interface ILogWriter
{
    void Write(string formattedLogEntry);
}
