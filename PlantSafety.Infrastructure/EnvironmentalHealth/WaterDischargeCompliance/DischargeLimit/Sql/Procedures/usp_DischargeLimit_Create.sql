-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargeLimit_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure backing IDischargeLimitRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeLimit_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @DischargeLimitId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @DischargePointId UNIQUEIDENTIFIER,
    @DischargePermitId UNIQUEIDENTIFIER,
    @Parameter NVARCHAR(50),
    @LimitBasis NVARCHAR(50),
    @LimitValue DECIMAL(18,4),
    @UnitOfMeasure NVARCHAR(50),
    @EffectiveAtUtc DATETIME2(3),
    @PermitConditionReference NVARCHAR(200),
    @RevisionCount INT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EnvironmentalHealth].[DischargeLimit]
            ([DischargeLimitId], [TenantId], [SiteId], [Status], [DischargePointId], [DischargePermitId], [Parameter],
             [LimitBasis], [LimitValue], [UnitOfMeasure], [EffectiveAtUtc], [PermitConditionReference], [RevisionCount],
             [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@DischargeLimitId, @TenantId, @SiteId, @Status, @DischargePointId, @DischargePermitId, @Parameter,
             @LimitBasis, @LimitValue, @UnitOfMeasure, @EffectiveAtUtc, @PermitConditionReference, @RevisionCount,
             @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO