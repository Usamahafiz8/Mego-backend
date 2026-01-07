# 🚀 EC2 Deployment - Step by Step Guide

## Prerequisites
- ✅ EC2 instance running Ubuntu
- ✅ .NET SDK 8.0 installed
- ✅ Nginx installed
- ✅ Git installed
- ✅ RDS password ready

---

## Step 1: Connect to EC2

```bash
ssh -i your-key.pem ubuntu@your-ec2-ip
```

---

## Step 2: Clone/Pull Your Code

```bash
# If you haven't cloned yet:
cd ~
git clone https://github.com/your-username/your-repo.git Mego-backend
cd Mego-backend/MeGo.Api

# OR if you already have the code:
cd ~/Mego-backend/MeGo.Api
git pull origin main
```

---

## Step 3: Create Production Config

```bash
# Copy example to production config
cp appsettings.Production.json.example appsettings.Production.json

# Edit with your RDS credentials
nano appsettings.Production.json
```

**In nano editor, update:**
1. `ConnectionStrings.DefaultConnection` - Replace `YOUR_PASSWORD` with your RDS password
2. `Jwt.Key` - Replace with a strong production JWT key (min 32 characters)
3. Save: `Ctrl+X`, then `Y`, then `Enter`

**Example connection string:**
```json
"DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_prod;Username=postgres;Password=YOUR_ACTUAL_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

---

## Step 4: Create Application Directory

```bash
sudo mkdir -p /var/www/mego
sudo chown ubuntu:ubuntu /var/www/mego
```

---

## Step 5: Publish Application

```bash
# Make sure you're in the project directory
cd ~/Mego-backend/MeGo.Api

# Publish to /var/www/mego
dotnet publish -c Release -o /var/www/mego

# Verify appsettings.Production.json was copied
ls -la /var/www/mego/appsettings*.json
```

---

## Step 6: Run Database Migrations

```bash
cd /var/www/mego

# Set environment
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

# Run migrations (creates database if needed)
dotnet ef database update
```

**Note:** If `dotnet ef` is not found, install EF tools:
```bash
dotnet tool install --global dotnet-ef
```

---

## Step 7: Test Run (Optional)

```bash
cd /var/www/mego
ASPNETCORE_ENVIRONMENT=Production dotnet MeGo.Api.dll
```

**Press `Ctrl+C` to stop after verifying it starts.**

---

## Step 8: Create Systemd Service

```bash
sudo nano /etc/systemd/system/mego-api.service
```

**Paste this configuration:**

```ini
[Unit]
Description=MeGo API Backend
After=network.target

[Service]
Type=notify
User=ubuntu
WorkingDirectory=/var/www/mego
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5144
Environment=DOTNET_ROOT=/home/ubuntu/.dotnet
Environment=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/home/ubuntu/.dotnet:/home/ubuntu/.dotnet/tools
ExecStart=/home/ubuntu/.dotnet/dotnet /var/www/mego/MeGo.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=mego-api

[Install]
WantedBy=multi-user.target
```

**Save and exit:** `Ctrl+X`, then `Y`, then `Enter`

---

## Step 9: Start the Service

```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable service (start on boot)
sudo systemctl enable mego-api

# Start service
sudo systemctl start mego-api

# Check status
sudo systemctl status mego-api
```

---

## Step 10: Configure Nginx

```bash
# Get your EC2 public IP
EC2_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)
echo "Your EC2 IP: $EC2_IP"

# Create Nginx config
sudo nano /etc/nginx/sites-available/mego-api
```

**Paste this configuration:**

```nginx
server {
    listen 80;
    server_name $EC2_IP;  # Replace with your domain if you have one

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

**Save and exit:** `Ctrl+X`, then `Y`, then `Enter`

```bash
# Enable site
sudo ln -sf /etc/nginx/sites-available/mego-api /etc/nginx/sites-enabled/

# Remove default site (optional)
sudo rm /etc/nginx/sites-enabled/default

# Test Nginx config
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx
```

---

## Step 11: Configure Firewall (Security Group)

**In AWS Console:**
1. Go to EC2 → Security Groups
2. Select your EC2 security group
3. Add inbound rules:
   - **Type:** HTTP, **Port:** 80, **Source:** 0.0.0.0/0
   - **Type:** HTTPS, **Port:** 443, **Source:** 0.0.0.0/0 (if using SSL)
   - **Type:** Custom TCP, **Port:** 5144, **Source:** Your IP (for direct API access)

---

## Step 12: Verify Deployment

```bash
# Check service status
sudo systemctl status mego-api

# Check logs
sudo journalctl -u mego-api -f

# Test health endpoint
curl http://localhost:5144/health

# Test from outside (replace with your EC2 IP)
curl http://YOUR_EC2_IP/health
```

---

## Step 13: Access Your API

- **Health Check:** `http://YOUR_EC2_IP/health`
- **Swagger UI:** `http://YOUR_EC2_IP/swagger` (if enabled)
- **API Base:** `http://YOUR_EC2_IP/api`

---

## Useful Commands

```bash
# View service logs
sudo journalctl -u mego-api -f

# Restart service
sudo systemctl restart mego-api

# Stop service
sudo systemctl stop mego-api

# View application logs
tail -f /var/www/mego/logs/app-*.log

# Check Nginx status
sudo systemctl status nginx

# Test Nginx config
sudo nginx -t
```

---

## Troubleshooting

### Service won't start
```bash
# Check logs
sudo journalctl -u mego-api -n 50

# Check if port is in use
sudo netstat -tlnp | grep 5144
```

### Database connection failed
- Verify RDS security group allows EC2 security group
- Check connection string in `appsettings.Production.json`
- Test connection: `dotnet ef database update --dry-run`

### Nginx 502 Bad Gateway
- Check if service is running: `sudo systemctl status mego-api`
- Check service logs: `sudo journalctl -u mego-api -f`

---

## ✅ Deployment Complete!

Your API should now be accessible at:
- `http://YOUR_EC2_IP/health`
- `http://YOUR_EC2_IP/swagger`
