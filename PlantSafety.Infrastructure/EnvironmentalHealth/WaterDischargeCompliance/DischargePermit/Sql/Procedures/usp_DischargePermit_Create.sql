-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargePermit_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure backing IDischargePermitRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePermit_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
    @LifecycleActionReason NVARCHAR(500) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EnvironmentalHealth].[DischargePermit]
            ([DischargePermitId], [TenantId], [SiteId], [Status], [PermitNumber], [RegulatoryProgram], [IssuingAuthority],
             [EffectiveAtUtc], [ExpiresAtUtc], [RenewalCount], [PermitConditionSummary], [LifecycleActionReason],
             [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@DischargePermitId, @TenantId, @SiteId, @Status, @PermitNumber, @RegulatoryProgram, @IssuingAuthority,
             @EffectiveAtUtc, @ExpiresAtUtc, @RenewalCount, @PermitConditionSummary, @LifecycleActionReason,
             @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO