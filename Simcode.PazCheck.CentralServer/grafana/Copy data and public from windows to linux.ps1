$PSScriptRoot = ($MyInvocation.MyCommand.Path | Split-Path | Resolve-Path).ProviderPath

$source = "$PSScriptRoot\windows\"
$destination = "$PSScriptRoot\linux\"
Remove-Item "$destination\data" -Recurse -ErrorAction Ignore
Remove-Item "$destination\public" -Recurse -ErrorAction Ignore
Copy-Item -Path "$source\data" -Destination "$destination" -Recurse
Copy-Item -Path "$source\public" -Destination "$destination" -Recurse
