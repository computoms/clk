# This script installs the clk command-line-tool

Write-Host "Installing clk"
$dest = "$env:UserProfile/.clk/bin"
$latestRelease = "v2.0"
mkdir -p $dest
Invoke-WebRequest -Uri "https://github.com/computoms/clk/releases/download/release%2F$lastestRelease/publish-windows-latest.zip" -OutFile "$dest/publish-windows-latest.zip"
Expand-Archive -Path "$dest/publish-windows-latest.zip" -Destination "$dest/"
rm "$dest/publish-windows-latest.zip"


if (-not (Test-Path -Path $Profile)) {
    "" | Out-File $Profile
}
$profileContent = Get-Content $Profile

if (-not ($profileContent -match ".clk/bin")) {
    $answer = Read-Host "Add ~/.clk/bin to PATH? (yes/no)"
    if ($answer -ne "yes") {
        exit
    }

    '$env:Path += ";$env:UserProfile/.clk/bin"' | Add-Content $Profile
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
