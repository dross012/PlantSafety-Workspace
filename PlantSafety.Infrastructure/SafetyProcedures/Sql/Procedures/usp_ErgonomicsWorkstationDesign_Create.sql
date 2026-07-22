-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ErgonomicsWorkstationDesign_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyProcedures].[ErgonomicsWorkstationDesign], backing
--                  IErgonomicsWorkstationDesignRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_ErgonomicsWorkstationDesign_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @ErgonomicsWorkstationDesignId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ProcedureTitle NVARCHAR(400),
    @DesignCategory NVARCHAR(100),
    @WorkstationLocation NVARCHAR(400),
    @ApprovedByUserId UNIQUEIDENTIFIER,
    @AssessedAtUtc DATETIME2(3) = NULL,
    @NextAssessmentDueAtUtc DATETIME2(3) = NULL,
    @RiskFactorCount INT,
    @ErgonomicsScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [SafetyProcedures].[ErgonomicsWorkstationDesign]
            ([ErgonomicsWorkstationDesignId], [TenantId], [SiteId], [Status], [ProcedureTitle], [DesignCategory], [WorkstationLocation], [ApprovedByUserId], [AssessedAtUtc], [NextAssessmentDueAtUtc], [RiskFactorCount], [ErgonomicsScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@ErgonomicsWorkstationDesignId, @TenantId, @SiteId, @Status, @ProcedureTitle, @DesignCategory, @WorkstationLocation, @ApprovedByUserId, @AssessedAtUtc, @NextAssessmentDueAtUtc, @RiskFactorCount, @ErgonomicsScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
