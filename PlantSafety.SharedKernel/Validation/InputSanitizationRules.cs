// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Shared Kernel
//  Module:         Validation
//  Layer:          SharedKernel (Rules)
//  File:           InputSanitizationRules.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Centralized input sanitization rules used across all modules. These
//      rules enforce safety against SQL injection, command injection, and
//      malformed or hostile input patterns.
//
//  Architectural Notes:
//      • Sanitization rules MUST be deterministic (Rule 1.4).
//      • Sanitization rules MUST NOT mutate input (Rule 1.5).
//      • Sanitization rules MUST be side-effect free (Rule 1.1).
//      • Sanitization rules MUST be reusable across all modules (Rule 8.1).
//
//  Onboarding Notes:
//      • These rules are used by FluentValidation validators.
//      • They do NOT replace domain invariants.
//      • They are the first line of defense against hostile input.
// ============================================================================
namespace PlantSafety.SharedKernel.Validation;

public static class InputSanitizationRules
{
    private static readonly Regex SqlInjectionPattern = new(
        @"(--|\b(SELECT|INSERT|UPDATE|DELETE|DROP|ALTER|EXEC|UNION)\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _dangerousCharactersPattern = new(
        @"[<>;{}$`|]",
        RegexOptions.Compiled);

    public static bool NoSqlOrCommandInjection(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        if (SqlInjectionPattern.IsMatch(input))
            return false;

        if (_dangerousCharactersPattern.IsMatch(input))
            return false;

        return true;
    }
}
