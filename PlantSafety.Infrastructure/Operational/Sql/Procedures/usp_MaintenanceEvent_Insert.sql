-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_MaintenanceEvent_Insert.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Appends one row to [Operational].[MaintenanceEvents]. Called by
--                  WorkOrderDapperRepository.AddAsync/UpdateAsync, once per domain event raised by
--                  the WorkOrder aggregate during that unit of work, so GetWorkOrderHistoryQuery and
--                  GetMachineHistoryQuery are backed by genuine data. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_MaintenanceEvent_Insert]
    @EventId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @WorkOrderId UNIQUEIDENTIFIER,
    @WorkOrderCode NVARCHAR(50),
    @MachineId UNIQUEIDENTIFIER,
    @OccurredAtUtc DATETIME2(3),
    @TechnicianName NVARCHAR(200) = NULL,
    @Summary NVARCHAR(500)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Operational].[MaintenanceEvents]
        (EventId, TenantId, WorkOrderId, WorkOrderCode, MachineId, OccurredAtUtc, TechnicianName, Summary)
        VALUES
        (@EventId, @TenantId, @WorkOrderId, @WorkOrderCode, @MachineId, @OccurredAtUtc, @TechnicianName, @Summary);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkOrderId=', @WorkOrderId, N'; EventId=', @EventId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
