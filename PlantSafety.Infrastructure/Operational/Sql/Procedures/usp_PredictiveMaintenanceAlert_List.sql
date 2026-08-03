-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PredictiveMaintenance
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PredictiveMaintenanceAlert_List.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Paginated, optionally equipment/active-filtered list of
--                  PredictiveMaintenanceAlert rows, tenant-scoped. Backs
--                  IPredictiveMaintenanceAlertRepository.GetPagedAsync, which in turn backs the
--                  Predictive Maintenance Insights sub-resources (risk-summary, equipment-health,
--                  predictions list, equipment predictive view). Projects a windowed TotalCount
--                  (COUNT(*) OVER()) alongside every row so the Application layer can build the
--                  canonical PaginatedResult<T> envelope without a second round-trip. Canonical
--                  TRY/CATCH + ErrorDB logging (no AuditLog for reads). Mirrors
--                  usp_PredictiveMaintenanceAlert_GetById.sql. Additive-only evolution -- the
--                  pre-existing usp_PredictiveMaintenance_List.sql (date-range list backing the
--                  original GetPredictiveMaintenanceQuery) is left completely untouched.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_PredictiveMaintenanceAlert_List]
    @TenantId    UNIQUEIDENTIFIER,
    @EquipmentId UNIQUEIDENTIFIER = NULL,
    @ActiveOnly  BIT = 0,
    @PageNumber  INT = 1,
    @PageSize    INT = 25
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        DECLARE @Offset INT = (CASE WHEN @PageNumber < 1 THEN 0 ELSE @PageNumber - 1 END) * @PageSize;

        SELECT
            PredictiveMaintenanceAlertId, TenantId, SiteId, Status, EquipmentId, PredictedFailureType, PredictedAtUtc,
            ReviewedBy, ReviewedAtUtc, ActionScheduledBy, WorkOrderReference, ActionScheduledAtUtc,
            ResolvedBy, ResolutionNotes, ResolvedAtUtc, DismissedBy, DismissedReason, DismissedAtUtc,
            ClosedBy, ClosedAtUtc, CreatedAtUtc, CreatedBy,
            COUNT(*) OVER() AS TotalCount
        FROM [Operational].[PredictiveMaintenanceReadModel]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@EquipmentId IS NULL OR EquipmentId = @EquipmentId)
          AND (@ActiveOnly = 0 OR Status NOT IN ('Resolved', 'Dismissed', 'Closed'))
        ORDER BY PredictedAtUtc DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; EquipmentId=', @EquipmentId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
