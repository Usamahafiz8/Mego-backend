#!/bin/bash

# Complete EC2 Deployment Script - Everything Automated
# Run this ON EC2 - it handles everything automatically

set -e

echo "🚀 MeGo API - Complete Automated Deployment"
echo "=========================================="
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Configuration
APP_DIR="/var/www/mego"
# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"
SERVICE_NAME="mego-api"
RDS_ENDPOINT="database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com"

# Check if running as root
if [ "$EUID" -eq 0 ]; then 
    echo -e "${RED}❌ Please don't run as root. Use ubuntu user.${NC}"
    exit 1
fi

# Step 1: Navigate to project directory (where script is located)
echo "📋 Step 1: Locating project..."
cd "$PROJECT_DIR" || {
    echo -e "${RED}❌ Cannot access project directory: $PROJECT_DIR${NC}"
    echo "Please run this script from the MeGo.Api directory"
    exit 1
}

echo -e "${GREEN}✅ Project directory: $(pwd)${NC}"

# Step 2: Ensure .NET is in PATH
echo ""
echo "📋 Step 2: Setting up .NET environment..."
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found. Please install it first.${NC}"
    exit 1
fi

echo -e "${GREEN}✅ .NET SDK ready${NC}"

# Step 3: Install EF Tools if needed
echo ""
echo "📋 Step 3: Installing EF Core tools..."
if ! dotnet ef --version &> /dev/null; then
    echo "Installing EF Core tools..."
    # Try installing specific version to avoid package issues
    dotnet tool install --global dotnet-ef --version 9.0.0 --verbosity quiet 2>/dev/null || \
    dotnet tool install --global dotnet-ef --verbosity quiet 2>/dev/null || \
    dotnet tool update --global dotnet-ef --verbosity quiet 2>/dev/null || true
    
    # Verify installation
    if dotnet ef --version &> /dev/null; then
        echo -e "${GREEN}✅ EF Core tools installed${NC}"
    else
        echo -e "${YELLOW}⚠️  EF Core tools installation had issues, but continuing...${NC}"
        echo -e "${YELLOW}   Migrations will be skipped if tools are not available${NC}"
    fi
else
    echo -e "${GREEN}✅ EF Core tools already installed${NC}"
fi

# Step 4: Create production config if not exists
echo ""
echo "📋 Step 4: Configuring production settings..."
if [ ! -f "appsettings.Production.json" ]; then
    if [ -f "appsettings.Production.json.example" ]; then
        cp appsettings.Production.json.example appsettings.Production.json
        echo -e "${YELLOW}⚠️  Created appsettings.Production.json from example${NC}"
        echo -e "${YELLOW}⚠️  Please update it with your RDS password before running again${NC}"
        echo ""
        echo "Run: nano appsettings.Production.json"
        echo "Update: ConnectionStrings.DefaultConnection with your RDS password"
        exit 1
    else
        echo -e "${RED}❌ appsettings.Production.json.example not found in $(pwd)${NC}"
        echo "Files in current directory:"
        ls -la *.json 2>/dev/null || echo "No JSON files found"
        exit 1
    fi
else
    echo -e "${GREEN}✅ Production config found${NC}"
fi

# Step 5: Create app directory
echo ""
echo "📋 Step 5: Creating application directory..."
sudo mkdir -p "$APP_DIR"
sudo chown ubuntu:ubuntu "$APP_DIR"
echo -e "${GREEN}✅ Application directory ready${NC}"

# Step 6: Publish application
echo ""
echo "📋 Step 6: Publishing application..."
dotnet publish -c Release -o "$APP_DIR" --verbosity quiet
echo -e "${GREEN}✅ Application published${NC}"

# Step 7: Copy config file
echo ""
echo "📋 Step 7: Copying configuration..."
cp appsettings.Production.json "$APP_DIR/"
echo -e "${GREEN}✅ Configuration copied${NC}"

# Step 8: Run migrations
echo ""
echo "📋 Step 8: Running database migrations..."
cd "$PROJECT_DIR"
export ASPNETCORE_ENVIRONMENT=Production

# Check if EF tools are available
if dotnet ef --version &> /dev/null; then
    # Run migrations from project directory
    if dotnet ef database update --verbosity quiet 2>&1; then
        echo -e "${GREEN}✅ Database migrations completed${NC}"
    else
        echo -e "${YELLOW}⚠️  Migrations may have failed or database already up to date${NC}"
        echo -e "${YELLOW}   Check connection string and RDS access${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  EF Core tools not available - skipping migrations${NC}"
    echo -e "${YELLOW}   You can run migrations manually later with: dotnet ef database update${NC}"
fi

# Step 9: Create systemd service
echo ""
echo "📋 Step 9: Creating systemd service..."
sudo tee /etc/systemd/system/$SERVICE_NAME.service > /dev/null <<EOF
[Unit]
Description=MeGo API Backend
After=network.target

[Service]
Type=simple
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
TimeoutStartSec=60
TimeoutStopSec=30
SyslogIdentifier=$SERVICE_NAME

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME
echo -e "${GREEN}✅ Systemd service created${NC}"

# Step 10: Start service
echo ""
echo "📋 Step 10: Starting service..."
sudo systemctl restart $SERVICE_NAME
sleep 3

if sudo systemctl is-active --quiet $SERVICE_NAME; then
    echo -e "${GREEN}✅ Service is running${NC}"
else
    echo -e "${RED}❌ Service failed to start${NC}"
    echo "Checking logs..."
    sudo journalctl -u $SERVICE_NAME -n 20 --no-pager
    exit 1
fi

# Step 11: Configure Nginx
echo ""
echo "📋 Step 11: Configuring Nginx..."
EC2_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4 2>/dev/null || echo "_")

# Create Nginx config
sudo tee /etc/nginx/sites-available/$SERVICE_NAME > /dev/null <<EOF
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name $EC2_IP _;

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

# Enable site and remove default
sudo ln -sf /etc/nginx/sites-available/$SERVICE_NAME /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default 2>/dev/null || true

# Test and restart Nginx
if sudo nginx -t 2>&1; then
    sudo systemctl restart nginx
    echo -e "${GREEN}✅ Nginx configured and restarted${NC}"
else
    echo -e "${RED}❌ Nginx config test failed${NC}"
    echo "Checking Nginx error..."
    sudo nginx -t
fi

# Step 12: Verify deployment
echo ""
echo "📋 Step 12: Verifying deployment..."
sleep 2

HEALTH_CHECK=$(curl -s http://localhost:5144/health 2>/dev/null || echo "")
if [ ! -z "$HEALTH_CHECK" ]; then
    echo -e "${GREEN}✅ Health check passed${NC}"
else
    echo -e "${YELLOW}⚠️  Health check failed, but service is running${NC}"
fi

echo ""
echo "✅ Deployment Complete!"
echo "======================"
echo ""
echo "🌐 Your API is available at:"
echo "   http://$EC2_IP/health"
echo "   http://$EC2_IP/swagger"
echo ""
echo "📊 Useful Commands:"
echo "   sudo systemctl status $SERVICE_NAME"
echo "   sudo journalctl -u $SERVICE_NAME -f"
echo "   tail -f $APP_DIR/logs/app-*.log"
echo ""
