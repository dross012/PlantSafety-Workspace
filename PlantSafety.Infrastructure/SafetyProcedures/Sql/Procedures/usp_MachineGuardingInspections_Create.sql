-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.MachineGuardingInspections
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuardingInspections_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyProcedures].[MachineGuardingInspections], backing
--                  IMachineGuardingInspectionsRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_MachineGuardingInspections_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [SafetyProcedures].[MachineGuardingInspections]
            ([MachineGuardingInspectionsId], [TenantId], [SiteId], [Status], [InspectionTitle], [InspectionCategory], [EquipmentLocation], [InspectedByUserId], [InspectedAtUtc], [NextInspectionDueAtUtc], [DeficiencyCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@MachineGuardingInspectionsId, @TenantId, @SiteId, @Status, @InspectionTitle, @InspectionCategory, @EquipmentLocation, @InspectedByUserId, @InspectedAtUtc, @NextInspectionDueAtUtc, @DeficiencyCount, @ComplianceScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
