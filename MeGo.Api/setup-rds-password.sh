#!/bin/bash

# RDS Password Setup Script
# This script helps you set up the RDS database password

echo "🔐 RDS Password Setup"
echo "====================="
echo ""

RDS_IDENTIFIER="database-1"
REGION="us-east-1"

echo "📋 RDS Details:"
echo "   Identifier: $RDS_IDENTIFIER"
echo "   Region: $REGION"
echo "   Endpoint: database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com"
echo ""

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "❌ AWS CLI not found!"
    echo "Please install AWS CLI: https://aws.amazon.com/cli/"
    exit 1
fi

echo "🔑 Setting up RDS Password..."
echo ""
echo "Option 1: Set password via AWS CLI (recommended)"
echo "Option 2: Instructions for AWS Console"
echo ""
read -p "Choose option [1-2]: " OPTION

case $OPTION in
    1)
        echo ""
        read -sp "Enter new master password: " NEW_PASSWORD
        echo ""
        read -sp "Confirm password: " CONFIRM_PASSWORD
        echo ""
        
        if [ "$NEW_PASSWORD" != "$CONFIRM_PASSWORD" ]; then
            echo "❌ Passwords don't match!"
            exit 1
        fi
        
        if [ -z "$NEW_PASSWORD" ]; then
            echo "❌ Password cannot be empty!"
            exit 1
        fi
        
        echo ""
        echo "🔄 Modifying RDS instance..."
        aws rds modify-db-instance \
            --db-instance-identifier "$RDS_IDENTIFIER" \
            --master-user-password "$NEW_PASSWORD" \
            --apply-immediately \
            --region "$REGION" 2>&1
        
        if [ $? -eq 0 ]; then
            echo ""
            echo "✅ Password update initiated!"
            echo "   Note: This may take a few minutes to apply."
            echo ""
            echo "📝 Next steps:"
            echo "   1. Wait for modification to complete (check AWS Console)"
            echo "   2. Enable public access (if connecting from local)"
            echo "   3. Configure security group (allow port 5432)"
            echo "   4. Update appsettings.json with connection string"
        else
            echo ""
            echo "❌ Failed to update password. Check AWS Console for details."
        fi
        ;;
    2)
        echo ""
        echo "📋 AWS Console Steps:"
        echo "====================="
        echo ""
        echo "1. Go to: https://console.aws.amazon.com/rds/"
        echo "2. Click 'Databases' → Select 'database-1'"
        echo "3. Click 'Modify' button"
        echo "4. Scroll to 'Database authentication' section"
        echo "5. Enter new password in 'Master password' field"
        echo "6. Click 'Continue' → 'Apply immediately'"
        echo "7. Wait for modification to complete (~2-5 minutes)"
        echo ""
        echo "🔓 To Enable Public Access:"
        echo "1. In Modify page, go to 'Connectivity' section"
        echo "2. Under 'Public access', select 'Publicly accessible'"
        echo "3. Click 'Continue' → 'Apply immediately'"
        echo ""
        echo "🔒 To Configure Security Group:"
        echo "1. Go to EC2 Console → Security Groups"
        echo "2. Select: sg-0a3d1bdab1a2a4393"
        echo "3. Click 'Edit inbound rules'"
        echo "4. Add rule:"
        echo "   - Type: PostgreSQL"
        echo "   - Port: 5432"
        echo "   - Source: 0.0.0.0/0 (or your IP)"
        echo "5. Click 'Save rules'"
        ;;
    *)
        echo "Invalid option"
        exit 1
        ;;
esac

echo ""
echo "📄 See RDS_SETUP_GUIDE.md for complete instructions"

