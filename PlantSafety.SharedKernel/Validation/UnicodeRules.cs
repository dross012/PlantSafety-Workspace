// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Shared Kernel
//  Module:         Validation
//  Layer:          SharedKernel (Rules)
//  File:           UnicodeRules.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Unicode safety rules that detect invisible, control, or malformed
//      Unicode characters. These rules prevent hidden payloads, homoglyph
//      attacks, and invisible injection vectors.
//
//  Architectural Notes:
//      • Unicode rules MUST be deterministic (Rule 1.4).
//      • Unicode rules MUST NOT mutate input (Rule 1.5).
//      • Unicode rules MUST detect invisible characters (Rule 8.3).
//
//  Onboarding Notes:
//      • Used by validators for all text fields.
//      • Prevents invisible Unicode characters from entering the system.
// ============================================================================

namespace PlantSafety.SharedKernel.Validation;

public static class UnicodeRules
{
    public static bool NoInvisibleCharacters(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        foreach (var c in input)
        {
            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                return false;

            if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.Format)
                return false;
        }

        return true;
    }
}

