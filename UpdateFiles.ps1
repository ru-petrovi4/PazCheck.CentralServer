$PSScriptRoot = ($MyInvocation.MyCommand.Path | Split-Path | Resolve-Path).ProviderPath

Remove-Item "$PSScriptRoot\Addons\*" -Recurse -Force
Copy-Item -Path "$PSScriptRoot\Simcode.PazCheck.CentralServer\bin\Release\net10.0\Addons\*" -Destination "$PSScriptRoot\Addons\" -Recurse
Remove-Item "$PSScriptRoot\Addons\net10.0\*.pdb" -Recurse -Force

& "C:\Program Files\PostgreSQL\14\bin\pg_dump.exe" -U postgres -d pazcheck -f Simcode.PazCheck.CentralServer\PazCheck.sql
Read-Host -Prompt "Press Enter to exit"