<#
.SYNOPSIS
    Restore, build (Release, warnings as errors) and test the whole Mizad solution (agent.md task 1.14).

.EXAMPLE
    ./scripts/build.ps1                 # restore + build + all tests
    ./scripts/build.ps1 -SkipTests      # restore + build only
    ./scripts/build.ps1 -UnitOnly       # skip integration tests (no SQL Server needed)

.NOTES
    Integration tests need SQL Server: MIZAD_TEST_SQL or the local default instance (see docs/TEST_PLAN.md).
    Exit code is non-zero on any failure.
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [switch] $SkipTests,
    [switch] $UnitOnly
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root 'Mizad.slnx'
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

function Invoke-Step([string] $Name, [scriptblock] $Action) {
    Write-Host "==> $Name" -ForegroundColor Cyan
    & $Action
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAILED: $Name (exit code $LASTEXITCODE)" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Push-Location $root
try {
    Invoke-Step 'Restore' { dotnet restore $solution --nologo }
    Invoke-Step "Build ($Configuration)" { dotnet build $solution -c $Configuration --no-restore --nologo }

    if (-not $SkipTests) {
        if ($UnitOnly) {
            Invoke-Step 'Unit tests' { dotnet test --project (Join-Path $root 'tests/Mizad.UnitTests/Mizad.UnitTests.csproj') -c $Configuration --no-build }
        }
        else {
            Invoke-Step 'All tests' { dotnet test --solution $solution -c $Configuration --no-build }
        }
    }

    Write-Host ("OK - completed in {0:n1}s" -f $stopwatch.Elapsed.TotalSeconds) -ForegroundColor Green
}
finally {
    Pop-Location
}
