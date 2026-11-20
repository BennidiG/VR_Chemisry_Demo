[CmdletBinding()]
param(
    [string]$UnityPath = $env:UNITY_PATH,
    [string]$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$SkipTests,
    [switch]$SkipBuild,
    [switch]$Release
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $UnityPath = 'C:\Program Files\Unity\Hub\Editor\2020.3.30f1c1\Editor\Unity.exe'
}
if (!(Test-Path -LiteralPath $UnityPath -PathType Leaf)) {
    throw "Unity executable not found: $UnityPath. Set UNITY_PATH or pass -UnityPath."
}

$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
$LogRoot = Join-Path $ProjectPath 'Logs/CI'
New-Item -ItemType Directory -Path $LogRoot -Force | Out-Null

function Invoke-Unity {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$LogName
    )
    $logPath = Join-Path $LogRoot $LogName
    Write-Host "Unity log: $logPath"
    & $UnityPath @Arguments -logFile $logPath
    if ($LASTEXITCODE -ne 0) {
        throw "Unity step failed with exit code $LASTEXITCODE. See $logPath"
    }
}

Invoke-Unity -Arguments @('-batchmode', '-nographics', '-projectPath', $ProjectPath, '-quit') -LogName 'import.log'

if (!$SkipTests) {
    Invoke-Unity -Arguments @('-batchmode', '-nographics', '-projectPath', $ProjectPath, '-runTests', '-testPlatform', 'EditMode', '-testResults', (Join-Path $LogRoot 'editmode-results.xml'), '-quit') -LogName 'editmode.log'
    Invoke-Unity -Arguments @('-batchmode', '-nographics', '-projectPath', $ProjectPath, '-runTests', '-testPlatform', 'PlayMode', '-testResults', (Join-Path $LogRoot 'playmode-results.xml'), '-quit') -LogName 'playmode.log'
}

Invoke-Unity -Arguments @('-batchmode', '-nographics', '-projectPath', $ProjectPath, '-executeMethod', 'ProjectBuildValidation.ValidateForCI', '-quit') -LogName 'project-validation.log'

if (!$SkipBuild) {
    if ($Release) {
        $method = 'ProjectBuildValidation.BuildSignedAndroidApk'
    } else {
        $method = 'ProjectBuildValidation.BuildAndroidApk'
    }
    Invoke-Unity -Arguments @('-batchmode', '-nographics', '-projectPath', $ProjectPath, '-executeMethod', $method, '-quit') -LogName 'android-build.log'
}

Write-Host "VR Chemistry validation completed. Logs: $LogRoot"
