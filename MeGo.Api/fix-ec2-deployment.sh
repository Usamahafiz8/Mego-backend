#!/bin/bash

# Quick fix script for EC2 deployment
# Run this on your EC2 instance

echo "🔧 Fixing EC2 Deployment..."
echo ""

# Navigate to project directory
cd ~/Mego-backend/MeGo.Api

# Create appsettings.Production.json from example
if [ -f "appsettings.Production.json.example" ]; then
    cp appsettings.Production.json.example appsettings.Production.json
    echo "✅ Created appsettings.Production.json"
else
    echo "❌ appsettings.Production.json.example not found!"
    exit 1
fi

# Get RDS password from user
echo ""
read -p "Enter your RDS password: " -s RDS_PASSWORD
echo ""

# Update connection string
RDS_ENDPOINT="database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com"

if command -v python3 &> /dev/null; then
    python3 << PYEOF
import json
import sys

try:
    with open('appsettings.Production.json', 'r') as f:
        config = json.load(f)
    
    config['ConnectionStrings']['DefaultConnection'] = \
        f"Host=$RDS_ENDPOINT;Port=5432;Database=mego_prod;Username=postgres;Password=$RDS_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
    
    with open('appsettings.Production.json', 'w') as f:
        json.dump(config, f, indent=2)
    
    print("✅ Updated connection string")
except Exception as e:
    print(f"❌ Error: {e}")
    sys.exit(1)
PYEOF
else
    echo "⚠️  Python3 not found. Please manually update appsettings.Production.json"
    echo "Connection string: Host=$RDS_ENDPOINT;Port=5432;Database=mego_prod;Username=postgres;Password=$RDS_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
fi

# Publish with appsettings.Production.json included
echo ""
echo "📦 Publishing application..."
dotnet publish -c Release -o /var/www/mego

# Copy appsettings.Production.json to publish directory
cp appsettings.Production.json /var/www/mego/

echo ""
echo "✅ Deployment fixed!"
echo ""
echo "Now run:"
echo "  cd /var/www/mego"
echo "  ASPNETCORE_ENVIRONMENT=Production dotnet MeGo.Api.dll"
