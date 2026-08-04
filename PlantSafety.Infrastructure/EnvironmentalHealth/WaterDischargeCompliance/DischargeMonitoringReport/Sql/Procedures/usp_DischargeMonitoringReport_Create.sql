-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargeMonitoringReport_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure backing IDischargeMonitoringReportRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeMonitoringReport_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @DischargeMonitoringReportId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @DischargePermitId UNIQUEIDENTIFIER,
    @ReportingPeriodStartUtc DATETIME2(3),
    @ReportingPeriodEndUtc DATETIME2(3),
    @DueAtUtc DATETIME2(3),
    @SamplesWithinLimitCount INT,
    @TotalSamplesCount INT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EnvironmentalHealth].[DischargeMonitoringReport]
            ([DischargeMonitoringReportId], [TenantId], [SiteId], [Status], [DischargePermitId], [ReportingPeriodStartUtc],
             [ReportingPeriodEndUtc], [DueAtUtc], [SamplesWithinLimitCount], [TotalSamplesCount],
             [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@DischargeMonitoringReportId, @TenantId, @SiteId, @Status, @DischargePermitId, @ReportingPeriodStartUtc,
             @ReportingPeriodEndUtc, @DueAtUtc, @SamplesWithinLimitCount, @TotalSamplesCount,
             @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO