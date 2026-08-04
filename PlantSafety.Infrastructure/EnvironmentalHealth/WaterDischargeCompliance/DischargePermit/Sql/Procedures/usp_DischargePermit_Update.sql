-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargePermit_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure backing IDischargePermitRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePermit_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @DischargePermitId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @PermitNumber NVARCHAR(100),
    @RegulatoryProgram NVARCHAR(50),
    @IssuingAuthority NVARCHAR(200),
    @EffectiveAtUtc DATETIME2(3),
    @ExpiresAtUtc DATETIME2(3),
    @RenewalCount INT,
    @PermitConditionSummary NVARCHAR(1000),
    @LifecycleActionReason NVARCHAR(500) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EnvironmentalHealth].[DischargePermit]
        SET
            [SiteId] = @SiteId,
            [Status] = @Status,
            [ExpiresAtUtc] = @ExpiresAtUtc,
            [RenewalCount] = @RenewalCount,
            [LifecycleActionReason] = @LifecycleActionReason,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [DischargePermitId] = @DischargePermitId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO