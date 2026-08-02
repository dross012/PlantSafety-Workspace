-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EnvironmentalHealthAggregate_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single EnvironmentalHealth aggregate root row by identity,
--                  tenant-scoped from day one. Backs IEnvironmentalHealthRepository.GetByIdAsync
--                  (aggregate hydration) and the Application-layer
--                  GetEnvironmentalHealthByIdQuery read path. Canonical TRY/CATCH + ErrorDB
--                  logging (no AuditLog for reads). Projects RowVersion as a string ETag.
--                  Mirrors usp_AuditingAggregate_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EnvironmentalHealthAggregate_GetById]
    @EnvironmentalHealthId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            EnvironmentalHealthId, TenantId, SiteId, Status, MonitoringType, MeasuredValue, Unit,
            Threshold, ThresholdExceeded, EvaluatedStatus, RecordedAtUtc,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [EnvironmentalHealth].[EnvironmentalHealthAggregate]
        WHERE EnvironmentalHealthId = @EnvironmentalHealthId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
