#!/bin/bash

# Fix .NET PATH on EC2
# Run this on EC2 if dotnet command is not found

echo "🔧 Fixing .NET PATH..."
echo ""

# Check if .NET is installed in root
if [ -d "/root/.dotnet" ]; then
    echo "⚠️  .NET found in /root/.dotnet"
    echo "Installing for ubuntu user..."
    
    # Install for ubuntu user (not root)
    wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    /tmp/dotnet-install.sh --channel 8.0
    
    # Add to PATH
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
    echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc
    
    # Reload
    export DOTNET_ROOT=$HOME/.dotnet
    export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
    
    echo "✅ .NET installed for ubuntu user"
else
    # Check if in ubuntu home
    if [ -d "$HOME/.dotnet" ]; then
        echo "✅ .NET found in $HOME/.dotnet"
        export DOTNET_ROOT=$HOME/.dotnet
        export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
    else
        echo "Installing .NET SDK 8.0..."
        wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
        chmod +x /tmp/dotnet-install.sh
        /tmp/dotnet-install.sh --channel 8.0
        
        echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
        echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc
        
        export DOTNET_ROOT=$HOME/.dotnet
        export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
    fi
fi

# Verify
echo ""
echo "Testing .NET installation..."
dotnet --version

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ .NET is working!"
    echo ""
    echo "To make it permanent, run:"
    echo "  source ~/.bashrc"
else
    echo ""
    echo "❌ .NET not found. Please check installation."
fi

