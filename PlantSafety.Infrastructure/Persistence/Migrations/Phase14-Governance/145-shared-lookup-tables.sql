-- =============================================
-- Migration:     145-shared-lookup-tables.sql
-- Author:        Documentation Agent
-- Create date:   2026-07-19
-- Description:   Adds two canonical, additive-only reference/lookup tables
--                to PlantSafety.CoreDB: System.StatusLookup and
--                System.SeverityLevel.
--
-- Genuine-duplication check performed first (per task instructions):
--   - Status: 33 CoreDB tables/schemas have a Status column (nvarchar(*),
--     unconstrained, no CHECK constraint -- confirmed live 2026-07-19 via
--     sys.columns). Inspected the backing PlantSafety.Domain C# enums for
--     22 of them (e.g. AuditingStatus, CultureStatus, ChecklistsStatus,
--     IncidentManagementStatus) and found them byte-for-byte identical:
--     Created=0, Active=1, Completed=2, Closed=3, Archived=4. This exact
--     5-value vocabulary is ALREADY documented as a shared pattern in
--     PlantSafety.Domain.Common.Types.Enums.DomainLifecycleStatus.cs
--     ("Base lifecycle status pattern reused across modules that share the
--     canonical Created > Active > Closed > Archived lifecycle"), but that
--     shared pattern is not enforced or even referenced at the SQL layer --
--     each table's Status column is free nvarchar. This is genuine,
--     confirmed duplication, not a guess.
--   - Severity: PlantSafety.Domain.Common.Types.Enums.DomainSeverity.cs is
--     explicitly documented as "Cross-cutting severity classification used
--     across all modules for exceptions, alerts, incidents, and
--     observations" (Info/Low/Medium/High/Critical, 0-4). The CoreDB
--     Severity columns (Operational.Alert/AlertSummary, SafetyEvents.
--     EmergencyManagement/IncidentManagement/IncidentManagementSummary) are
--     likewise unconstrained nvarchar. Same pattern as Status.
--
-- Scope decision (per task instructions -- additive-only, no FK risk):
--   This migration ONLY creates and seeds the two lookup tables. It does
--   NOT alter any of the 33+ existing Status/Severity columns, does NOT add
--   FK constraints to them, and does NOT touch the 66 Phase 1 CHECK
--   constraints. Wiring existing columns to these lookups via FK is a
--   separate, higher-risk follow-up that needs its own careful, verified
--   pass (existing/future data must be confirmed to only ever contain these
--   5 values first) -- explicitly deferred, not attempted here.
--
-- Idempotency: CREATE TABLE guarded by existence check; seed rows use
-- MERGE keyed on the natural key (Code) so re-running does not duplicate
-- or perturb any operator-added rows. Zero rows of any OTHER table are
-- touched by this migration.
-- =============================================
SET NOCOUNT ON;
GO
USE [PlantSafety.CoreDB];
GO

------------------------------------------------------------
-- 1. System.StatusLookup
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'System' AND t.name = N'StatusLookup')
BEGIN
    CREATE TABLE [System].[StatusLookup] (
        StatusLookupId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_System_StatusLookup PRIMARY KEY CLUSTERED,
        Code           nvarchar(50)   NOT NULL,
        SortOrder      int            NOT NULL,
        IsTerminal     bit            NOT NULL CONSTRAINT DF_System_StatusLookup_IsTerminal DEFAULT (0),
        Description    nvarchar(400)  NULL,
        CONSTRAINT UQ_System_StatusLookup_Code UNIQUE (Code)
    );
END
GO

MERGE [System].[StatusLookup] AS tgt
USING (VALUES
    (N'Created',   0, CAST(0 AS bit), N'Record has been created and awaits processing.'),
    (N'Active',    1, CAST(0 AS bit), N'Record is active and under management.'),
    (N'Completed', 2, CAST(0 AS bit), N'Record has been completed or resolved.'),
    (N'Closed',    3, CAST(1 AS bit), N'Record has been closed with no further action required.'),
    (N'Archived',  4, CAST(1 AS bit), N'Record has been archived for long-term retention.')
) AS src (Code, SortOrder, IsTerminal, Description)
ON tgt.Code = src.Code
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Code, SortOrder, IsTerminal, Description) VALUES (src.Code, src.SortOrder, src.IsTerminal, src.Description);
GO

