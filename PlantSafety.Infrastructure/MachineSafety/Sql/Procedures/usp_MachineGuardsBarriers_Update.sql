-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.MachineGuardsBarriers
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuardsBarriers_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [MachineSafety].[MachineGuardsBarriers], backing
--                  IMachineGuardsBarriersRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_MachineGuardsBarriers_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @MachineGuardsBarriersId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @GuardTitle NVARCHAR(400),
    @GuardType NVARCHAR(100),
    @EquipmentLocation NVARCHAR(400),
    @InstalledByUserId UNIQUEIDENTIFIER,
    @InspectedAtUtc DATETIME2(3) = NULL,
    @NextInspectionDueAtUtc DATETIME2(3) = NULL,
    @DeficiencyCount INT,
    @GuardIntegrityScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [MachineSafety].[MachineGuardsBarriers]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [GuardTitle] = @GuardTitle,
        [GuardType] = @GuardType,
        [EquipmentLocation] = @EquipmentLocation,
        [InstalledByUserId] = @InstalledByUserId,
        [InspectedAtUtc] = @InspectedAtUtc,
        [NextInspectionDueAtUtc] = @NextInspectionDueAtUtc,
        [DeficiencyCount] = @DeficiencyCount,
        [GuardIntegrityScore] = @GuardIntegrityScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [MachineGuardsBarriersId] = @MachineGuardsBarriersId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
