-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_Insert.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Inserts a new WorkOrder aggregate root row. Backs IWorkOrderRepository.AddAsync.
--                  Mirrors usp_PreventiveMaintenanceTask_Insert.sql's TRY/CATCH + AuditDB/ErrorDB
--                  logging template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_Insert]
    @WorkOrderId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @WorkOrderCode NVARCHAR(50),
    @MachineId UNIQUEIDENTIFIER,
    @MachineCode NVARCHAR(50),
    @Priority NVARCHAR(20),
    @Description NVARCHAR(2000),
    @DueAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Operational].[WorkOrders]
        (
            WorkOrderId, TenantId, SiteId, Status, WorkOrderCode, MachineId, MachineCode, Priority, Description, DueAtUtc,
            CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @WorkOrderId, @TenantId, @SiteId, @Status, @WorkOrderCode, @MachineId, @MachineCode, @Priority, @Description, @DueAtUtc,
            @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'WorkOrders', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'WorkOrderId=', @WorkOrderId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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
