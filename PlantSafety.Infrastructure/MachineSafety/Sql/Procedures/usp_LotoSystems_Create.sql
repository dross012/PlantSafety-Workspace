-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.LotoSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoSystems_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [MachineSafety].[LotoSystems], backing
--                  ILotoSystemsRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_LotoSystems_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [MachineSafety].[LotoSystems]
            ([LotoSystemsId], [TenantId], [SiteId], [Status], [SystemTitle], [EquipmentTag], [EnergySourceTypes], [IsolationPointDescription], [IsolationPointCount], [VerifiedByUserId], [VerifiedZeroEnergyAtUtc], [AuditedAtUtc], [ComplianceScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@LotoSystemsId, @TenantId, @SiteId, @Status, @SystemTitle, @EquipmentTag, @EnergySourceTypes, @IsolationPointDescription, @IsolationPointCount, @VerifiedByUserId, @VerifiedZeroEnergyAtUtc, @AuditedAtUtc, @ComplianceScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
