#!/bin/bash

# Deploy to EC2 from your local machine
# This script uploads code and runs deployment on EC2

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo "🚀 Deploying MeGo API to EC2..."
echo ""

# Configuration
read -p "EC2 Host (IP or domain): " EC2_HOST
read -p "EC2 User [ubuntu]: " EC2_USER
EC2_USER=${EC2_USER:-ubuntu}
read -p "SSH Key Path: " SSH_KEY
read -p "EC2 Project Path [~/Mego-backend]: " EC2_PATH
EC2_PATH=${EC2_PATH:-~/Mego-backend}

# Check if appsettings.Production.json exists
if [ ! -f "appsettings.Production.json" ]; then
    echo -e "${YELLOW}⚠️  appsettings.Production.json not found${NC}"
    echo "Running prepare-production-config.sh..."
    ./prepare-production-config.sh
fi

# Create deployment package
echo ""
echo "📦 Creating deployment package..."
TEMP_DIR=$(mktemp -d)
cp -r . "$TEMP_DIR/MeGo.Api"
cd "$TEMP_DIR"

# Remove unnecessary files
rm -rf MeGo.Api/bin MeGo.Api/obj MeGo.Api/.vs MeGo.Api/.vscode
rm -f MeGo.Api/*.md MeGo.Api/*.sh 2>/dev/null || true

# Create tarball
tar czf deploy.tar.gz MeGo.Api/

echo "✅ Package created: deploy.tar.gz"

# Upload to EC2
echo ""
echo "📤 Uploading to EC2..."
scp -i "$SSH_KEY" deploy.tar.gz ${EC2_USER}@${EC2_HOST}:~/

# Run deployment on EC2
echo ""
echo "🔧 Running deployment on EC2..."
ssh -i "$SSH_KEY" ${EC2_USER}@${EC2_HOST} << 'REMOTE_SCRIPT'
set -e

# Extract
cd ~
tar xzf deploy.tar.gz

# Move to project directory
mkdir -p Mego-backend
rm -rf Mego-backend/MeGo.Api
mv MeGo.Api Mego-backend/

# Create app directory
sudo mkdir -p /var/www/mego
sudo chown ubuntu:ubuntu /var/www/mego

# Publish
cd Mego-backend/MeGo.Api
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
dotnet publish -c Release -o /var/www/mego

# Copy config
cp appsettings.Production.json /var/www/mego/ 2>/dev/null || echo "Config already in place"

# Run migrations
cd /var/www/mego
dotnet tool install --global dotnet-ef 2>/dev/null || true
export ASPNETCORE_ENVIRONMENT=Production
dotnet ef database update || echo "Migrations may have failed - check manually"

# Restart service if exists
sudo systemctl restart mego-api 2>/dev/null || echo "Service not configured yet"

echo "✅ Deployment complete on EC2!"
REMOTE_SCRIPT

# Cleanup
rm -rf "$TEMP_DIR"

echo ""
echo -e "${GREEN}✅ Deployment complete!${NC}"
echo ""
echo "Next steps on EC2:"
echo "  1. Configure systemd service (if not done): sudo nano /etc/systemd/system/mego-api.service"
echo "  2. Start service: sudo systemctl start mego-api"
echo "  3. Configure Nginx (if not done)"
