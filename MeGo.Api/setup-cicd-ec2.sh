#!/bin/bash

# CI/CD Setup Script for EC2
# Run this on your EC2 instance to prepare it for CI/CD

set -e

echo "🚀 MeGo API - CI/CD Setup for EC2"
echo "=================================="
echo ""

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Configuration
APP_DIR="/home/ubuntu/mego-api"
APP_PATH="$APP_DIR/MeGo.Api"
SERVICE_NAME="mego-api"

echo "📋 Step 1: Installing Basic Tools..."
echo "===================================="

# Update system
sudo apt update && sudo apt upgrade -y

# Install essential tools
sudo apt install -y \
    git \
    curl \
    wget \
    unzip \
    tar \
    build-essential \
    software-properties-common \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release

echo -e "${GREEN}✅ Basic tools installed${NC}"

echo ""
echo "📋 Step 2: Installing .NET SDK 8.0..."
echo "====================================="

if ! command -v dotnet &> /dev/null; then
    wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    /tmp/dotnet-install.sh --channel 8.0
    
    # Add to PATH
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
    echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc
    export DOTNET_ROOT=$HOME/.dotnet
    export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
    
    echo -e "${GREEN}✅ .NET SDK 8.0 installed${NC}"
else
    echo -e "${GREEN}✅ .NET SDK already installed${NC}"
fi

echo ""
echo "📋 Step 3: Installing PostgreSQL Client..."
echo "=========================================="

# Install PostgreSQL CLIENT only (NOT server - we use RDS!)
# Client is needed for:
#   - Testing RDS connection
#   - Running migrations (dotnet ef database update)
#   - Database management and debugging
# The application connects to AWS RDS, not local PostgreSQL
if ! command -v psql &> /dev/null; then
    sudo apt install -y postgresql-client
    echo -e "${GREEN}✅ PostgreSQL client installed (for RDS connection)${NC}"
else
    echo -e "${GREEN}✅ PostgreSQL client already installed${NC}"
fi

echo ""
echo "📋 Step 4: Installing Nginx..."
echo "=============================="

if ! command -v nginx &> /dev/null; then
    sudo apt install -y nginx
    sudo systemctl enable nginx
    echo -e "${GREEN}✅ Nginx installed${NC}"
else
    echo -e "${GREEN}✅ Nginx already installed${NC}"
fi

echo ""
echo "📋 Step 5: Setting Up Application Directory..."
echo "=============================================="

# Create app directory
mkdir -p "$APP_DIR"
mkdir -p "$APP_PATH"

# Create deployment directories
mkdir -p "$APP_DIR/deployments"
mkdir -p "$APP_DIR/backups"

echo -e "${GREEN}✅ Directories created${NC}"

echo ""
echo "📋 Step 6: Configuring SSH for CI/CD..."
echo "======================================="

# Ensure SSH is properly configured
sudo systemctl enable ssh
sudo systemctl start ssh

# Create .ssh directory if it doesn't exist
mkdir -p ~/.ssh
chmod 700 ~/.ssh

echo -e "${GREEN}✅ SSH configured${NC}"

echo ""
echo "📋 Step 7: Setting Up Systemd Service..."
echo "======================================="

# Create systemd service file
sudo tee /etc/systemd/system/$SERVICE_NAME.service > /dev/null <<EOF
[Unit]
Description=MeGo API Backend
After=network.target

[Service]
Type=notify
User=ubuntu
WorkingDirectory=$APP_PATH/app_current
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5144
Environment=DOTNET_ROOT=$HOME/.dotnet
Environment=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:$HOME/.dotnet:$HOME/.dotnet/tools
ExecStart=$HOME/.dotnet/dotnet $APP_PATH/app_current/MeGo.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=$SERVICE_NAME

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload

echo -e "${GREEN}✅ Systemd service configured${NC}"

echo ""
echo "📋 Step 8: Configuring Nginx..."
echo "==============================="

EC2_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4 2>/dev/null || echo "localhost")

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
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t && sudo systemctl restart nginx

echo -e "${GREEN}✅ Nginx configured${NC}"

echo ""
echo "📋 Step 9: Setting Up Firewall..."
echo "=================================="

# Configure UFW
sudo ufw --force enable
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 5144/tcp

echo -e "${GREEN}✅ Firewall configured${NC}"

echo ""
echo "📋 Step 10: Creating Deployment Helper Script..."
echo "==============================================="

# Create deployment helper script
cat > "$APP_DIR/deploy.sh" <<'DEPLOY_SCRIPT'
#!/bin/bash
# Deployment helper script

APP_PATH="/home/ubuntu/mego-api/MeGo.Api"
SERVICE_NAME="mego-api"

cd "$APP_PATH"

# Stop service
sudo systemctl stop $SERVICE_NAME || true

# Backup current
if [ -d "app_current" ]; then
    BACKUP_DIR="backups/app_$(date +%Y%m%d_%H%M%S)"
    mkdir -p backups
    cp -r app_current "$BACKUP_DIR"
fi

# Extract new version
if [ -f "deployment.tar.gz" ]; then
    mkdir -p app_new
    tar -xzf deployment.tar.gz -C app_new
    
    # Switch versions
    if [ -d "app_current" ]; then
        rm -rf app_current
    fi
    mv app_new app_current
    
    # Start service
    sudo systemctl start $SERVICE_NAME
    sleep 5
    
    # Check status
    if sudo systemctl is-active --quiet $SERVICE_NAME; then
        echo "✅ Deployment successful!"
    else
        echo "❌ Deployment failed. Restoring backup..."
        if [ -d "$BACKUP_DIR" ]; then
            rm -rf app_current
            mv "$BACKUP_DIR" app_current
            sudo systemctl start $SERVICE_NAME
        fi
        exit 1
    fi
else
    echo "❌ deployment.tar.gz not found"
    exit 1
fi
DEPLOY_SCRIPT

chmod +x "$APP_DIR/deploy.sh"

echo -e "${GREEN}✅ Deployment script created${NC}"

echo ""
echo "✅ CI/CD Setup Complete!"
echo "========================"
echo ""
echo "📋 Summary:"
echo "  ✅ .NET SDK 8.0 installed"
echo "  ✅ PostgreSQL client installed"
echo "  ✅ Nginx installed and configured"
echo "  ✅ Systemd service configured"
echo "  ✅ Firewall configured"
echo "  ✅ Deployment scripts ready"
echo ""
echo "📝 Next Steps:"
echo "  1. Configure GitHub Secrets:"
echo "     - EC2_HOST (your EC2 public IP)"
echo "     - EC2_USER (ubuntu)"
echo "     - EC2_SSH_KEY (your EC2 private key)"
echo "     - EC2_APP_PATH (/home/ubuntu/mego-api/MeGo.Api)"
echo ""
echo "  2. Push code to GitHub"
echo "  3. GitHub Actions will automatically deploy on push to main"
echo ""
echo "🔧 Manual Deployment:"
echo "  cd $APP_DIR"
echo "  ./deploy.sh"
echo ""
echo "📊 Check Status:"
echo "  sudo systemctl status $SERVICE_NAME"
echo "  sudo journalctl -u $SERVICE_NAME -f"
echo ""

