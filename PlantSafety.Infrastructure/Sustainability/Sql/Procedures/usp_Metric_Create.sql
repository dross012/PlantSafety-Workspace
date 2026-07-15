-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Sustainability
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Metric_Create.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Records a new sustainability metric observation. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Sustainability].[usp_Metric_Create]
(
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
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @Status NVARCHAR(50) =
            CASE
                WHEN @Target IS NULL THEN 'OnTrack'
                WHEN @Value <= @Target THEN 'OnTrack'
                WHEN @Value <= @Target * 1.1 THEN 'AtRisk'
                ELSE 'Exceeded'
            END;

        INSERT INTO [Sustainability].[Metric]
            ([MetricId], [TenantId], [SiteId], [MetricName], [Category], [Value], [Unit],
             [Target], [PeriodEndUtc], [Status], [CreatedAtUtc], [CorrelationId], [ActorId])
        VALUES
            (@MetricId, @TenantId, @SiteId, @MetricName, @Category, @Value, @Unit,
             @Target, @PeriodEndUtc, @Status, @NowUtc, @CorrelationId, @ActorId);

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
