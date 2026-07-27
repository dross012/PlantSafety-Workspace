-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Moc_Approve.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing IMocRepository.ApproveAsync. ETag-guarded
--                  (ROWVERSION) optimistic-concurrency update; ends with an explicit
--                  SELECT @@ROWCOUNT so the caller can detect a stale/incorrect ETag via
--                  DapperCommandExecutor.ExecuteScalarAsync (SET NOCOUNT ON means
--                  ExecuteNonQuery/ExecuteAsync would otherwise always report -1, per the
--                  platform-wide rowcount-concurrency pattern documented on
--                  DapperCommandExecutor.ExecuteScalarAsync).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Moc_Approve]
    @TenantId       UNIQUEIDENTIFIER,
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2(3),
    @MocId          UNIQUEIDENTIFIER,
    @ApprovalNotes  NVARCHAR(2000) = NULL,
    @ETag           NVARCHAR(64)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        UPDATE [Compliance].[Moc]
        SET
            Status = N'Approved',
            ApprovalNotes = @ApprovalNotes,
            ApprovedByActorId = @ActorId,
            ApprovedAtUtc = @NowUtc,
            LastModifiedAtUtc = @NowUtc
        WHERE MocId = @MocId
          AND TenantId = @TenantId
          AND IsDeleted = 0
          AND CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag;

        DECLARE @RowsAffected INT = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
