-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_GetById.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single WorkOrder aggregate root row by identity, tenant-scoped.
--                  Backs IWorkOrderRepository.GetByIdAsync (aggregate hydration) and
--                  GetWorkOrderByIdQuery (Application-layer read via WorkOrderReadDapperRepository).
--                  Projects RowVersion as a string ETag. Mirrors usp_PreventiveMaintenanceTask_GetById.sql.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_GetById]
    @WorkOrderId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            WorkOrderId, TenantId, SiteId, Status, WorkOrderCode, MachineId, MachineCode, Priority, Description, DueAtUtc,
            TechnicianId, TechnicianName, AssignedAtUtc, StartedAtUtc, OnHoldAtUtc,
            CompletedBy, CompletionNotes, CompletedAtUtc, CancelledBy, CancelledReason, CancelledAtUtc,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Operational].[WorkOrders]
        WHERE WorkOrderId = @WorkOrderId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkOrderId=', @WorkOrderId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
