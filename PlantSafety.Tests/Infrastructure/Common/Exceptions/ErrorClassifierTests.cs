// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Infrastructure.Common.Exceptions
//  Layer:          Tests (Unit)
//  File:           ErrorClassifierTests.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ErrorClassifier, covering all five SqlException.Number
//                  branches (2627 duplicate key, 50002 concurrency conflict, 50004 not
//                  found, 3930 masked transaction error, and the unclassified default).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using System.Reflection;
using Microsoft.Data.SqlClient;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Infrastructure.Common.Exceptions;
using PlantSafety.SharedKernel.Exceptions;

namespace PlantSafety.Tests.Infrastructure.Common.Exceptions;

public sealed class ErrorClassifierTests : UnitTestBase
{
    private const string ProcedureName = "[SafetyEvents].[usp_TestProc]";

    [Fact]
    public void Classify_DuplicateKeyError2627_ReturnsConflictException()
    {
        var sqlEx = CreateSqlException(2627, "Violation of UNIQUE KEY constraint.");
        var logger = new FakeLogger<ErrorClassifierTests>();

        var result = ErrorClassifier.Classify(sqlEx, ProcedureName, logger);

        result.Should().BeOfType<ConflictException>();
        result.InnerException.Should().BeSameAs(sqlEx);
        logger.ShouldHaveLogged(LogLevel.Warning);
    }

    [Fact]
    public void Classify_ConcurrencyConflictError50002_ReturnsConcurrencyConflictException()
    {
        var sqlEx = CreateSqlException(50002, "Concurrency conflict detected.");
        var logger = new FakeLogger<ErrorClassifierTests>();

        var result = ErrorClassifier.Classify(sqlEx, ProcedureName, logger);

        result.Should().BeOfType<ConcurrencyConflictException>();
        logger.ShouldHaveLogged(LogLevel.Warning);
    }

    [Fact]
    public void Classify_NotFoundError50004_ReturnsNotFoundException()
    {
        var sqlEx = CreateSqlException(50004, "Record not found or has been deleted.");
        var logger = new FakeLogger<ErrorClassifierTests>();

        var result = ErrorClassifier.Classify(sqlEx, ProcedureName, logger);

        result.Should().BeOfType<NotFoundException>();
        result.InnerException.Should().BeSameAs(sqlEx);
        logger.ShouldHaveLogged(LogLevel.Warning);
    }

    [Fact]
    public void Classify_MaskedTransactionError3930_ReturnsPersistenceExceptionAndLogsError()
    {
        var sqlEx = CreateSqlException(3930, "The current transaction cannot be committed...");
        var logger = new FakeLogger<ErrorClassifierTests>();

        var result = ErrorClassifier.Classify(sqlEx, ProcedureName, logger);

        result.Should().BeOfType<PersistenceException>();
        result.InnerException.Should().BeSameAs(sqlEx);
        logger.ShouldHaveLogged(LogLevel.Error);

        // Must never be mistaken for a distinct/retryable condition — the log must explain
        // that the true cause was lost, per the Phase17 masking-bug documentation.
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Error &&
            e.Message.Contains("MASKING", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Classify_UnknownErrorNumber_ReturnsPersistenceExceptionAndLogsError()
    {
        var sqlEx = CreateSqlException(999999, "Some unrelated SQL failure.");
        var logger = new FakeLogger<ErrorClassifierTests>();

        var result = ErrorClassifier.Classify(sqlEx, ProcedureName, logger);

        result.Should().BeOfType<PersistenceException>();
        result.InnerException.Should().BeSameAs(sqlEx);
        logger.ShouldHaveLogged(LogLevel.Error);
    }

    /// <summary>
    /// Builds a real <see cref="SqlException"/> instance with the given <c>Number</c> via
    /// reflection. <see cref="SqlException"/> has no public constructor; this mirrors the
    /// reflection-based construction pattern Microsoft.Data.SqlClient requires for testing
    /// error-classification logic without a live SQL Server connection.
    /// </summary>
    private static SqlException CreateSqlException(int number, string message)
    {
        var assembly = typeof(SqlException).Assembly;

        var errorType = assembly.GetType("Microsoft.Data.SqlClient.SqlError", throwOnError: true)!;
        var errorCtor = errorType.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types:
            [
                typeof(int), typeof(byte), typeof(byte), typeof(string),
                typeof(string), typeof(string), typeof(int), typeof(Exception)
            ],
            modifiers: null)!;
        var sqlError = errorCtor.Invoke(
            [number, (byte)0, (byte)0, "TestServer", message, ProcedureName, 1, null]);

        var collectionType = assembly.GetType("Microsoft.Data.SqlClient.SqlErrorCollection", throwOnError: true)!;
        var collection = Activator.CreateInstance(collectionType, nonPublic: true)!;
        collectionType
            .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(collection, [sqlError]);

        var createException = typeof(SqlException).GetMethod(
            "CreateException",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types: [collectionType, typeof(string)],
            modifiers: null)!;

        return (SqlException)createException.Invoke(null, [collection, "11.0.0"])!;
    }
}
