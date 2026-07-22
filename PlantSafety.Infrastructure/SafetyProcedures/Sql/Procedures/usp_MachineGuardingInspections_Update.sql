-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.MachineGuardingInspections
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuardingInspections_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [SafetyProcedures].[MachineGuardingInspections], backing
--                  IMachineGuardingInspectionsRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_MachineGuardingInspections_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @MachineGuardingInspectionsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @InspectionTitle NVARCHAR(400),
    @InspectionCategory NVARCHAR(100),
    @EquipmentLocation NVARCHAR(400),
    @InspectedByUserId UNIQUEIDENTIFIER,
    @InspectedAtUtc DATETIME2(3) = NULL,
    @NextInspectionDueAtUtc DATETIME2(3) = NULL,
    @DeficiencyCount INT,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyProcedures].[MachineGuardingInspections]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [InspectionTitle] = @InspectionTitle,
        [InspectionCategory] = @InspectionCategory,
        [EquipmentLocation] = @EquipmentLocation,
        [InspectedByUserId] = @InspectedByUserId,
        [InspectedAtUtc] = @InspectedAtUtc,
        [NextInspectionDueAtUtc] = @NextInspectionDueAtUtc,
        [DeficiencyCount] = @DeficiencyCount,
        [ComplianceScore] = @ComplianceScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [MachineGuardingInspectionsId] = @MachineGuardingInspectionsId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
