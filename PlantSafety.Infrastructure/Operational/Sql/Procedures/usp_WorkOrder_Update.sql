-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_Update.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a WorkOrder aggregate root row. Backs
--                  IWorkOrderRepository.UpdateAsync. @TenantId is part of the WHERE clause and
--                  @ETag enforces optimistic concurrency against the native RowVersion column.
--                  Callers MUST invoke this proc via ExecuteScalarAsync (not ExecuteNonQuery), since
--                  SET NOCOUNT ON makes ExecuteNonQuery always return -1. Mirrors
--                  usp_PreventiveMaintenanceTask_Update.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_Update]
    @WorkOrderId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @TechnicianId UNIQUEIDENTIFIER = NULL,
    @TechnicianName NVARCHAR(200) = NULL,
    @AssignedAtUtc DATETIME2(3) = NULL,
    @StartedAtUtc DATETIME2(3) = NULL,
    @OnHoldAtUtc DATETIME2(3) = NULL,
    @CompletedBy UNIQUEIDENTIFIER = NULL,
    @CompletionNotes NVARCHAR(2000) = NULL,
    @CompletedAtUtc DATETIME2(3) = NULL,
    @CancelledBy UNIQUEIDENTIFIER = NULL,
    @CancelledReason NVARCHAR(2000) = NULL,
    @CancelledAtUtc DATETIME2(3) = NULL,
    @UpdatedAtUtc DATETIME2(3),
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Operational].[WorkOrders] WHERE WorkOrderId = @WorkOrderId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'WorkOrder not found or has been deleted.', 1;

        UPDATE [Operational].[WorkOrders]
        SET
            Status = @Status,
            TechnicianId = @TechnicianId,
            TechnicianName = @TechnicianName,
            AssignedAtUtc = @AssignedAtUtc,
            StartedAtUtc = @StartedAtUtc,
            OnHoldAtUtc = @OnHoldAtUtc,
            CompletedBy = @CompletedBy,
            CompletionNotes = @CompletionNotes,
            CompletedAtUtc = @CompletedAtUtc,
            CancelledBy = @CancelledBy,
            CancelledReason = @CancelledReason,
            CancelledAtUtc = @CancelledAtUtc,
            UpdatedAtUtc = @UpdatedAtUtc
        WHERE WorkOrderId = @WorkOrderId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'WorkOrders', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'WorkOrderId=', @WorkOrderId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkOrderId=', @WorkOrderId, N'; User=', SYSTEM_USER);
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
