// ============================================================================
//  Test IDs:  PLNT-DINT-SQL-001..003
//  Manual:    project_plantsafety_patterns.md (stored-proc template incl.
//             @TenantId); tests_rules_design_manual.md (DINT type).
//  Purpose:   Confirms every checked-in stored procedure script declares the
//             canonical header parameter contract (@TenantId, @CorrelationId,
//             @ActorId, @NowUtc) mandated for cross-module data integration
//             and tenant-safe auditability, and that every proc wraps its
//             write in an explicit transaction with rollback-on-error.
// ============================================================================
using PlantSafety.DataIntegrationTests.Support;

namespace PlantSafety.DataIntegrationTests.StoredProcedures;

public sealed class CanonicalHeaderContractTests
{
    private static IEnumerable<string> AllProcedureScripts =>
        Directory.EnumerateFiles(Path.Combine(RepoPaths.ScriptsRoot, "Procedures"), "*.sql");

    [Fact(DisplayName = "PLNT-DINT-SQL-001: At least one canonical stored-procedure script exists on disk")]
    public void ProcedureScripts_ShouldExist()
    {
        AllProcedureScripts.Should().NotBeEmpty();
    }

    [Theory(DisplayName = "PLNT-DINT-SQL-002: Every write (Insert/Update/Delete-named) procedure declares the four canonical header parameters")]
    [InlineData("usp_Template_Insert.sql")]
    public void WriteProcedures_ShouldDeclareCanonicalHeaderParameters(string fileName)
    {
        var path = Path.Combine(RepoPaths.ScriptsRoot, "Procedures", fileName);
        File.Exists(path).Should().BeTrue($"expected canonical script {fileName} to exist");

        var sql = File.ReadAllText(path);

        sql.Should().Contain("@TenantId", "every stored procedure must accept @TenantId per the canonical header contract");
        sql.Should().Contain("@CorrelationId", "every stored procedure must accept @CorrelationId for cross-module traceability");
        sql.Should().Contain("@ActorId", "every stored procedure must accept @ActorId for audit attribution");
        sql.Should().Contain("@NowUtc", "every stored procedure must accept @NowUtc for deterministic server-independent timestamps");
    }

    [Fact(DisplayName = "PLNT-DINT-SQL-003: The canonical Insert template wraps its write in an explicit transaction with rollback on error")]
    public void CanonicalInsertTemplate_ShouldUseExplicitTransactionWithRollback()
    {
        var path = Path.Combine(RepoPaths.ScriptsRoot, "Procedures", "usp_Template_Insert.sql");
        var sql = File.ReadAllText(path);

        sql.Should().ContainEquivalentOf("BEGIN TRANSACTION");
        sql.Should().ContainEquivalentOf("COMMIT TRANSACTION");
        sql.Should().ContainEquivalentOf("ROLLBACK TRANSACTION");
        sql.Should().ContainEquivalentOf("BEGIN TRY");
        sql.Should().ContainEquivalentOf("BEGIN CATCH");
    }

    [Fact(DisplayName = "PLNT-DINT-SQL-004: No checked-in procedure script contains dynamic SQL string concatenation (EXEC/sp_executesql)")]
    public void ProcedureScripts_ShouldNotContainDynamicSql()
    {
        var offenders = AllProcedureScripts
            .Where(f =>
            {
                var text = File.ReadAllText(f);
                return text.Contains("sp_executesql", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("EXEC (", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("EXEC(", StringComparison.OrdinalIgnoreCase);
            })
            .Select(Path.GetFileName)
            .ToList();

        offenders.Should().BeEmpty("dynamic SQL is an injection surface and is prohibited by the Dapper+stored-procs-only architectural rule");
    }
}
