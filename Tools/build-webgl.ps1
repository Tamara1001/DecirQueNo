# Builds the WebGL version and zips it for itch.io.
# Usage:  powershell -ExecutionPolicy Bypass -File Tools\build-webgl.ps1
# Close the Unity Editor first: Unity cannot open the same project twice.
# Output: Builds\DecirQueNo-WebGL.zip   (log: Builds\build-webgl.log)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$versionLine = Get-Content (Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt') | Select-Object -First 1
$version = ($versionLine -replace 'm_EditorVersion:\s*', '').Trim()

$candidates = @(
    "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe",
    "D:\Unity\Hub\Editor\$version\Editor\Unity.exe",
    "D:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
)
if ($env:UNITY_EXE) { $candidates = @($env:UNITY_EXE) + $candidates }

$unity = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $unity) {
    Write-Error "Unity $version not found. Set the UNITY_EXE environment variable to the full path of Unity.exe."
}

if (Get-Process -Name 'Unity' -ErrorAction SilentlyContinue) {
    Write-Warning 'A Unity Editor is open. If it has this project open, the build will fail. Close it, or use the menu DecirQueNo > Build > WebGL for itch.io (zip).'
}

New-Item -ItemType Directory -Force (Join-Path $projectRoot 'Builds') | Out-Null
$log = Join-Path $projectRoot 'Builds\build-webgl.log'

Write-Host "Unity: $unity"
& $unity -batchmode -quit -nographics -projectPath $projectRoot `
    -buildTarget WebGL `
    -executeMethod DecirQueNo.EditorTools.WebGLBuildTool.BuildCli `
    -logFile $log

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed (exit code $LASTEXITCODE). See $log"
}

Write-Host "Done: $(Join-Path $projectRoot 'Builds\DecirQueNo-WebGL.zip')"
