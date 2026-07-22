-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.LotoSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoSystems_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [MachineSafety].[LotoSystems], backing
--                  ILotoSystemsRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_LotoSystems_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @LotoSystemsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @SystemTitle NVARCHAR(400),
    @EquipmentTag NVARCHAR(400),
    @EnergySourceTypes NVARCHAR(100),
    @IsolationPointDescription NVARCHAR(2000),
    @IsolationPointCount INT,
    @VerifiedByUserId UNIQUEIDENTIFIER,
    @VerifiedZeroEnergyAtUtc DATETIME2(3) = NULL,
    @AuditedAtUtc DATETIME2(3) = NULL,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [MachineSafety].[LotoSystems]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [SystemTitle] = @SystemTitle,
        [EquipmentTag] = @EquipmentTag,
        [EnergySourceTypes] = @EnergySourceTypes,
        [IsolationPointDescription] = @IsolationPointDescription,
        [IsolationPointCount] = @IsolationPointCount,
        [VerifiedByUserId] = @VerifiedByUserId,
        [VerifiedZeroEnergyAtUtc] = @VerifiedZeroEnergyAtUtc,
        [AuditedAtUtc] = @AuditedAtUtc,
        [ComplianceScore] = @ComplianceScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [LotoSystemsId] = @LotoSystemsId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
