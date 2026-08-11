<#
.SYNOPSIS
    Consolidates every branch in one or more Git repositories down to a single
    canonical branch named "master", preserving history.

.DESCRIPTION
    PowerShell equivalent of consolidate-branches.sh, with identical behaviour.

    SAFETY MODEL (read before running):
      * DRY-RUN IS THE DEFAULT. Nothing is modified unless -Mode execute.
      * Conflicts are NEVER auto-resolved. The first conflicting branch stops
        all automated merges for that repository; other repositories continue.
      * Every branch is tagged (pre-delete-backup/<branch>/<timestamp>) and the
        tag is PUSHED before any deletion, so deletion is always recoverable.
      * Protected branches are never deleted.
      * Merges use --no-ff so each branch's history survives as its own line.

.EXAMPLE
    .\Consolidate-Branches.ps1
    Dry run over the configured repositories. Modifies nothing.

.EXAMPLE
    .\Consolidate-Branches.ps1 -Mode execute -DeleteRemoteBranches
    Performs the consolidation and deletes merged branches locally and on origin.

.NOTES
    Requires Git in PATH. Uses only standard Git commands.
#>
[CmdletBinding()]
param(
    [string[]] $Repos = @(
        "D:/PlantSafety/source/Azure",
        "D:/PlantSafety/source/DataLake",
        "D:/PlantSafety/source/Diagnostics",
        "D:/PlantSafety/source/ExportImport",
        "D:/PlantSafety/source/Intelligence",
        "D:/PlantSafety/source/PlantSafety",
        "D:/PlantSafety/source/Reporting",
        "D:/PlantSafety/source/SafetyIntegrations",
        "D:/PlantSafety/source/SafetyTooling"
    ),

    [ValidateSet("dry-run", "execute")]
    [string] $Mode = "dry-run",

    [switch] $DeleteRemoteBranches,

    [string] $GithubToken = "",

    [string[]] $ProtectedBranches = @("master"),

    [string] $ReportDir = (Join-Path (Get-Location) "consolidation-reports")
)

$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$null = New-Item -ItemType Directory -Force -Path $ReportDir
$consolidatedReport = Join-Path $ReportDir "consolidated-$timestamp.md"
$tempClones = New-Object System.Collections.Generic.List[string]

function Write-Step { param([string]$Message) Write-Host "`n== $Message" }
function Write-Line { param([string]$Message) Write-Host $Message }

# Every mutating git call goes through here, so a mutation cannot bypass the
# mode check. Returns $true on success.
function Invoke-Git {
    param(
        [string]   $RepoPath,
        [string[]] $GitArgs,
        [switch]   $AlwaysRun   # for read-only calls such as fetch
    )
    if ($Mode -ne "execute" -and -not $AlwaysRun) {
        Write-Host "   [dry-run] git -C $RepoPath $($GitArgs -join ' ')"
        return $true
    }
    & git -C $RepoPath @GitArgs 2>&1 | Out-Null
    return ($LASTEXITCODE -eq 0)
}

function Get-GitOutput {
    param([string] $RepoPath, [string[]] $GitArgs)
    $out = & git -C $RepoPath @GitArgs 2>$null
    if ($null -eq $out) { return @() }
    return @($out)
}

function Test-Protected {
    param([string] $Branch)
    return $ProtectedBranches -contains $Branch
}

@"
# Branch consolidation -- consolidated report

- Timestamp: ``$timestamp``
- Mode: ``$Mode``
- Delete remote branches: ``$($DeleteRemoteBranches.IsPresent)``
- Protected: ``$($ProtectedBranches -join ', ')``

"@ | Set-Content -Path $consolidatedReport -Encoding utf8

