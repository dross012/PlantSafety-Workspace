-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PredictiveMaintenance
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PredictiveMaintenanceAlert_Update.sql
--  Created:        2026-07-30
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PredictiveMaintenanceAlert aggregate
--                  root row. Backs IPredictiveMaintenanceAlertRepository.UpdateAsync. @TenantId is
--                  part of the WHERE clause from day one (tenant-scoped write) and @ETag enforces
--                  optimistic concurrency against the native RowVersion column from day one,
--                  mirroring usp_Permits_Update.sql. @RowsAffected is captured via @@ROWCOUNT
--                  immediately after the UPDATE (before the audit INSERT, which would otherwise
--                  reset it) and returned via SELECT -- callers MUST invoke this proc via
--                  ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON
--                  makes ExecuteNonQuery always return -1, which would silently disable the @ETag
--                  concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_PredictiveMaintenanceAlert_Update]
    @PredictiveMaintenanceAlertId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @EquipmentId UNIQUEIDENTIFIER,
    @PredictedFailureType NVARCHAR(400),
    @PredictedAtUtc DATETIME2(3),
    @ReviewedBy UNIQUEIDENTIFIER = NULL,
    @ReviewedAtUtc DATETIME2(3) = NULL,
    @ActionScheduledBy UNIQUEIDENTIFIER = NULL,
    @WorkOrderReference NVARCHAR(200) = NULL,
    @ActionScheduledAtUtc DATETIME2(3) = NULL,
    @ResolvedBy UNIQUEIDENTIFIER = NULL,
    @ResolutionNotes NVARCHAR(2000) = NULL,
    @ResolvedAtUtc DATETIME2(3) = NULL,
    @DismissedBy UNIQUEIDENTIFIER = NULL,
    @DismissedReason NVARCHAR(2000) = NULL,
    @DismissedAtUtc DATETIME2(3) = NULL,
    @ClosedBy UNIQUEIDENTIFIER = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Operational].[PredictiveMaintenanceReadModel] WHERE PredictiveMaintenanceAlertId = @PredictiveMaintenanceAlertId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'PredictiveMaintenanceAlert not found or has been deleted.', 1;

        UPDATE [Operational].[PredictiveMaintenanceReadModel]
        SET
            Status = @Status,
            EquipmentId = @EquipmentId,
            PredictedFailureType = @PredictedFailureType,
            PredictedAtUtc = @PredictedAtUtc,
            ReviewedBy = @ReviewedBy,
            ReviewedAtUtc = @ReviewedAtUtc,
            ActionScheduledBy = @ActionScheduledBy,
            WorkOrderReference = @WorkOrderReference,
            ActionScheduledAtUtc = @ActionScheduledAtUtc,
            ResolvedBy = @ResolvedBy,
            ResolutionNotes = @ResolutionNotes,
            ResolvedAtUtc = @ResolvedAtUtc,
            DismissedBy = @DismissedBy,
            DismissedReason = @DismissedReason,
            DismissedAtUtc = @DismissedAtUtc,
            ClosedBy = @ClosedBy,
            ClosedAtUtc = @ClosedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE PredictiveMaintenanceAlertId = @PredictiveMaintenanceAlertId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PredictiveMaintenanceReadModel', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PredictiveMaintenanceAlertId=', @PredictiveMaintenanceAlertId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictiveMaintenanceAlertId=', @PredictiveMaintenanceAlertId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
