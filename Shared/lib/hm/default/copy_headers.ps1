<#
.SYNOPSIS
    Copies HM library header files to a target directory.

.DESCRIPTION
    This script copies all necessary header files from HM source libraries to a target include directory,
    maintaining the original directory structure for proper include path resolution.

.PARAMETER HmRoot
    The root directory of the HM repository. Must contain 'source\Lib' subdirectory.

.PARAMETER TargetDir
    The target directory for header files. Defaults to 'include' in the script directory.
    If the directory exists, it will be deleted and recreated.

.EXAMPLE
    .\copy_headers_hm.ps1 -HmRoot "D:\Project\Other\HM"
    Copies headers from HM to the default 'include' directory.

.EXAMPLE
    .\copy_headers_hm.ps1 -HmRoot "D:\Project\Other\HM" -TargetDir "D:\MyProject\hm_headers"
    Copies headers to a custom directory.

.NOTES
    Exit codes:
    0 - Success
    1 - Some files failed to copy
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateScript({
        if(-Not (Test-Path $_)) {
            throw "HM root directory does not exist: $_"
        }
        if(-Not (Test-Path (Join-Path $_ "source\Lib"))) {
            throw "Invalid HM root directory. 'source\Lib' not found in: $_"
        }
        return $true
    })]
    [string]$HmRoot,
    [string]$TargetDir = "include"
)


$hmRootPath = Resolve-Path $HmRoot
$targetPath = if([System.IO.Path]::IsPathRooted($TargetDir)) {
    $TargetDir
} else {
    Join-Path $PSScriptRoot $TargetDir
}


$sourceLibPath = Join-Path $hmRootPath "source\Lib"

$requiredLibraries = @(
    "libmd5",
    "TLibCommon",
    "TLibCommonAnalyser",
    "TLibDecoder",
    "TLibDecoderAnalyser",
    "TLibEncoder",
    "Utilities"
)


if (Test-Path $targetPath) {
    Write-Host "Cleaning existing directory: $targetPath"
    Remove-Item -Path $targetPath -Recurse -Force
}

Write-Host "Copying HM headers to $targetPath"

New-Item -ItemType Directory -Path $targetPath -Force | Out-Null

$totalFilesCopied = 0
$failedCopies = @()

foreach ($library in $requiredLibraries) {
    $sourcePath = Join-Path $sourceLibPath $library
    $targetLibPath = Join-Path $targetPath $library


    if (-Not (Test-Path $sourcePath)) {
        Write-Host "Warning: Library not found: $library (skipping)" -ForegroundColor Yellow
        continue
    }


    New-Item -ItemType Directory -Path $targetLibPath -Force | Out-Null


    $headerFiles = Get-ChildItem -Path $sourcePath -Filter "*.h" -Recurse -File
    $libraryFileCount = 0

    foreach ($headerFile in $headerFiles) {
        try {

            $relativePath = $headerFile.FullName.Substring($sourcePath.Length)
            if ($relativePath.StartsWith("\")) {
                $relativePath = $relativePath.Substring(1)
            }

            $targetFilePath = Join-Path $targetLibPath $relativePath
            $targetFileDir = Split-Path $targetFilePath -Parent


            if (-Not (Test-Path $targetFileDir)) {
                New-Item -ItemType Directory -Path $targetFileDir -Force | Out-Null
            }


            Copy-Item -Path $headerFile.FullName -Destination $targetFilePath -Force
            $libraryFileCount++
            $totalFilesCopied++
        }
        catch {
            $failedCopies += @{
                Source = $headerFile.FullName
                Error = $_.Exception.Message
            }
        }
    }
}

Write-Host "Copied $totalFilesCopied header files"

if ($failedCopies.Count -gt 0) {
    Write-Host "Warning: $($failedCopies.Count) files failed to copy" -ForegroundColor Yellow
    exit 1
}