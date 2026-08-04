-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LimitExceedance_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure backing ILimitExceedanceRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_LimitExceedance_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @LimitExceedanceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @DischargePointId UNIQUEIDENTIFIER,
    @EffluentSampleId UNIQUEIDENTIFIER,
    @DischargeLimitId UNIQUEIDENTIFIER,
    @ParameterName NVARCHAR(100),
    @MeasuredValue DECIMAL(18,4),
    @LimitValue DECIMAL(18,4),
    @ExceedancePercentage DECIMAL(9,2),
    @Severity NVARCHAR(50),
    @DetectedAtUtc DATETIME2(3),
    @RepeatExceedanceCount INT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EnvironmentalHealth].[LimitExceedance]
            ([LimitExceedanceId], [TenantId], [SiteId], [Status], [DischargePointId], [EffluentSampleId], [DischargeLimitId],
             [ParameterName], [MeasuredValue], [LimitValue], [ExceedancePercentage], [Severity], [DetectedAtUtc],
             [RepeatExceedanceCount], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@LimitExceedanceId, @TenantId, @SiteId, @Status, @DischargePointId, @EffluentSampleId, @DischargeLimitId,
             @ParameterName, @MeasuredValue, @LimitValue, @ExceedancePercentage, @Severity, @DetectedAtUtc,
             @RepeatExceedanceCount, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO