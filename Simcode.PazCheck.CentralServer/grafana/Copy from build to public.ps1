$source = "D:\Dev\grafana\public"
$destination = "D:\Dev\PazCheck.CentralServer\Simcode.PazCheck.CentralServer\grafana\windows\public"
Remove-Item "$destination" -Recurse -ErrorAction Ignore
Copy-Item -Path "$source" -Destination "$destination" -Recurse
