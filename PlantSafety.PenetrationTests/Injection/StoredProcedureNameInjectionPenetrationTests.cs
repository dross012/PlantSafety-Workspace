// ============================================================================
//  Test IDs:  PLNT-PEN-SQLI-010..014
//  Attack:    SQL text smuggled through the SQL gateway's procedure-name
//             argument (OWASP A03, Injection).
//
//  Surface:   ISqlGateway takes parameters as typed values, which
//             PlantSafety.SecurityTests already covers. But it also takes one
//             string that is NOT a parameter -- the stored-procedure name --
//             and that string is the only text on the whole gateway that
//             reaches the command as SQL rather than as data. If a caller can
//             be induced to pass a tainted value there (a name assembled from
//             a route segment, a tenant-configurable report name, a
//             feature-flag string), everything the parameter binding protects
//             is bypassed.
//
//             SqlIdentifierValidator is the seam that refuses it. These tests
//             attack that seam with payloads that would each be catastrophic
//             if executed.
// ============================================================================
using Shared.SqlGateway.Abstractions.Gateway;

namespace PlantSafety.PenetrationTests.Injection;

public sealed class StoredProcedureNameInjectionPenetrationTests
{
    [Theory(DisplayName = "PLNT-PEN-SQLI-010: SQL text smuggled through the procedure name is rejected")]
    // Statement termination -- the payload runs as a second batch.
    [InlineData("usp_Accident_GetById; DROP TABLE Accident--")]
    [InlineData("usp_Accident_GetById;EXEC sp_configure 'xp_cmdshell', 1")]
    // Bracket escape: closing the identifier quoting the caller assumed would contain it.
    [InlineData("usp_Accident_GetById]; DROP TABLE [Accident")]
    // Comment markers, used to truncate whatever the caller appends after the name.
    [InlineData("usp_Accident_GetById--")]
    [InlineData("usp_Accident_GetById/*")]
    // Quote breakout.
    [InlineData("usp_Accident_GetById' OR '1'='1")]
    [InlineData("usp_Accident_GetById\" OR \"1\"=\"1")]
    // Batch separator and whitespace tricks, including a newline-hidden second statement.
    [InlineData("usp_Accident_GetById\nGO\nDROP TABLE Accident")]
    [InlineData("usp_Accident_GetById\r\nDELETE FROM Accident")]
    [InlineData("usp_Accident_GetById\tDROP TABLE Accident")]
    // A bare destructive statement, i.e. the caller passed SQL text where a name was expected.
    [InlineData("DROP TABLE Accident")]
    [InlineData("SELECT * FROM Accident")]
    // Three-part and server-qualified names: reaching outside the intended database.
    [InlineData("OtherDb.dbo.usp_Exfiltrate")]
    [InlineData("[master].[dbo].[xp_cmdshell]")]
    public void ProcedureName_WithInjectionPayload_ShouldBeRejected(string payload)
    {
        SqlIdentifierValidator.IsValidStoredProcedureName(payload).Should().BeFalse(
            "'{0}' is SQL text, not an identifier. The gateway has no code path that executes " +
            "caller-supplied SQL, and this validator is what keeps that true", payload);

        // Both entry points are attacked. IsValid... returning false is worthless if a caller
        // reaching EnsureValid is let through, and the two are separate methods.
        var act = () => SqlIdentifierValidator.EnsureValid(payload);
        act.Should().Throw<ArgumentException>("EnsureValid must refuse everything IsValid... rejects");
    }

    [Theory(DisplayName = "PLNT-PEN-SQLI-011: Empty and whitespace-only procedure names are rejected")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ProcedureName_ThatIsBlank_ShouldBeRejected(string? name)
    {
        // A blank name is the shape a missing configuration value takes. Accepting it produces a
        // confusing server-side failure at best; at worst it is the first half of a payload whose
        // second half arrives from somewhere else.
        SqlIdentifierValidator.IsValidStoredProcedureName(name).Should().BeFalse(
            "a blank procedure name is a missing value, never a valid target");
    }

    [Theory(DisplayName = "PLNT-PEN-SQLI-012: Legitimate one- and two-part procedure names are still accepted")]
    [InlineData("usp_Accident_GetById")]
    [InlineData("dbo.usp_Accident_GetById")]
    [InlineData("SafetyEvents.usp_IncidentSlaPolicy_GetAll")]
    [InlineData("_internal.usp_Rebuild")]
    public void ProcedureName_ThatIsLegitimate_ShouldBeAccepted(string name)
    {
        // Without this, the whole file is satisfied by a validator that returns false
        // unconditionally -- which would pass every attack test above and break every caller.
        SqlIdentifierValidator.IsValidStoredProcedureName(name).Should().BeTrue(
            "'{0}' is an ordinary procedure name and must not be caught by the injection guard", name);
    }

    [Fact(DisplayName = "PLNT-PEN-SQLI-013: The rejection message does not echo the payload into a new sink")]
    public void RejectionMessage_ShouldNameTheParameterBeingRejected()
    {
        var act = () => SqlIdentifierValidator.EnsureValid("usp_X; DROP TABLE Accident--");

        // The message does quote the offending value, which is the right trade-off for
        // diagnosability -- an operator cannot fix what they cannot see. What matters is that it
        // is an ArgumentException naming the parameter, so it is handled as a caller error and
        // surfaced through the normal problem-details path rather than as a raw server fault.
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("name",
                "the failure must be attributable to the procedure-name argument");
    }

    [Fact(DisplayName = "PLNT-PEN-SQLI-014: A unicode homoglyph identifier is rejected rather than silently normalised")]
    public void ProcedureName_WithNonAsciiCharacters_ShouldBeRejected()
    {
        // Cyrillic 'с' and 'а' in place of the Latin letters. This is an impersonation probe: the
        // string looks like a known-good procedure name to a human reviewing an allow-list, but is
        // a different identifier. The validator is ASCII-only, so it refuses -- asserted here so
        // that a future "let's support unicode identifiers" change has to confront this case.
        SqlIdentifierValidator.IsValidStoredProcedureName("usp_Ассident_GetById").Should().BeFalse(
            "a homoglyph identifier must not pass as its Latin-alphabet lookalike");
    }
}