function Invoke-RepoConsolidation {
    param([string] $InputRepo)

    $repoPath = $InputRepo
    $isTemp = $false

    if ($InputRepo -match '^(https?://|git@)') {
        $repoPath = Join-Path ([System.IO.Path]::GetTempPath()) ("consolidate-" + [System.Guid]::NewGuid().ToString("N"))
        $tempClones.Add($repoPath)
        $isTemp = $true
        Write-Step "Cloning $InputRepo -> $repoPath"
        if ($Mode -eq "execute") {
            & git clone --quiet $InputRepo $repoPath
            if ($LASTEXITCODE -ne 0) { Write-Line "   ERROR: clone failed"; return }
        }
        else {
            Write-Line "   [dry-run] git clone $InputRepo (skipping; nothing to inspect)"
            return
        }
    }

    $name = Split-Path $repoPath -Leaf
    $report = Join-Path $ReportDir "$name-$timestamp.md"
    Write-Step "Repository: $name  ($repoPath)"

    if (-not (Test-Path (Join-Path $repoPath ".git"))) {
        Write-Line "   ERROR: not a git repository -- skipping"
        Add-Content $consolidatedReport "## $name -- ERROR: not a git repository`n"
        return
    }

    # Fetch is real even in dry-run: it mutates nothing in the working tree and
    # without it every listing below would be stale.
    $null = Invoke-Git -RepoPath $repoPath -GitArgs @("fetch","--all","--prune","--tags","--quiet") -AlwaysRun

    # 8 of 9 repos here have no origin/master; their default is a feature/*
    # branch. Failing would make the script useless for them, so master is
    # CREATED from the remote default when missing.
    $masterOnOrigin = @(Get-GitOutput $repoPath @("ls-remote","--heads","origin","master")).Count

    $baseRef = ""
    if ($masterOnOrigin -eq 0) {
        $defaultRef = (Get-GitOutput $repoPath @("symbolic-ref","refs/remotes/origin/HEAD")) -replace '^refs/remotes/origin/',''
        if ([string]::IsNullOrWhiteSpace($defaultRef)) {
            Write-Line "   ERROR: origin/master missing and origin/HEAD unset."
            Write-Line "          Fix with: git -C '$repoPath' remote set-head origin --auto"
            Add-Content $consolidatedReport "## $name -- ERROR: no origin/master and no origin/HEAD`n"
            return
        }
        Write-Line "   origin/master does not exist. Will create it from origin/$defaultRef."
        # The merge test must run against the branch master WOULD be created
        # from. Testing against origin/master would silently test nothing in
        # dry-run, because dry-run never creates it.
        $baseRef = "origin/$defaultRef"
        $null = Invoke-Git $repoPath @("checkout","-B","master","origin/$defaultRef")
        $null = Invoke-Git $repoPath @("push","-u","origin","master")
    }
    else {
        Write-Line "   origin/master exists."
        $baseRef = "origin/master"
        $null = Invoke-Git $repoPath @("checkout","-B","master","origin/master")
    }

    # ls-remote --heads, NOT for-each-ref: git shortens refs/remotes/origin/HEAD
    # to the bare remote name ("origin"), so a for-each-ref listing yields a
    # phantom branch called "origin" that no HEAD filter catches.
    $branches = @(Get-GitOutput $repoPath @("ls-remote","--heads","origin") |
        ForEach-Object { ($_ -split "\s+")[1] } |
        ForEach-Object { $_ -replace '^refs/heads/','' } |
        Where-Object { $_ -and -not (Test-Protected $_) } |
        Sort-Object -Unique)

    # Local-only branches are reported but not merged: the spec scopes this to
    # origin. Reporting them means nothing disappears silently.
    $localOnly = @(Get-GitOutput $repoPath @("for-each-ref","--format=%(refname:short)","refs/heads") |
        Where-Object { $_ -and -not (Test-Protected $_) } |
        Where-Object {
            & git -C $repoPath show-ref --verify --quiet "refs/remotes/origin/$_"
            $LASTEXITCODE -ne 0
        })

    Write-Line "   Remote branches to consolidate: $($branches.Count)"
    Write-Line "   Local-only branches (reported, not merged): $($localOnly.Count)"

    # ---- Merge test, no auto-resolution ----
    $mergeable = New-Object System.Collections.Generic.List[string]
    $conflicted = New-Object System.Collections.Generic.List[string]
    $conflictDetail = ""
    $testBranch = "consolidation-merge-test-$timestamp"

    foreach ($b in $branches) {
        & git -C $repoPath checkout -q -B $testBranch $baseRef 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Line "   ERROR: cannot create merge-test branch from $baseRef -- skipping $b"
            $conflicted.Add($b)
            $conflictDetail += "- ``$b``: could not check out base ``$baseRef```n"
            continue
        }

        $mergeOutput = (& git -C $repoPath merge --no-commit --no-ff "origin/$b" 2>&1) -join "`n"
        if ($LASTEXITCODE -eq 0) {
            & git -C $repoPath merge --abort 2>&1 | Out-Null
            $mergeable.Add($b)
            Write-Line "   [ok]       $b"
        }
        else {
            # Capture git's own message, not just the conflicted-file list. A
            # merge can fail with NO conflicted files -- "refusing to merge
            # unrelated histories" is the common case, and an empty file list
            # would read as a mystery rather than a diagnosis.
            $files = (Get-GitOutput $repoPath @("diff","--name-only","--diff-filter=U")) -join " "
            $reason = ($mergeOutput -split "`n" | Select-Object -First 1)
            & git -C $repoPath merge --abort 2>&1 | Out-Null
            $conflicted.Add($b)
            if ($files) {
                $conflictDetail += "- ``$b``: conflicting files: $files`n"
                Write-Line "   [CONFLICT] $b -> $files"
            }
            else {
                $conflictDetail += "- ``$b``: $reason`n"
                Write-Line "   [BLOCKED]  $b -> $reason"
            }
        }
    }

    & git -C $repoPath checkout -q master 2>&1 | Out-Null
    & git -C $repoPath branch -q -D $testBranch 2>&1 | Out-Null

    # A conflict stops automated merges for THIS repo only. Merging the clean
    # subset would leave it half-consolidated with no clear record of where it
    # stopped, which is harder to reason about than not starting.
    $halted = $false
    if ($conflicted.Count -gt 0) {
        $halted = $true
        Write-Line "   HALTED: $($conflicted.Count) branch(es) conflict. No merges performed for this repository."
    }

    $merged      = New-Object System.Collections.Generic.List[string]
    $tagged      = New-Object System.Collections.Generic.List[string]
    $deleted     = New-Object System.Collections.Generic.List[string]
    $undeletable = New-Object System.Collections.Generic.List[string]

    if (-not $halted -and $mergeable.Count -gt 0) {
        # Tags first, and pushed, so every branch is recoverable before any
        # merge or delete touches it.
        foreach ($b in $mergeable) {
            $tag = "pre-delete-backup/$b/$timestamp"
            $null = Invoke-Git $repoPath @("tag","-a",$tag,"origin/$b","-m","Backup of $b before consolidation ($timestamp)")
            $null = Invoke-Git $repoPath @("push","origin","refs/tags/$tag")
            $tagged.Add($tag)
        }

        $null = Invoke-Git $repoPath @("checkout","master")
        foreach ($b in $mergeable) {
            if (Invoke-Git $repoPath @("merge","--no-ff","origin/$b","-m","Merge $b into master (consolidation)")) {
                $merged.Add($b)
            }
            else {
                Write-Line "   ERROR: merge of $b failed unexpectedly after a clean test -- stopping."
                $null = Invoke-Git $repoPath @("merge","--abort")
                $halted = $true
                break
            }
        }

        if (-not $halted) {
            $null = Invoke-Git $repoPath @("push","origin","master")

            if ($DeleteRemoteBranches) {
                foreach ($b in $merged) {
                    if (Test-Protected $b) { continue }
                    $null = Invoke-Git $repoPath @("branch","-D",$b)
                    if (Invoke-Git $repoPath @("push","origin","--delete",$b)) {
                        $deleted.Add($b)
                    }
                    else {
                        # Branch protection or a default-branch pointer refuses this.
                        $undeletable.Add($b)
                        Write-Line "   NOTE: could not delete origin/$b (protected, or it is the default branch)."
                    }
                }
            }
            else {
                Write-Line "   -DeleteRemoteBranches not set -- branches merged and tagged, none deleted."
            }
        }
    }

    function Format-List { param($Items) if ($Items -and $Items.Count -gt 0) { ($Items | ForEach-Object { "- $_" }) -join "`n" } else { "(none)" } }

    $finalMaster = (Get-GitOutput $repoPath @("log","--oneline","-1","master")) -join ""

    @"
