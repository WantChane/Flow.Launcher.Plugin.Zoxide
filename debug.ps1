$pluginName = "Zoxide"
$projectName = "Flow.Launcher.Plugin.$pluginName"

dotnet publish $projectName -c Debug -r win-x64 --no-self-contained

$scoopPath = "$((Get-Item (Get-Command scoop.ps1).Path).Directory.Parent.FullName)"
$flowLauncherExe = "$scoopPath\apps\Flow-Launcher\current\Flow.Launcher.exe"
$flowLauncherPlugin = "$scoopPath\persist\Flow-Launcher\UserData\Plugins"

if (Test-Path $flowLauncherExe) {
    Stop-Process -Name "Flow.Launcher" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2

    $pluginDest = "$flowLauncherPlugin\$pluginName"
    if (Test-Path $pluginDest) {
        Remove-Item -Recurse -Force $pluginDest
    }

    Copy-Item "$projectName\bin\Debug\win-x64\publish" "$flowLauncherPlugin\" -Recurse -Force
    Rename-Item -Path "$flowLauncherPlugin\publish" -NewName $pluginName

    Start-Sleep -Seconds 2
    Start-Process $flowLauncherExe
} else {
    Write-Host "Flow.Launcher.exe not found. Please install Flow Launcher first"
}
