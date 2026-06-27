/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ILogFormatter.cs
    DESCRIPTION:        Abstraction for formatting canonical LogEvent instances into a deterministic, SIEM-safe serialized
                        representation suitable for storage or transport.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Formats canonical <see cref="LogEvent"/> instances into deterministic serialized representations.
/// Implementations MUST be stable and additive-only.
/// </summary>
public interface ILogFormatter
{
    string Format(LogEvent logEvent);
}
