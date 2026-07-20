// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           ErrorClassifier.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Maps SqlException.Number values raised by the canonical Phase6
//                  stored-procedure template (and its Phase17 reliability follow-up)
//                  into the platform's existing domain exception taxonomy, so Dapper
//                  callers stop receiving a generic PersistenceException for every SQL
//                  failure. Single mapping chokepoint — no plugin/strategy abstraction.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.SharedKernel.Exceptions;
using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.Infrastructure.Common.Exceptions;

/// <summary>
/// Classifies a <see cref="SqlException"/> raised by a stored procedure call into the
/// platform's canonical exception taxonomy (<see cref="ConflictException"/>,
/// <see cref="ConcurrencyConflictException"/>, <see cref="NotFoundException"/>, or
/// <see cref="PersistenceException"/>). Used by <c>DapperCommandExecutor</c> and
/// <c>DapperQueryExecutor</c> in their <c>catch (SqlException)</c> blocks.
/// </summary>
public static class ErrorClassifier
{
    /// <summary>SQL Server unique-index/unique-constraint violation (duplicate key).</summary>
    public const int DuplicateKeyErrorNumber = 2627;

    /// <summary>
    /// Canonical optimistic-concurrency check raised by every Phase6 stored procedure
    /// (<c>THROW 50002, N'Concurrency conflict...'</c>).
    /// </summary>
    public const int ConcurrencyConflictErrorNumber = 50002;

    /// <summary>
    /// Canonical "record not found or has been deleted" raised pervasively by the Phase6
    /// stored procedures (<c>THROW 50004, N'Record not found or has been deleted.'</c>).
    /// </summary>
    public const int NotFoundErrorNumber = 50004;

    /// <summary>
    /// "The current transaction cannot be committed and cannot support operations that
    /// write to the log file" — documented in Phase17
    /// (170-fix-errorlog-masking-bug.sql) as a symptom of the CATCH block's own ErrorLog
    /// INSERT failing and MASKING the real underlying error. MUST NOT be trusted as the
    /// true failure cause; callers should never build retry/success logic around it.
    /// </summary>
    public const int MaskedTransactionErrorNumber = 3930;

    /// <summary>
    /// Classifies <paramref name="ex"/>, logs it at the appropriate severity, and returns
    /// the exception that should be thrown in its place. Does not throw itself.
    /// </summary>
    /// <param name="ex">The <see cref="SqlException"/> caught around the stored procedure call.</param>
    /// <param name="procedureName">The stored procedure that raised the error.</param>
    /// <param name="logger">Logger of the calling executor, used to record the classification.</param>
    /// <param name="correlationId">Optional request correlation id, when available at this layer.</param>
    public static Exception Classify(SqlException ex, string procedureName, ILogger logger, Guid correlationId = default)
    {
        switch (ex.Number)
        {
            case DuplicateKeyErrorNumber:
                logger.LogWarning(
                    ex,
                    "Stored procedure {ProcedureName} failed with duplicate-key violation (SQL error {ErrorNumber}); classified as ConflictException.",
                    procedureName, ex.Number);
                return new ConflictException(
                    new Error(
                        ErrorCode.Create("persistence.conflict"),
                        ErrorType.Conflict,
                        $"A conflicting record already exists for stored procedure '{procedureName}'."),
                    ex);

            case ConcurrencyConflictErrorNumber:
                logger.LogWarning(
                    ex,
                    "Stored procedure {ProcedureName} failed with optimistic concurrency conflict (SQL error {ErrorNumber}); classified as ConcurrencyConflictException.",
                    procedureName, ex.Number);
                return new ConcurrencyConflictException(
                    $"A concurrency conflict occurred while executing stored procedure '{procedureName}'. The record was modified by another process.",
                    correlationId);

            case NotFoundErrorNumber:
                logger.LogWarning(
                    ex,
                    "Stored procedure {ProcedureName} reported the target record was not found or has been deleted (SQL error {ErrorNumber}); classified as NotFoundException.",
                    procedureName, ex.Number);
                return new NotFoundException(
                    new Error(
                        ErrorCode.Create("persistence.notfound"),
                        ErrorType.NotFound,
                        $"The record targeted by stored procedure '{procedureName}' was not found or has been deleted."),
                    ex);

            case MaskedTransactionErrorNumber:
                logger.LogError(
                    ex,
                    "Stored procedure {ProcedureName} failed with SQL error {ErrorNumber} (\"the current transaction " +
                    "cannot be committed and cannot support operations that write to the log file\"). This is a KNOWN " +
                    "symptom of the CATCH block's own ErrorLog INSERT failing and MASKING the real underlying error " +
                    "(see Phase17 170-fix-errorlog-masking-bug.sql). The true cause has been lost; do not treat SQL " +
                    "error 3930 as a distinct, retryable, or success condition — classified as PersistenceException.",
                    procedureName, ex.Number);
                return new PersistenceException(procedureName, string.Empty, ex);

            default:
                logger.LogError(
                    ex,
                    "Stored procedure {ProcedureName} failed with unclassified SQL error {ErrorNumber}; classified as PersistenceException.",
                    procedureName, ex.Number);
                return new PersistenceException(procedureName, string.Empty, ex);
        }
    }
}
