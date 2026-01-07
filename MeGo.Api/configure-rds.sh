#!/bin/bash

# AWS RDS Configuration Script for MeGo API
# This script helps configure the connection to AWS RDS PostgreSQL

echo "🔧 MeGo API - RDS Database Configuration"
echo "========================================"
echo ""

# RDS Details
RDS_ENDPOINT="database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com"
RDS_USERNAME="postgres"
RDS_PORT="5432"
REGION="us-east-1"

echo "📋 RDS Details:"
echo "   Endpoint: $RDS_ENDPOINT"
echo "   Username: $RDS_USERNAME"
echo "   Port: $RDS_PORT"
echo "   Region: $REGION"
echo ""

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "❌ AWS CLI not found!"
    echo "Please install AWS CLI first: https://aws.amazon.com/cli/"
    exit 1
fi

# Check if jq is installed
if ! command -v jq &> /dev/null; then
    echo "⚠️  jq not found. Installing via brew..."
    if command -v brew &> /dev/null; then
        brew install jq
    else
        echo "Please install jq manually: https://stedolan.github.io/jq/download/"
        exit 1
    fi
fi

echo "🔑 Retrieving password from AWS Secrets Manager..."
echo ""

# Try to get password from Secrets Manager
# Common secret names for RDS
SECRET_NAMES=(
    "rds-db-credentials/database-1"
    "rds!database-1"
    "database-1-credentials"
)

PASSWORD=""
for SECRET_NAME in "${SECRET_NAMES[@]}"; do
    echo "Trying secret: $SECRET_NAME..."
    SECRET_VALUE=$(aws secretsmanager get-secret-value \
        --secret-id "$SECRET_NAME" \
        --region "$REGION" \
        --query SecretString \
        --output text 2>/dev/null)
    
    if [ $? -eq 0 ] && [ ! -z "$SECRET_VALUE" ]; then
        # Try to parse as JSON
        PASSWORD=$(echo "$SECRET_VALUE" | jq -r '.password // .Password // empty' 2>/dev/null)
        if [ -z "$PASSWORD" ]; then
            # If not JSON, might be plain text
            PASSWORD="$SECRET_VALUE"
        fi
        echo "✅ Found password in secret: $SECRET_NAME"
        break
    fi
done

if [ -z "$PASSWORD" ]; then
    echo ""
    echo "⚠️  Could not retrieve password automatically."
    echo ""
    echo "Please get the password manually:"
    echo "1. Go to AWS Secrets Manager: https://console.aws.amazon.com/secretsmanager/"
    echo "2. Find the secret for your RDS database"
    echo "3. Retrieve the secret value"
    echo "4. Copy the password"
    echo ""
    read -sp "Enter RDS password: " PASSWORD
    echo ""
fi

# Database name selection
echo ""
echo "Select database name:"
echo "1) mego_dev (Development)"
echo "2) mego_prod (Production)"
echo "3) Custom"
read -p "Choice [1-3]: " DB_CHOICE

case $DB_CHOICE in
    1)
        DB_NAME="mego_dev"
        ;;
    2)
        DB_NAME="mego_prod"
        ;;
    3)
        read -p "Enter database name: " DB_NAME
        ;;
    *)
        DB_NAME="mego_dev"
        echo "Using default: mego_dev"
        ;;
esac

# Build connection string
CONNECTION_STRING="Host=$RDS_ENDPOINT;Port=$RDS_PORT;Database=$DB_NAME;Username=$RDS_USERNAME;Password=$PASSWORD;SSL Mode=Require;Trust Server Certificate=true"

echo ""
echo "✅ Connection String Generated:"
echo "=================================="
echo "$CONNECTION_STRING"
echo ""

# Option to update appsettings.json
read -p "Update appsettings.json? [y/N]: " UPDATE_CONFIG
if [[ $UPDATE_CONFIG =~ ^[Yy]$ ]]; then
    # Backup existing config
    if [ -f "appsettings.json" ]; then
        cp appsettings.json appsettings.json.backup
        echo "✅ Backed up existing appsettings.json"
    fi
    
    # Update connection string in appsettings.json
    if command -v python3 &> /dev/null; then
        python3 << EOF
import json
import sys

try:
    with open('appsettings.json', 'r') as f:
        config = json.load(f)
    
    config['ConnectionStrings']['DefaultConnection'] = '$CONNECTION_STRING'
    
    with open('appsettings.json', 'w') as f:
        json.dump(config, f, indent=2)
    
    print("✅ Updated appsettings.json")
except Exception as e:
    print(f"❌ Error updating appsettings.json: {e}")
    sys.exit(1)
EOF
    else
        echo "⚠️  Python3 not found. Please update appsettings.json manually:"
        echo ""
        echo "Add this to ConnectionStrings section:"
        echo "  \"DefaultConnection\": \"$CONNECTION_STRING\""
    fi
fi

# Option to set as environment variable
echo ""
read -p "Set as environment variable for current session? [y/N]: " SET_ENV
if [[ $SET_ENV =~ ^[Yy]$ ]]; then
    export ConnectionStrings__DefaultConnection="$CONNECTION_STRING"
    echo "✅ Environment variable set for current session"
    echo ""
    echo "To make it permanent, add to ~/.zshrc:"
    echo "export ConnectionStrings__DefaultConnection=\"$CONNECTION_STRING\""
fi

# Test connection
echo ""
read -p "Test database connection? [y/N]: " TEST_CONN
if [[ $TEST_CONN =~ ^[Yy]$ ]]; then
    echo ""
    echo "🔍 Testing connection..."
    
    if command -v psql &> /dev/null; then
        PGPASSWORD="$PASSWORD" psql -h "$RDS_ENDPOINT" -U "$RDS_USERNAME" -d "$DB_NAME" -c "SELECT version();" 2>&1
        if [ $? -eq 0 ]; then
            echo ""
            echo "✅ Connection successful!"
        else
            echo ""
            echo "❌ Connection failed. Check:"
            echo "   - Security group allows your IP"
            echo "   - Database exists"
            echo "   - Credentials are correct"
        fi
    else
        echo "⚠️  psql not found. Install PostgreSQL client to test connection."
    fi
fi

echo ""
echo "📝 Next Steps:"
echo "1. Run database migrations: dotnet ef database update"
echo "2. Start the API: dotnet run"
echo "3. Test health endpoint: curl http://localhost:5144/health/detailed"
echo ""
echo "✅ Configuration complete!"

