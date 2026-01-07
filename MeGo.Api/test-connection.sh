#!/bin/bash

# Test RDS Database Connection Script

echo "🔍 Testing RDS Database Connection"
echo "==================================="
echo ""

cd "$(dirname "$0")"

# Check if appsettings.json exists
if [ ! -f "appsettings.json" ]; then
    echo "❌ appsettings.json not found!"
    exit 1
fi

# Extract connection string
CONN_STRING=$(grep -A 1 "ConnectionStrings" appsettings.json | grep "DefaultConnection" | cut -d'"' -f4)

if [ -z "$CONN_STRING" ]; then
    echo "❌ Connection string not found in appsettings.json"
    exit 1
fi

echo "📋 Connection String Found"
echo ""

# Parse connection string
HOST=$(echo "$CONN_STRING" | grep -oP 'Host=\K[^;]+' || echo "")
PORT=$(echo "$CONN_STRING" | grep -oP 'Port=\K[^;]+' || echo "5432")
USER=$(echo "$CONN_STRING" | grep -oP 'Username=\K[^;]+' || echo "")
DB=$(echo "$CONN_STRING" | grep -oP 'Database=\K[^;]+' || echo "")
PASSWORD=$(echo "$CONN_STRING" | grep -oP 'Password=\K[^;]+' || echo "")

echo "Endpoint: $HOST"
echo "Port: $PORT"
echo "Username: $USER"
echo "Database: $DB"
echo "Password: ${PASSWORD:0:3}***" # Show only first 3 chars
echo ""

# Test 1: Check if it's RDS endpoint
if [[ "$HOST" == *"rds.amazonaws.com"* ]]; then
    echo "✅ RDS endpoint detected"
else
    echo "⚠️  Not an RDS endpoint (might be local)"
fi

echo ""

# Test 2: Test connection via psql
if command -v psql &> /dev/null; then
    echo "🧪 Testing connection with psql..."
    echo ""
    
    if [ -z "$PASSWORD" ]; then
        echo "⚠️  No password in connection string"
        echo "Attempting connection without password..."
        psql -h "$HOST" -p "$PORT" -U "$USER" -d "$DB" -c "SELECT version();" 2>&1
    else
        PGPASSWORD="$PASSWORD" psql -h "$HOST" -p "$PORT" -U "$USER" -d "$DB" -c "SELECT version();" 2>&1
    fi
    
    CONN_RESULT=$?
    
    if [ $CONN_RESULT -eq 0 ]; then
        echo ""
        echo "✅ Connection successful!"
        echo ""
        echo "📊 Database Info:"
        PGPASSWORD="$PASSWORD" psql -h "$HOST" -p "$PORT" -U "$USER" -d "$DB" -c "\conninfo" 2>/dev/null
    else
        echo ""
        echo "❌ Connection failed!"
        echo ""
        echo "Common issues:"
        echo "  1. Password not set in RDS"
        echo "  2. Public access not enabled"
        echo "  3. Security group doesn't allow port 5432"
        echo "  4. Database doesn't exist"
        echo ""
        echo "See RDS_SETUP_GUIDE.md for troubleshooting"
    fi
else
    echo "⚠️  psql not installed"
    echo "Install PostgreSQL client to test connection:"
    echo "  brew install postgresql"
fi

echo ""

# Test 3: Test via API health check
echo "🧪 Testing via API health check..."
echo ""

API_RUNNING=$(lsof -ti:5144 2>/dev/null)

if [ ! -z "$API_RUNNING" ]; then
    echo "✅ API is running on port 5144"
    echo ""
    HEALTH_RESPONSE=$(curl -s http://localhost:5144/health/detailed 2>/dev/null)
    
    if [ ! -z "$HEALTH_RESPONSE" ]; then
        echo "$HEALTH_RESPONSE" | python3 -m json.tool 2>/dev/null | grep -A 5 "database\|Database" || echo "$HEALTH_RESPONSE"
        
        # Check if database is healthy
        if echo "$HEALTH_RESPONSE" | grep -qi "healthy"; then
            echo ""
            echo "✅ Database connection is healthy!"
        else
            echo ""
            echo "⚠️  Database might not be connected"
        fi
    else
        echo "❌ Could not reach health endpoint"
    fi
else
    echo "⚠️  API is not running"
    echo "Start the API first: ./start.sh"
fi

echo ""
echo "📝 Summary:"
echo "==========="
echo "Connection String: Configured"
echo "Endpoint: $HOST"
echo "Database: $DB"
echo ""

