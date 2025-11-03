param($path)

if (-not $path) {
    $path = "$env:USERPROFILE/.clk/bin"
}
dotnet publish .\clk\clk.csproj -c Release -o $path -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
