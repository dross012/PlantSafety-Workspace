// ============================================================================
//  Test IDs:  PLNT-REG-SQL-001..005
//  Purpose:   Pins the stored-procedure audit and error-logging work. Every
//             assertion here corresponds to a defect that was really present
//             in this repository:
//
//               001  fifteen procedures were defined twice, in two different
//                    files, and the copy that won depended on load order.
//               002  an audit INSERT placed before "SELECT @@ROWCOUNT" reset
//                    @@ROWCOUNT to 1, turning a failed concurrency check into
//                    a reported success.
//               003  an audit block landed after "RETURN 0;" -- unreachable,
//                    so the procedure appeared audited and wrote nothing.
//               004  109 mutating procedures wrote no audit trail at all.
//               005  procedures caught and rethrew without recording the
//                    failure anywhere.
// ============================================================================
using PlantSafety.RegressionTests.Support;

namespace PlantSafety.RegressionTests.Sql;

public sealed class StoredProcedureLoggingRegressionTests
{
    [Fact(DisplayName = "PLNT-REG-SQL-001: No stored procedure is defined in more than one file")]
    public void StoredProcedures_ShouldNotBeDefinedTwice()
    {
        var duplicates = StoredProcedureCorpus.All
            .GroupBy(p => p.QualifiedName, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Select(p => p.FilePath).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .Select(g => $"{g.Key} defined in: {string.Join(", ", g.Select(p => p.FileName).Distinct())}")
            .ToArray();

        // Keyed on schema AND name deliberately. Two procedures may legitimately share a bare
        // name in different schemas; what is never legitimate is one [schema].[name] having two
        // definitions, because then which one the database ends up with is decided by whichever
        // file was applied last.
        duplicates.Should().BeEmpty(
            "a procedure defined in two files silently drifts -- the definitions diverge and the " +
            "one that wins depends on apply order:\n{0}", string.Join("\n", duplicates));
    }

    [Fact(DisplayName = "PLNT-REG-SQL-002: No audit write precedes a SELECT @@ROWCOUNT result")]
    public void AuditWrites_ShouldNotResetRowCountBeforeItIsReturned()
    {
        var offenders = new List<string>();

        foreach (var procedure in StoredProcedureCorpus.All)
        {
            var rowCountReturn = Regex.Match(procedure.Body, @"^[ \t]*SELECT\s+@@ROWCOUNT\s*;", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (!rowCountReturn.Success)
            {
                continue;
            }

            var beforeReturn = procedure.Body[..rowCountReturn.Index];

            if (beforeReturn.Contains("AuditLog", StringComparison.OrdinalIgnoreCase))
            {
                offenders.Add($"{procedure.QualifiedName} ({procedure.FileName})");
            }
        }

        // @@ROWCOUNT reflects the LAST statement executed. An audit INSERT between the mutation
        // and the SELECT overwrites it with 1, so a concurrency-guarded UPDATE that matched zero
        // rows reports one -- the caller believes it won a race it actually lost. The correct
        // shape captures the count into a local immediately after the mutation.
        offenders.Should().BeEmpty(
            "an audit INSERT before SELECT @@ROWCOUNT overwrites the value being returned, " +
            "silently converting a failed optimistic-concurrency check into a success:\n{0}",
            string.Join("\n", offenders));
    }

    [Fact(DisplayName = "PLNT-REG-SQL-003: No audit write is placed after RETURN, where it can never execute")]
    public void AuditWrites_ShouldNotBeUnreachable()
    {
        var offenders = new List<string>();

        foreach (var procedure in StoredProcedureCorpus.All)
        {
            // Anything between a RETURN and the END TRY that follows it is dead code.
            foreach (Match dead in Regex.Matches(
                         procedure.Body,
                         @"^[ \t]*RETURN\s+\d+\s*;(?<dead>.*?)^[ \t]*END\s+TRY",
                         RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase))
            {
                if (dead.Groups["dead"].Value.Contains("AuditLog", StringComparison.OrdinalIgnoreCase))
                {
                    offenders.Add($"{procedure.QualifiedName} ({procedure.FileName})");
                }
            }
        }

        // This is the worse half of an audit gap. A missing trail is at least visibly missing;
        // an unreachable one makes the procedure look audited to every reviewer and every grep,
        // while writing nothing.
        offenders.Should().BeEmpty(
            "an audit block after RETURN never executes, so the procedure reads as audited but " +
            "produces no trail:\n{0}", string.Join("\n", offenders));
    }

    [Fact(DisplayName = "PLNT-REG-SQL-004: Every procedure that mutates a domain table writes an audit trail")]
    public void MutatingProcedures_ShouldWriteAnAuditTrail()
    {
        var unaudited = StoredProcedureCorpus.All
            .Where(ProcedureAnalysis.IsMutating)
            .Where(p => !ProcedureAnalysis.WritesAuditTrail(p))
            .Where(p => !LoggingExclusions.AuditExempt.ContainsKey(p.Name))
            .Select(p => $"{p.QualifiedName} mutates {string.Join(", ", ProcedureAnalysis.MutatedTables(p))} ({p.FileName})")
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();

        unaudited.Should().BeEmpty(
            "every domain write must be attributable. A new mutating procedure either records an " +
            "audit row or is added to LoggingExclusions with a written reason -- there is no third " +
            "option that leaves the decision unrecorded:\n{0}", string.Join("\n", unaudited));
    }

    [Fact(DisplayName = "PLNT-REG-SQL-005: Every procedure that catches an error records it before rethrowing")]
    public void ProceduresWithCatchBlocks_ShouldRecordTheError()
    {
        var silent = StoredProcedureCorpus.All
            .Where(ProcedureAnalysis.IsMutating)
            .Where(ProcedureAnalysis.HasCatchBlock)
            .Where(p => !ProcedureAnalysis.WritesErrorTrail(p))
            .Where(p => !LoggingExclusions.ErrorExempt.ContainsKey(p.Name))
            .Select(p => $"{p.QualifiedName} ({p.FileName})")
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();

        silent.Should().BeEmpty(
            "a CATCH that rethrows without writing ErrorLog loses the only server-side record of " +
            "the failure -- the caller sees an exception, operations sees nothing:\n{0}",
            string.Join("\n", silent));
    }

    [Fact(DisplayName = "PLNT-REG-SQL-006: Every logging exclusion names a procedure that still exists")]
    public void LoggingExclusions_ShouldNotOutliveTheirProcedures()
    {
        var known = StoredProcedureCorpus.All.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var stale = LoggingExclusions.AuditExempt.Keys
            .Concat(LoggingExclusions.ErrorExempt.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(name => !known.Contains(name))
            .ToArray();

        // Without this, an exclusion outlives the procedure it was written for and then silently
        // adopts the next procedure that happens to be given the same name.
        stale.Should().BeEmpty(
            "these exclusions name procedures that no longer exist and should be deleted:\n{0}",
            string.Join("\n", stale));
    }
}
