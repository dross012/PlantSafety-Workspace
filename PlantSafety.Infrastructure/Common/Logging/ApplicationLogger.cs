// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           ApplicationLogger.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
//  SchemaVersion:  2026.1
//  Description:    Infrastructure-layer implementation of IApplicationLogger (Application
//                  Abstractions.Logging). Previously this interface had ZERO registered
//                  implementation and ZERO consumers anywhere in the solution -- a dead
//                  abstraction that could not enforce the Canonical Log Envelope Rule
//                  (Handbook §2.22) for any caller. Closes that gap: emits exactly one
//                  structured envelope object per call via Serilog's "@" destructuring
//                  operator (Handbook §2.19-2.22, §2.30), never a message template.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Logging;

namespace PlantSafety.Infrastructure.Common.Logging;

/// <summary>
/// Serilog/<see cref="ILogger"/>-backed implementation of <see cref="IApplicationLogger"/>.
/// Emits one structured envelope object per call ("{@Envelope}") -- never a message template,
/// never concatenated strings -- per the Canonical Log Envelope Rule (§2.22).
/// </summary>
public sealed class ApplicationLogger(ILogger<ApplicationLogger> logger) : IApplicationLogger
{
    /// <inheritdoc/>
    public void LogSuccess(OperationLogEnvelope envelope) =>
        logger.LogInformation("{@Success}", envelope);

    /// <inheritdoc/>
    public void LogWarning(OperationLogEnvelope envelope) =>
        logger.LogWarning("{@Warning}", envelope);

    /// <inheritdoc/>
    public void LogError(OperationLogEnvelope envelope) =>
        logger.LogError("{@Error}", envelope);

    /// <inheritdoc/>
    public void LogDebug(OperationLogEnvelope envelope) =>
        logger.LogDebug("{@Debug}", envelope);
}