------------------------------------------------------------
-- 2. System.SeverityLevel
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'System' AND t.name = N'SeverityLevel')
BEGIN
    CREATE TABLE [System].[SeverityLevel] (
        SeverityLevelId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_System_SeverityLevel PRIMARY KEY CLUSTERED,
        Code            nvarchar(50)   NOT NULL,
        Rank            int            NOT NULL,
        Description     nvarchar(400)  NULL,
        CONSTRAINT UQ_System_SeverityLevel_Code UNIQUE (Code)
    );
END
GO

MERGE [System].[SeverityLevel] AS tgt
USING (VALUES
    (N'Info',     0, N'Informational -- no action required.'),
    (N'Low',      1, N'Low severity -- monitor and log.'),
    (N'Medium',   2, N'Medium severity -- requires attention within the shift.'),
    (N'High',     3, N'High severity -- requires immediate attention.'),
    (N'Critical', 4, N'Critical severity -- safety-of-life or production-stopping.')
) AS src (Code, Rank, Description)
ON tgt.Code = src.Code
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Code, Rank, Description) VALUES (src.Code, src.Rank, src.Description);
GO

------------------------------------------------------------
-- 3. Documentation.
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties ep WHERE ep.major_id = OBJECT_ID(N'System.StatusLookup') AND ep.minor_id = 0 AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Canonical reference vocabulary for the Created/Active/Completed/Closed/Archived lifecycle shared by ~22+ CoreDB aggregate Status columns, mirroring PlantSafety.Domain.Common.Types.Enums.DomainLifecycleStatus. Additive/reference-only -- NOT yet wired via FK to the existing Status columns (those remain free nvarchar as of this migration); see 145-shared-lookup-tables.sql header for the deferred-FK-wiring rationale.', @level0type=N'SCHEMA', @level0name=N'System', @level1type=N'TABLE', @level1name=N'StatusLookup';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Canonical reference vocabulary for the Created/Active/Completed/Closed/Archived lifecycle shared by ~22+ CoreDB aggregate Status columns, mirroring PlantSafety.Domain.Common.Types.Enums.DomainLifecycleStatus. Additive/reference-only -- NOT yet wired via FK to the existing Status columns (those remain free nvarchar as of this migration); see 145-shared-lookup-tables.sql header for the deferred-FK-wiring rationale.', @level0type=N'SCHEMA', @level0name=N'System', @level1type=N'TABLE', @level1name=N'StatusLookup';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep WHERE ep.major_id = OBJECT_ID(N'System.SeverityLevel') AND ep.minor_id = 0 AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Canonical reference vocabulary for the Info/Low/Medium/High/Critical severity scale shared by CoreDB Severity columns (Operational.Alert/AlertSummary, SafetyEvents.EmergencyManagement/IncidentManagement/IncidentManagementSummary), mirroring PlantSafety.Domain.Common.Types.Enums.DomainSeverity. Additive/reference-only -- NOT yet wired via FK to the existing Severity columns; see 145-shared-lookup-tables.sql header for the deferred-FK-wiring rationale.', @level0type=N'SCHEMA', @level0name=N'System', @level1type=N'TABLE', @level1name=N'SeverityLevel';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Canonical reference vocabulary for the Info/Low/Medium/High/Critical severity scale shared by CoreDB Severity columns (Operational.Alert/AlertSummary, SafetyEvents.EmergencyManagement/IncidentManagement/IncidentManagementSummary), mirroring PlantSafety.Domain.Common.Types.Enums.DomainSeverity. Additive/reference-only -- NOT yet wired via FK to the existing Severity columns; see 145-shared-lookup-tables.sql header for the deferred-FK-wiring rationale.', @level0type=N'SCHEMA', @level0name=N'System', @level1type=N'TABLE', @level1name=N'SeverityLevel';
GO

------------------------------------------------------------
-- 4. Verification.
------------------------------------------------------------
SELECT * FROM [System].[StatusLookup] ORDER BY SortOrder;
SELECT * FROM [System].[SeverityLevel] ORDER BY Rank;
GO
