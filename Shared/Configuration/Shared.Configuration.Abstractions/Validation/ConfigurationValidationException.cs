// ============================================================================
//  Project:        Shared.Configuration
//  Module:         Abstractions.Validation
//  File:           ConfigurationValidationException.cs
//  Created:        2026-08-09
//  Description:    Thrown by a host at startup when EnvironmentManifestValidator
//                  finds one or more problems -- the host is refused from
//                  starting with invalid configuration rather than
//                  starting and failing unpredictably later at first use.
// ============================================================================
namespace Shared.Configuration.Abstractions.Validation;

/// <summary>Thrown at startup when the environment manifest fails validation.</summary>
public sealed class ConfigurationValidationException(IReadOnlyList<string> errors)
    : Exception(BuildMessage(errors))
{
    /// <summary>Every validation problem found.</summary>
    public IReadOnlyList<string> Errors { get; } = errors;

    private static string BuildMessage(IReadOnlyList<string> errors) =>
        $"Environment manifest failed validation with {errors.Count} problem(s):{Environment.NewLine}" +
        string.Join(Environment.NewLine, errors.Select(e => $"  - {e}"));
}
