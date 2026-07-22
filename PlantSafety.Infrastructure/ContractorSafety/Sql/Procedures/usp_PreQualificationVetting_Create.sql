-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PreQualificationVetting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PreQualificationVetting_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [ContractorSafety].[PreQualificationVetting], backing
--                  IPreQualificationVettingRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_PreQualificationVetting_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @PreQualificationVettingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @VettingTitle NVARCHAR(400),
    @ContractorName NVARCHAR(400),
    @EmrScore DECIMAL(9,2) = NULL,
    @TrirScore DECIMAL(9,2) = NULL,
    @OshaTrainingCompliant BIT,
    @DeficiencyCount INT,
    @InsuranceExpiresAtUtc DATETIME2(3) = NULL,
    @ReviewedByUserId UNIQUEIDENTIFIER,
    @VettedAtUtc DATETIME2(3) = NULL,
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @RejectionReason NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [ContractorSafety].[PreQualificationVetting]
            ([PreQualificationVettingId], [TenantId], [SiteId], [Status], [VettingTitle], [ContractorName], [EmrScore], [TrirScore], [OshaTrainingCompliant], [DeficiencyCount], [InsuranceExpiresAtUtc], [ReviewedByUserId], [VettedAtUtc], [ExpiresAtUtc], [RejectionReason], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@PreQualificationVettingId, @TenantId, @SiteId, @Status, @VettingTitle, @ContractorName, @EmrScore, @TrirScore, @OshaTrainingCompliant, @DeficiencyCount, @InsuranceExpiresAtUtc, @ReviewedByUserId, @VettedAtUtc, @ExpiresAtUtc, @RejectionReason, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
