# This script installs the clk command-line-tool

echo "Installing clk"
dest=~/.clk/bin
mkdir -p $dest
cd $dest
curl -sfL -o publish-macos-latest.zip "https://github.com/computoms/clk/releases/download/release%2Fv2.0/publish-macos-latest.zip"
unzip publish-macos-latest.zip
rm publish-macos-latest.zip

echo "clk installed in ~/.clk/bin"
echo "Please add '~/.clk/bin' to your path before using clk"