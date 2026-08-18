# 运行 Unity Code Guard 机械门禁
# 用法: & run-guard.ps1 <Unity项目根目录> [--files 文件...] [--json] [--fail-on-warn]
$ErrorActionPreference = 'Stop'

if ($args.Count -eq 0 -or $args[0].StartsWith('-')) {
    Write-Host '用法: run-guard.ps1 <Unity项目根目录> [--files 文件...] [--json] [--fail-on-warn]'
    exit 2
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

& dotnet script (Join-Path $scriptDir 'lint.csx') -- @args
exit $LASTEXITCODE