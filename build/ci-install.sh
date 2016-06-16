KOREBUILD_DOTNET_VERSION=1.0.0-preview1-002702
KOREBUILD_DOTNET_CHANNEL=beta
pwd
chmod +x ./dotnet-install.sh

./dotnet-install.sh --channel $KOREBUILD_DOTNET_CHANNEL --version $KOREBUILD_DOTNET_VERSION
