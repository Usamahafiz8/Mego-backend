#!/bin/bash

# Quick EC2 Setup Script
# Run this on a fresh EC2 Ubuntu instance

echo "🚀 MeGo API - Quick EC2 Setup"
echo "============================="
echo ""

# Update system
echo "📦 Updating system..."
sudo apt update && sudo apt upgrade -y

# Install .NET SDK 8.0
echo "📦 Installing .NET SDK 8.0..."
wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 8.0

# Add to PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

# Install dependencies
echo "📦 Installing dependencies..."
sudo apt install -y postgresql-client nginx git curl

# Verify installation
echo ""
echo "✅ Installation Complete!"
echo ""
echo "📋 Installed:"
dotnet --version && echo "  ✅ .NET SDK"
psql --version && echo "  ✅ PostgreSQL client"
nginx -v && echo "  ✅ Nginx"
git --version && echo "  ✅ Git"
echo ""
echo "📝 Next Steps:"
echo "  1. Clone or upload your code"
echo "  2. Run: ./deploy-to-ec2.sh"
echo ""

