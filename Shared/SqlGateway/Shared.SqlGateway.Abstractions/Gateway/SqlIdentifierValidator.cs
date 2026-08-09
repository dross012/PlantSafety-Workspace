// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Abstractions.Gateway
//  File:           SqlIdentifierValidator.cs
//  Created:        2026-08-09
//  Description:    Phase 2.6 deliverable: enforces "no dynamic SQL"
//                  structurally. ISqlGateway's methods only ever accept a
//                  stored-procedure NAME (a bounded identifier), never a
//                  SQL text string -- there is no code path through this
//                  gateway that could execute caller-supplied SQL text.
//                  This validator is the seam that rejects anything that
//                  isn't a plausible two-part (schema.procedure) or one-
//                  part identifier, catching accidental or malicious
//                  attempts to smuggle SQL text (spaces, semicolons,
//                  comment markers, quotes) in through the "name" param.
// ============================================================================
using System.Text.RegularExpressions;

namespace Shared.SqlGateway.Abstractions.Gateway;

/// <summary>Validates that a string is a plausible SQL Server stored-procedure identifier, never raw SQL text.</summary>
public static partial class SqlIdentifierValidator
{
    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)?$")]
    private static partial Regex IdentifierPattern();

    /// <summary>True if the given name is a plausible one- or two-part SQL identifier (e.g. "dbo.usp_GetHazardSummary").</summary>
    public static bool IsValidStoredProcedureName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && IdentifierPattern().IsMatch(name);

    /// <summary>Throws if the given name is not a valid stored-procedure identifier.</summary>
    public static void EnsureValid(string? name)
    {
        if (!IsValidStoredProcedureName(name))
        {
            throw new ArgumentException(
                $"'{name}' is not a valid stored-procedure identifier. ISqlGateway never accepts raw SQL text -- only a schema.procedure name.",
                nameof(name));
        }
    }
}
