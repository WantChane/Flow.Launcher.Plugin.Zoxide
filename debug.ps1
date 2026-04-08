dotnet publish Flow.Launcher.Plugin.Zoxide -c Debug -r win-x64 --no-self-contained

$scoopPath = "$((Get-Item (Get-Command scoop.ps1).Path).Directory.Parent.FullName)"
$flowLauncherExe = "$scoopPath\apps\Flow-Launcher\current\Flow.Launcher.exe"
$flowLauncherPlugin = "$scoopPath\persist\Flow-Launcher\UserData\Plugins"


if (Test-Path $flowLauncherExe) {
    Stop-Process -Name "Flow.Launcher" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2

    if (Test-Path "$flowLauncherPlugin\Zoxide") {
        Remove-Item -Recurse -Force "$flowLauncherPlugin\Zoxide"
    }

    Copy-Item "Flow.Launcher.Plugin.Zoxide\bin\Debug\win-x64\publish" "$flowLauncherPlugin\" -Recurse -Force
    Rename-Item -Path "$flowLauncherPlugin\publish" -NewName "Zoxide"

    Start-Sleep -Seconds 2
    Start-Process $flowLauncherExe
} else {
    Write-Host "Flow.Launcher.exe not found. Please install Flow Launcher first"
}