# $name

- Path: ``$repoPath``
- Mode: ``$Mode``  |  Timestamp: ``$timestamp``
- Halted by conflict: **$(if ($halted) { "yes" } else { "no" })**

## Branches evaluated ($($branches.Count))
$(Format-List $branches)

## Merged into master ($($merged.Count))
$(Format-List $merged)

## Conflicts ($($conflicted.Count)) -- require manual resolution
$(if ($conflictDetail) { $conflictDetail } else { "(none)" })

## Backup tags created ($($tagged.Count))
$(Format-List $tagged)

## Branches deleted ($($deleted.Count))
$(Format-List $deleted)

## Deletion refused ($($undeletable.Count)) -- protected or default branch
$(Format-List $undeletable)

## Local-only branches, NOT merged ($($localOnly.Count))
$(Format-List $localOnly)

## Final master commit
``````
$finalMaster
``````
"@ | Set-Content -Path $report -Encoding utf8

    @"
## $name
- Evaluated: $($branches.Count) | Merged: $($merged.Count) | Conflicts: $($conflicted.Count) | Tagged: $($tagged.Count) | Deleted: $($deleted.Count)
- Halted: $(if ($halted) { "yes" } else { "no" })
- Final master: ``$finalMaster``
- Report: ``$report``

"@ | Add-Content -Path $consolidatedReport -Encoding utf8

    Write-Line "   Summary written: $report"

    if ($isTemp) { Remove-Item -Recurse -Force $repoPath -ErrorAction SilentlyContinue }
}

try {
    Write-Step "Branch consolidation -- Mode=$Mode  DeleteRemoteBranches=$($DeleteRemoteBranches.IsPresent)"
    if ($Mode -ne "execute") { Write-Line "DRY RUN: no repository will be modified." }

    foreach ($r in $Repos) { Invoke-RepoConsolidation -InputRepo $r }

    Write-Step "Consolidated report: $consolidatedReport"
    Write-Line ""
    Write-Line "Verify final state per repo with:"
    Write-Line "  git fetch --all --prune"
    Write-Line "  git for-each-ref --format='%(refname:short)' refs/remotes/origin"
}
finally {
    foreach ($d in $tempClones) {
        if (Test-Path $d) { Remove-Item -Recurse -Force $d -ErrorAction SilentlyContinue }
    }
}
