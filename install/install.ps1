# This script installs the clocknet command-line-tool

Write-Host "Installing clocknet"
$clocknetDest = "$env:UserProfile/.clock/bin"
$latestRelease = "v2.0"
mkdir -p $clocknetDest
Invoke-WebRequest -Uri "https://github.com/computoms/clocknet/releases/download/release%2F$lastestRelease/publish-windows-latest.zip" -OutFile "$clocknetDest/publish-windows-latest.zip"
Expand-Archive -Path "$clocknetDest/publish-windows-latest.zip" -Destination "$clocknetDest/"
rm "$clocknetDest/publish-windows-latest.zip"


if (-not (Test-Path -Path $Profile)) {
    "" | Out-File $Profile
}
$profileContent = Get-Content $Profile

if (-not ($profileContent -match "Set-Alias -Name clk -Value")) {
    $answer = Read-Host "Add clk alias to Powershell Profile? (yes/no)"
    if ($answer -ne "yes") {
        exit
    }

    'Set-Alias -Name clk -Value "$env:UserProfile/.clock/bin/clocknet.exe"' | Add-Content $PROFILE
}


if (-not ($profileContent -match "Set-Alias -Name clkz -Value")) {
    $answer = Read-Host "Add clkz alias to search for tasks using fzf? (yes/no)"
    if ($answer -ne "yes") {
        exit
    }

    'Set-Alias -Name clkz -Value "Clock-Fzf"' | Add-Content $PROFILE
    'function Clock-Fzf() {'        | Add-Content $PROFILE
    '   $TASK=$(clk list | fzf);'   | Add-Content $PROFILE
    '    clk add $TASK'             | Add-Content $PROFILE
    '}'                             | Add-Content $PROFILE
}
