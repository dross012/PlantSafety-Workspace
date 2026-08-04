-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CarbonEmissionRecord_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Inserts a new CarbonEmissionRecord child row. Backs
--                  RecordCarbonEmissionCommandHandler via AdvancedMetricsLedgerDapperRepository.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_CarbonEmissionRecord_Insert]
    @CarbonEmissionRecordId UNIQUEIDENTIFIER,
    @AdvancedMetricsLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Scope NVARCHAR(20),
    @ActivityDescription NVARCHAR(400),
    @EmissionsMetricTonsCo2e DECIMAL(18,6),
    @MethodologyVersion NVARCHAR(100),
    @ActivityAtUtc DATETIME2(3),
    @RecordedAtUtc DATETIME2(3),
    @RecordedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        -- Idempotent by design: repository re-submits the aggregate's full in-memory child
        -- collection on every Add/Update call, so a duplicate CarbonEmissionRecordId is a no-op
        -- rather than a PK-violation error.
        IF NOT EXISTS (SELECT 1 FROM [SustainabilityAdvancedMetrics].[CarbonEmissionRecord] WHERE CarbonEmissionRecordId = @CarbonEmissionRecordId)
        INSERT INTO [SustainabilityAdvancedMetrics].[CarbonEmissionRecord]
        (CarbonEmissionRecordId, AdvancedMetricsLedgerId, TenantId, Scope, ActivityDescription, EmissionsMetricTonsCo2e, MethodologyVersion, ActivityAtUtc, RecordedAtUtc, RecordedBy)
        VALUES
        (@CarbonEmissionRecordId, @AdvancedMetricsLedgerId, @TenantId, @Scope, @ActivityDescription, @EmissionsMetricTonsCo2e, @MethodologyVersion, @ActivityAtUtc, @RecordedAtUtc, @RecordedBy);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CarbonEmissionRecordId=', @CarbonEmissionRecordId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
