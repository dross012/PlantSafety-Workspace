-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Permit_Approve.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing IPermitRepository.ApproveAsync. ETag-guarded
--                  (ROWVERSION) optimistic-concurrency update, transitions Draft/PendingApproval
--                  to Active. Ends with an explicit SELECT @@ROWCOUNT so the caller can detect a
--                  stale/incorrect ETag via DapperCommandExecutor.ExecuteScalarAsync (SET NOCOUNT
--                  ON means ExecuteNonQuery/ExecuteAsync would otherwise always report -1).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Permit_Approve]
    @TenantId       UNIQUEIDENTIFIER,
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2(3),
    @PermitId       UNIQUEIDENTIFIER,
    @ApprovalNotes  NVARCHAR(2000) = NULL,
    @ETag           NVARCHAR(64)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        UPDATE [Compliance].[Permit]
        SET
            Status = N'Active',
            ApprovalNotes = @ApprovalNotes,
            ApprovedByActorId = @ActorId,
            ApprovedAtUtc = @NowUtc,
            LastModifiedAtUtc = @NowUtc
        WHERE PermitId = @PermitId
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
