-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EffluentSample_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure backing IEffluentSampleRepository.AddAsync. Chain-of-custody is append-only: this is the only write path for sample rows -- there is no delete/mutate path once inserted. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EffluentSample_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @EffluentSampleId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @DischargePointId UNIQUEIDENTIFIER,
    @Parameter NVARCHAR(50),
    @MeasuredValue DECIMAL(18,4),
    @UnitOfMeasure NVARCHAR(50),
    @SampledAtUtc DATETIME2(3),
    @CollectedByUserId UNIQUEIDENTIFIER,
    @LabSourceSystem NVARCHAR(100),
    @ChainOfCustodyReference NVARCHAR(200),
    @CorrectionOfSampleId UNIQUEIDENTIFIER = NULL,
    @CorrectionReason NVARCHAR(500) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EnvironmentalHealth].[EffluentSample]
            ([EffluentSampleId], [TenantId], [SiteId], [Status], [DischargePointId], [Parameter], [MeasuredValue],
             [UnitOfMeasure], [SampledAtUtc], [CollectedByUserId], [LabSourceSystem], [ChainOfCustodyReference],
             [CorrectionOfSampleId], [CorrectionReason], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@EffluentSampleId, @TenantId, @SiteId, @Status, @DischargePointId, @Parameter, @MeasuredValue,
             @UnitOfMeasure, @SampledAtUtc, @CollectedByUserId, @LabSourceSystem, @ChainOfCustodyReference,
             @CorrectionOfSampleId, @CorrectionReason, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO