#!/bin/bash

# EC2 Deployment Script for MeGo API
# Run this script on your EC2 instance

set -e

echo "🚀 MeGo API - EC2 Deployment Script"
echo "===================================="
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
APP_DIR="/home/ubuntu/mego-api/MeGo.Api"
SERVICE_NAME="mego-api"
RDS_ENDPOINT="database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com"

# Check if running as root
if [ "$EUID" -eq 0 ]; then 
    echo -e "${RED}❌ Please don't run as root. Use ubuntu user.${NC}"
    exit 1
fi

echo "📋 Step 1: Installing Dependencies..."
echo "====================================="

# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET SDK if not installed
if ! command -v dotnet &> /dev/null; then
    echo "Installing .NET SDK 8.0..."
    wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    /tmp/dotnet-install.sh --channel 8.0
    
    # Add to PATH
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
    echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc
    export DOTNET_ROOT=$HOME/.dotnet
    export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
else
    echo -e "${GREEN}✅ .NET SDK already installed${NC}"
fi

# PostgreSQL client NOT needed - EF Core connects directly to RDS via .NET/Npgsql
echo "⏭️  Skipping PostgreSQL client (not needed - EF Core handles RDS connection)"

# Install Nginx
if ! command -v nginx &> /dev/null; then
    echo "Installing Nginx..."
    sudo apt install nginx -y
else
    echo -e "${GREEN}✅ Nginx already installed${NC}"
fi

echo ""
echo "📋 Step 2: Setting Up Application Directory..."
echo "=============================================="

# Create app directory
mkdir -p "$APP_DIR"
cd "$APP_DIR" || exit 1

echo -e "${GREEN}✅ Application directory ready${NC}"

echo ""
echo "📋 Step 3: Configuring Application..."
echo "====================================="

# Check if appsettings.Production.json exists
if [ ! -f "appsettings.Production.json" ]; then
    echo -e "${YELLOW}⚠️  appsettings.Production.json not found${NC}"
    echo "Creating from example..."
    
    if [ -f "appsettings.Production.json.example" ]; then
        cp appsettings.Production.json.example appsettings.Production.json
        echo -e "${YELLOW}⚠️  Please edit appsettings.Production.json with your RDS credentials${NC}"
    else
        echo -e "${RED}❌ appsettings.Production.json.example not found${NC}"
        exit 1
    fi
fi

echo ""
read -p "Enter RDS password: " -s RDS_PASSWORD
echo ""

# Update connection string in appsettings.Production.json
if command -v python3 &> /dev/null; then
    python3 << EOF
import json
import sys

try:
    with open('appsettings.Production.json', 'r') as f:
        config = json.load(f)
    
    config['ConnectionStrings']['DefaultConnection'] = \
        f"Host=$RDS_ENDPOINT;Port=5432;Database=mego_prod;Username=postgres;Password=$RDS_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
    
    with open('appsettings.Production.json', 'w') as f:
        json.dump(config, f, indent=2)
    
    print("✅ Updated appsettings.Production.json")
except Exception as e:
    print(f"❌ Error: {e}")
    sys.exit(1)
EOF
else
    echo -e "${YELLOW}⚠️  Python3 not found. Please update appsettings.Production.json manually${NC}"
fi

echo ""
echo "📋 Step 4: Building Application..."
echo "=================================="

export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

dotnet restore
dotnet build -c Release
dotnet publish -c Release -o ./publish

echo -e "${GREEN}✅ Application built successfully${NC}"

echo ""
echo "📋 Step 5: Setting Up Database..."
echo "================================="

# Run migrations - EF Core connects directly to RDS via .NET/Npgsql
# Will create database automatically if it doesn't exist
# No psql needed - everything via .NET!
echo "Running migrations (connects directly to RDS via .NET)..."
dotnet ef database update

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Database migrations completed${NC}"
else
    echo -e "${RED}❌ Migration failed. Check RDS connection and credentials${NC}"
    exit 1
fi

echo ""
echo "📋 Step 6: Creating Systemd Service..."
echo "====================================="

sudo tee /etc/systemd/system/$SERVICE_NAME.service > /dev/null <<EOF
[Unit]
Description=MeGo API Backend
After=network.target

[Service]
Type=notify
User=ubuntu
WorkingDirectory=$APP_DIR
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5144
Environment=DOTNET_ROOT=$HOME/.dotnet
Environment=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:$HOME/.dotnet:$HOME/.dotnet/tools
ExecStart=$HOME/.dotnet/dotnet $APP_DIR/MeGo.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=$SERVICE_NAME

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME
sudo systemctl start $SERVICE_NAME

echo -e "${GREEN}✅ Systemd service created and started${NC}"

echo ""
echo "📋 Step 7: Configuring Nginx..."
echo "==============================="

EC2_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)

sudo tee /etc/nginx/sites-available/$SERVICE_NAME > /dev/null <<EOF
server {
    listen 80;
    server_name $EC2_IP;

    location / {
        proxy_pass http://localhost:5144;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF

sudo ln -sf /etc/nginx/sites-available/$SERVICE_NAME /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl restart nginx

echo -e "${GREEN}✅ Nginx configured${NC}"

echo ""
echo "📋 Step 8: Verifying Deployment..."
echo "=================================="

sleep 5

# Check service status
if sudo systemctl is-active --quiet $SERVICE_NAME; then
    echo -e "${GREEN}✅ Service is running${NC}"
else
    echo -e "${RED}❌ Service is not running${NC}"
    sudo systemctl status $SERVICE_NAME
    exit 1
fi

# Test health endpoint
HEALTH_CHECK=$(curl -s http://localhost:5144/health 2>/dev/null)
if [ ! -z "$HEALTH_CHECK" ]; then
    echo -e "${GREEN}✅ Health check passed${NC}"
else
    echo -e "${YELLOW}⚠️  Health check failed. Check logs${NC}"
fi

echo ""
echo "✅ Deployment Complete!"
echo "======================"
echo ""
echo "🌐 Access your API:"
echo "   http://$EC2_IP/health"
echo "   http://$EC2_IP/swagger"
echo ""
echo "📊 Useful Commands:"
echo "   sudo systemctl status $SERVICE_NAME"
echo "   sudo journalctl -u $SERVICE_NAME -f"
echo "   tail -f $APP_DIR/logs/app-*.log"
echo ""
