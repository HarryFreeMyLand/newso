# Make sure you have "screen" installed
# =====================================
# macOS: sudo port install screen
# Ubuntu: apt-get install screen
# Arch: pacman -S screen

screen -S NsoApi -d -m dotnet run FSO.Server.Api.dll