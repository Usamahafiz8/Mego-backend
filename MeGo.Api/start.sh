#!/bin/bash

# MeGo API Backend Startup Script
# Sets up .NET SDK path and runs the backend

echo "🚀 Starting MeGo API Backend..."
echo ""

# Set .NET SDK path
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

# Navigate to script directory
cd "$(dirname "$0")"

# Check if .NET is available
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK not found!"
    echo "Please install .NET SDK 8.0 first."
    echo ""
    echo "Installation:"
    echo "  curl -sSL https://dot.net/v8/dotnet-install.sh | bash /dev/stdin --channel 8.0"
    exit 1
fi

echo "✅ .NET SDK Version: $(dotnet --version)"
echo "📁 Working Directory: $(pwd)"
echo ""

# Restore dependencies (if needed)
echo "📦 Restoring dependencies..."
dotnet restore --verbosity quiet

# Run the backend
echo "🌐 Starting API server on http://localhost:5144"
echo "📚 Swagger UI: http://localhost:5144/swagger"
echo "🏥 Health Check: http://localhost:5144/health"
echo ""
echo "Press Ctrl+C to stop the server"
echo ""

dotnet run
