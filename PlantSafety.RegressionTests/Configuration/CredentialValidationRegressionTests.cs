// ============================================================================
//  Test IDs:  PLNT-REG-CFG-001
//  Purpose:   Pins the credential-validation fix applied across eight Azure
//             projects.
//
//             Each read a required credential with
//                 Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
//                     ?? throw new InvalidOperationException(...)
//             which looks like validation and is not. `??` only fires on null.
//             An environment variable that is set but EMPTY -- the normal
//             result of `AZURE_CLIENT_ID=` in a compose file, a blank
//             deployment template value, or a secret that failed to mount --
//             returns "" and passes straight through the guard. The empty
//             string then reached ManagedIdentityCredentialFactory, and the
//             failure surfaced later as an opaque Azure authentication error
//             rather than as the misconfiguration it was.
//
//             The repaired form uses string.IsNullOrWhiteSpace, which rejects
//             null, empty and whitespace alike.
// ============================================================================
using PlantSafety.RegressionTests.Support;

namespace PlantSafety.RegressionTests.Configuration;

public sealed class CredentialValidationRegressionTests
{
    [Fact(DisplayName = "PLNT-REG-CFG-001: No environment-variable credential is validated with ?? alone")]
    public void EnvironmentVariableCredentials_ShouldRejectEmptyAndWhitespace()
    {
        var offenders = Directory
            .EnumerateFiles(WorkspaceRoot.Directory.FullName, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}.claude{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            // This file quotes the defective pattern in its own header comment.
            .Where(p => !p.EndsWith(nameof(CredentialValidationRegressionTests) + ".cs", StringComparison.OrdinalIgnoreCase))
            .Select(p => new { Path = p, Text = File.ReadAllText(p) })
            // The whole defect in one pattern: a GetEnvironmentVariable call whose only guard is
            // `?? throw`. Matched across newlines because the throw is usually wrapped onto the
            // following line, which is what makes it easy to read past in review.
            .Where(x => Regex.IsMatch(x.Text, @"GetEnvironmentVariable\s*\([^)]*\)\s*\?\?\s*throw", RegexOptions.Singleline))
            .Select(x => Path.GetFileName(x.Path))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        offenders.Should().BeEmpty(
            "`?? throw` only rejects a null environment variable. A variable that is set but empty " +
            "-- a blank value in a compose file or an unmounted secret -- passes the guard and the " +
            "empty credential fails much later as an opaque authentication error. Use " +
            "string.IsNullOrWhiteSpace:\n{0}", string.Join("\n", offenders));
    }
}
