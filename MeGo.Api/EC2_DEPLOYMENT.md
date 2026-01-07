# 🚀 EC2 Deployment Guide for MeGo API

Complete guide to deploy MeGo API backend to AWS EC2.

---

## 📋 Prerequisites

- AWS Account with EC2 access
- RDS Database configured (database-1)
- EC2 Instance (Ubuntu 22.04 LTS recommended)
- SSH access to EC2 instance
- AWS CLI configured (optional)

---

## 🏗️ Step 1: Launch EC2 Instance

### Via AWS Console:

1. Go to **EC2 Console** → **Launch Instance**
2. **Name**: `mego-api-backend`
3. **AMI**: Ubuntu Server 22.04 LTS (Free tier eligible)
4. **Instance Type**: `t3.micro` (free tier) or `t3.small` (recommended)
5. **Key Pair**: Create or select existing key pair
6. **Network Settings**:
   - VPC: Same as RDS (`vpc-0adb29a28ef24ac66`)
   - Subnet: Any public subnet
   - Auto-assign Public IP: **Enable**
   - Security Group: Create new or use existing
     - Allow SSH (port 22) from your IP
     - Allow HTTP (port 80) from anywhere
     - Allow HTTPS (port 443) from anywhere
     - Allow Custom TCP (port 5144) from anywhere (or specific IPs)
7. **Storage**: 20 GB (minimum)
8. Click **"Launch Instance"**

### Security Group Rules:

```
Inbound Rules:
- SSH (22) - Your IP
- HTTP (80) - 0.0.0.0/0
- HTTPS (443) - 0.0.0.0.0/0
- Custom TCP (5144) - 0.0.0.0/0 (or specific IPs)

Outbound Rules:
- All traffic - 0.0.0.0/0
```

---

## 🔧 Step 2: Connect to EC2 Instance

### Via SSH:

```bash
ssh -i your-key.pem ubuntu@YOUR_EC2_PUBLIC_IP
```

Replace:
- `your-key.pem` - Your EC2 key pair file
- `YOUR_EC2_PUBLIC_IP` - Your EC2 instance public IP

---

## 📦 Step 3: Install Dependencies on EC2

Run these commands on your EC2 instance:

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET SDK 8.0
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
sudo ./dotnet-install.sh --channel 8.0

# Add .NET to PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc
source ~/.bashrc

# Install PostgreSQL client (for testing)
sudo apt install postgresql-client -y

# Install Nginx (for reverse proxy)
sudo apt install nginx -y

# Install Git
sudo apt install git -y

# Verify .NET installation
dotnet --version
```

---

## 📥 Step 4: Deploy Application Code

### Option A: Clone from GitHub

```bash
# Clone repository
cd /home/ubuntu
git clone YOUR_GITHUB_REPO_URL mego-api
cd mego-api/MeGo.Api

# Or if repository is private, use SSH key
```

### Option B: Upload via SCP

```bash
# From your local machine
scp -i your-key.pem -r "/Users/cybillnerd/Desktop/mego/mego-api (1)" ubuntu@YOUR_EC2_IP:/home/ubuntu/
```

### Option C: Use Deployment Script

```bash
# On EC2, run the deployment script
./deploy-to-ec2.sh
```

---

## ⚙️ Step 5: Configure Application

### 5.1: Create appsettings.Production.json

```bash
cd /home/ubuntu/mego-api/MeGo.Api
nano appsettings.Production.json
```

Add:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_prod;Username=postgres;Password=YOUR_RDS_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Key": "YOUR_PRODUCTION_JWT_SECRET_KEY_MIN_32_CHARACTERS",
    "Issuer": "mego-api",
    "Audience": "mego-clients",
    "ExpireMinutes": "60"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ASPNETCORE_ENVIRONMENT": "Production",
  "ASPNETCORE_URLS": "http://0.0.0.0:5144"
}
```

### 5.2: Set Environment Variables

```bash
# Set production environment
export ASPNETCORE_ENVIRONMENT=Production

# Set RDS connection (if using environment variables)
export ConnectionStrings__DefaultConnection="Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_prod;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

---

## 🗄️ Step 6: Configure RDS Connection

### 6.1: Update RDS Security Group

1. Go to **RDS Console** → **database-1**
2. Click **"Modify"**
3. Under **"Connectivity"**:
   - **VPC security groups**: Add your EC2 instance security group
4. Click **"Continue"** → **"Apply immediately"**

### 6.2: Test RDS Connection from EC2

```bash
# Test connection
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d postgres \
     -c "SELECT version();"
