param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "WindowResizerApp\WindowResizerApp.csproj"

dotnet publish $project `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=false
