/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               LogLevel.cs
    DESCRIPTION:        Canonical log level enumeration. Decouples domain/application logging semantics from any specific
                        infrastructure logging framework. Used across all logging abstractions and envelopes.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. New levels MAY be added; existing values MUST NOT change semantics or ordering.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Canonical log level enumeration used across all modules and layers.
/// This MUST be mapped deterministically to infrastructure-specific levels.
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Info = 2,
    Warn = 3,
    Error = 4,
    Fatal = 5
}