```

---

## 🗄️ Step 7: Setup Database

```bash
cd /home/ubuntu/mego-api/MeGo.Api

# Create database (if not exists)
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d postgres \
     -c "CREATE DATABASE mego_prod;"

# Run migrations
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
dotnet ef database update
```

---

## 🚀 Step 8: Run Application

### Option A: Run Directly (Testing)

```bash
cd /home/ubuntu/mego-api/MeGo.Api
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
dotnet run
```

### Option B: Run as Systemd Service (Production)

Create service file:

```bash
sudo nano /etc/systemd/system/mego-api.service
```

Add:

```ini
[Unit]
Description=MeGo API Backend
After=network.target

[Service]
Type=notify
User=ubuntu
WorkingDirectory=/home/ubuntu/mego-api/MeGo.Api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5144
Environment=DOTNET_ROOT=/home/ubuntu/.dotnet
Environment=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/home/ubuntu/.dotnet:/home/ubuntu/.dotnet/tools
ExecStart=/home/ubuntu/.dotnet/dotnet /home/ubuntu/mego-api/MeGo.Api/MeGo.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=mego-api

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable mego-api
sudo systemctl start mego-api
sudo systemctl status mego-api
```

---

## 🌐 Step 9: Configure Nginx Reverse Proxy

### 9.1: Create Nginx Configuration

```bash
sudo nano /etc/nginx/sites-available/mego-api
```

Add:

```nginx
server {
    listen 80;
    server_name YOUR_EC2_PUBLIC_IP_OR_DOMAIN;

    location / {
        proxy_pass http://localhost:5144;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 9.2: Enable Site

```bash
sudo ln -s /etc/nginx/sites-available/mego-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

---

## ✅ Step 10: Verify Deployment

### Test Endpoints:

```bash
# Health check
curl http://localhost:5144/health

# Detailed health
curl http://localhost:5144/health/detailed

# Via Nginx
curl http://YOUR_EC2_IP/health
```

### Check Logs:

```bash
# Application logs
tail -f /home/ubuntu/mego-api/MeGo.Api/logs/app-*.log

# Systemd logs
sudo journalctl -u mego-api -f

# Nginx logs
sudo tail -f /var/log/nginx/access.log
```

---

## 🔒 Step 11: Security Hardening

### Firewall (UFW):

```bash
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

### SSL Certificate (Let's Encrypt):

```bash
sudo apt install certbot python3-certbot-nginx -y
sudo certbot --nginx -d your-domain.com
```

---

## 📊 Monitoring

### View Application Status:

```bash
sudo systemctl status mego-api
```

### View Logs:

```bash
# Application logs
tail -f /home/ubuntu/mego-api/MeGo.Api/logs/app-*.log

# System logs
sudo journalctl -u mego-api -n 100 -f
```

### Restart Application:

```bash
sudo systemctl restart mego-api
```

---

## 🔄 Updates and Deployment

### Update Application:

```bash
cd /home/ubuntu/mego-api
git pull origin main
cd MeGo.Api
dotnet publish -c Release -o /home/ubuntu/mego-api/publish
sudo systemctl restart mego-api
```

---

## 🐛 Troubleshooting

### Application Not Starting:

```bash
# Check logs
sudo journalctl -u mego-api -n 50

# Check if port is in use
sudo lsof -i :5144

# Test connection manually
cd /home/ubuntu/mego-api/MeGo.Api
dotnet run
```

### Database Connection Issues:

```bash
# Test RDS connection
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com -U postgres

# Check security group
# Ensure EC2 security group is added to RDS security group
```

### Nginx Issues:

```bash
# Test configuration
sudo nginx -t

# Check logs
sudo tail -f /var/log/nginx/error.log
```

---

## 📝 Quick Deployment Checklist

- [ ] EC2 instance launched
- [ ] .NET SDK installed
- [ ] Application code deployed
- [ ] appsettings.Production.json configured
- [ ] RDS security group updated
- [ ] Database created and migrations run
- [ ] Systemd service created and started
- [ ] Nginx configured and running
- [ ] Health check endpoints working
- [ ] Logs accessible

---

**Your backend is now running on EC2! 🎉**

