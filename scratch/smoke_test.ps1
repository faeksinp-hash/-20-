# Launch app and wait 4 seconds, then gracefully kill and check exit or crash
$proc = Start-Process -FilePath "bin\Debug\net9.0-windows\MatchaShop.exe" -PassThru
Start-Sleep -Seconds 4
if ($proc.HasExited) {
    Write-Host "Process exited unexpectedly! ExitCode: $($proc.ExitCode)"
    exit 1
} else {
    Write-Host "App launched and running healthy! PID: $($proc.Id)"
    Stop-Process -Id $proc.Id -Force
    Write-Host "App smoke test PASSED successfully!"
    exit 0
}
