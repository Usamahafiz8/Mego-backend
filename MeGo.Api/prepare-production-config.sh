#!/bin/bash

# Script to prepare production configuration locally
# Run this BEFORE deploying to EC2

echo "🔧 Preparing Production Configuration..."
echo ""

# Check if example file exists
if [ ! -f "appsettings.Production.json.example" ]; then
    echo "❌ appsettings.Production.json.example not found!"
    exit 1
fi

# Copy example to production config
cp appsettings.Production.json.example appsettings.Production.json

echo "✅ Created appsettings.Production.json"
echo ""
echo "📝 Please provide the following information:"
echo ""

# Get RDS details
read -p "RDS Endpoint [database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com]: " RDS_ENDPOINT
RDS_ENDPOINT=${RDS_ENDPOINT:-database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com}

read -p "RDS Database Name [mego_prod]: " DB_NAME
DB_NAME=${DB_NAME:-mego_prod}

read -p "RDS Username [postgres]: " DB_USER
DB_USER=${DB_USER:-postgres}

read -s -p "RDS Password: " RDS_PASSWORD
echo ""

read -p "JWT Secret Key (min 32 characters): " JWT_KEY

if [ -z "$JWT_KEY" ] || [ ${#JWT_KEY} -lt 32 ]; then
    echo "⚠️  JWT Key must be at least 32 characters. Generating one..."
    JWT_KEY=$(openssl rand -base64 32 | tr -d '\n')
    echo "✅ Generated JWT Key: $JWT_KEY"
fi

# Update config file
if command -v python3 &> /dev/null; then
    python3 << PYEOF
import json
import sys

try:
    with open('appsettings.Production.json', 'r') as f:
        config = json.load(f)
    
    # Update connection string
    config['ConnectionStrings']['DefaultConnection'] = \
        f"Host=$RDS_ENDPOINT;Port=5432;Database=$DB_NAME;Username=$DB_USER;Password=$RDS_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
    
    # Update JWT key
    config['Jwt']['Key'] = "$JWT_KEY"
    
    # Save
    with open('appsettings.Production.json', 'w') as f:
        json.dump(config, f, indent=2)
    
    print("✅ Updated appsettings.Production.json")
except Exception as e:
    print(f"❌ Error: {e}")
    sys.exit(1)
PYEOF
else
    echo "⚠️  Python3 not found. Please install it or update appsettings.Production.json manually"
    exit 1
fi

echo ""
echo "✅ Production configuration ready!"
echo ""
echo "📄 File: appsettings.Production.json"
echo "⚠️  Remember: This file contains secrets. Don't commit it to git!"
echo ""
echo "Next steps:"
echo "  1. Commit and push your code (appsettings.Production.json is in .gitignore)"
echo "  2. Deploy to EC2 using deploy-to-ec2.sh"
