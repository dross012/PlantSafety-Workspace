// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RegressionTests.Support
//  Layer:          Tests (Regression)
//  Description:    Classifies a stored procedure as mutating or read-only and
//                  reports whether it writes the canonical audit and error
//                  trails. Shared by the logging-coverage regression tests.
// ============================================================================
namespace PlantSafety.RegressionTests.Support;

/// <summary>Static analysis of a single stored-procedure body.</summary>
public static class ProcedureAnalysis
{
    // Matches a mutation whose target is written as [schema].[Table]. That form is the codebase's
    // convention for real domain tables, and it deliberately does NOT match:
    //   * the audit/error writes themselves, which target [PlantSafety.AuditDB].dbo.AuditLog
    //     (a [database].dbo.Table shape, with no bracketed second part), and
    //   * temp tables (#Staging) and table variables (@Rows), which are procedure-local scratch
    //     space and not something an audit trail should record.
    private static readonly Regex Mutation = new(
        @"^\s*(?<op>INSERT\s+INTO|UPDATE|DELETE\s+FROM|MERGE)\s+\[(?<schema>[^\]]+)\]\s*\.\s*\[(?<table>[^\]]+)\]",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>True if the procedure writes to at least one real domain table.</summary>
    public static bool IsMutating(StoredProcedureDefinition procedure) =>
        MutatedTables(procedure).Count > 0;

    /// <summary>Gets the distinct domain tables the procedure mutates, in source order.</summary>
    public static IReadOnlyList<string> MutatedTables(StoredProcedureDefinition procedure)
    {
        var tables = new List<string>();

        foreach (Match match in Mutation.Matches(procedure.Body))
        {
            var table = match.Groups["table"].Value;

            // Audit/error bookkeeping is not itself a domain mutation. Counting it would make
            // every audited procedure trivially "mutating and audited", which is circular.
            if (table.Equals("AuditLog", StringComparison.OrdinalIgnoreCase) ||
                table.Equals("ErrorLog", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!tables.Contains(table, StringComparer.OrdinalIgnoreCase))
            {
                tables.Add(table);
            }
        }

        return tables;
    }

    /// <summary>True if the procedure writes an AuditLog row.</summary>
    public static bool WritesAuditTrail(StoredProcedureDefinition procedure) =>
        procedure.Body.Contains("AuditLog", StringComparison.OrdinalIgnoreCase);

    /// <summary>True if the procedure writes an ErrorLog row.</summary>
    public static bool WritesErrorTrail(StoredProcedureDefinition procedure) =>
        procedure.Body.Contains("ErrorLog", StringComparison.OrdinalIgnoreCase);

    /// <summary>True if the procedure has a CATCH block, i.e. somewhere for an error write to live.</summary>
    public static bool HasCatchBlock(StoredProcedureDefinition procedure) =>
        procedure.Body.Contains("BEGIN CATCH", StringComparison.OrdinalIgnoreCase);
}
