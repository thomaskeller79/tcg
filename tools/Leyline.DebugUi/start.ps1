# Stops any previous Leyline.DebugUi instance, then starts a fresh one.
#
# Why this exists: Ctrl+C sent to `dotnet run`'s console doesn't always reach the actual child
# process it spawns on Windows (Leyline.DebugUi.exe) — the terminal returns to your prompt, but
# the process (and its lock on port 5299 / the build output DLLs) can survive. Run this instead
# of `dotnet run --project tools/Leyline.DebugUi` directly so a leftover instance never blocks
# the next build or squats on the port.
#
# Usage (from anywhere):  .\tools\Leyline.DebugUi\start.ps1

$ErrorActionPreference = 'Stop'
$Port = 5299

# Primary: kill by name — catches an instance even if it somehow isn't on $Port.
Get-Process -Name 'Leyline.DebugUi' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# Belt-and-suspenders: kill whatever is actually squatting on the port, whatever it's called.
$conns = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
foreach ($conn in $conns) {
    Stop-Process -Id $conn.OwningProcess -Force -ErrorAction SilentlyContinue
}

if ($conns) {
    Start-Sleep -Milliseconds 500 # give the OS a moment to actually release the port
}

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Set-Location $RepoRoot
dotnet run --project tools/Leyline.DebugUi
