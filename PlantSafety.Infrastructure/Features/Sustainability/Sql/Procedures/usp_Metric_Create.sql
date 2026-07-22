-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Sustainability
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Metric_Create.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Records a new sustainability metric observation. Backs
--                  ISustainabilityRepository.CreateAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Sustainability].[usp_Metric_Create]
    @TenantId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2(3),
    @MetricId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @MetricName    NVARCHAR(200),
    @Category      NVARCHAR(50),
    @Value         DECIMAL(18,4),
    @Unit          NVARCHAR(20),
    @Target        DECIMAL(18,4) = NULL,
    @PeriodEndUtc  DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        DECLARE @Status NVARCHAR(50) = CASE
            WHEN @Target IS NULL THEN N'Recorded'
            WHEN @Value <= @Target THEN N'OnTarget'
            ELSE N'AboveTarget'
        END;

        INSERT INTO [Sustainability].[Metric]
            (MetricId, TenantId, SiteId, MetricName, Category, Value, Unit, Target, PeriodEndUtc, Status,
             IsDeleted, CorrelationId, CreatedAtUtc, CreatedBy)
        VALUES
            (@MetricId, @TenantId, @SiteId, @MetricName, @Category, @Value, @Unit, @Target, @PeriodEndUtc, @Status,
             0, @CorrelationId, @NowUtc, @ActorId);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; MetricId=', @MetricId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
