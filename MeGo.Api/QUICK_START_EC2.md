# 🚀 Quick Start - Your EC2 Instance

## Your EC2 Details

- **Instance ID**: `i-0f80cc6f49a5e0aa9`
- **Public IP**: `3.91.9.196`
- **Public DNS**: `ec2-3-91-9-196.compute-1.amazonaws.com`
- **Private IP**: `172.31.23.246`
- **Instance Type**: `t3.micro`
- **AMI**: Ubuntu 24.04
- **VPC**: `vpc-0adb29a28ef24ac66` (Same as RDS ✅)
- **Key Pair**: `mego-api`
- **Status**: Running ✅

---

## ⚡ Quick Setup (5 Minutes)

### Step 1: Connect to EC2

```bash
ssh -i ~/.ssh/mego-api.pem ubuntu@3.91.9.196
```

**Note**: Adjust the key path if your key is in a different location.

### Step 2: Run Setup Script

Once connected to EC2, run:

```bash
# Download setup script
curl -o setup-cicd-ec2.sh https://raw.githubusercontent.com/YOUR_REPO/main/MeGo.Api/setup-cicd-ec2.sh

# OR upload from local machine:
# scp -i ~/.ssh/mego-api.pem MeGo.Api/setup-cicd-ec2.sh ubuntu@3.91.9.196:/home/ubuntu/

# Make executable and run
chmod +x setup-cicd-ec2.sh
./setup-cicd-ec2.sh
```

This will install:
- ✅ .NET SDK 8.0
- ✅ PostgreSQL client
- ✅ Nginx
- ✅ Systemd service
- ✅ All dependencies

### Step 3: Configure RDS Connection

On EC2:

```bash
# Create production config
mkdir -p /home/ubuntu/mego-api/MeGo.Api
cd /home/ubuntu/mego-api/MeGo.Api

nano appsettings.Production.json
```

Paste this (update password):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_prod;Username=postgres;Password=YOUR_RDS_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Key": "MegoApp_SuperStrongJWTKey_2025_Production_!@#$%_VeryLongAndSecureSecretKey123456",
    "Issuer": "mego-api",
    "Audience": "mego-clients",
    "ExpireMinutes": "60"
  },
  "ASPNETCORE_ENVIRONMENT": "Production",
  "ASPNETCORE_URLS": "http://0.0.0.0:5144",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Step 4: Setup Database

```bash
# Set .NET path
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

# Create database
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d postgres \
     -c "CREATE DATABASE mego_prod;"
```

---

## 🔐 Step 5: Configure GitHub Secrets

Go to GitHub → Your Repo → Settings → Secrets and variables → Actions

Add these secrets:

| Secret Name | Value |
|------------|-------|
| `EC2_HOST` | `3.91.9.196` |
| `EC2_USER` | `ubuntu` |
| `EC2_SSH_KEY` | (Content of your `mego-api.pem` file) |
| `EC2_APP_PATH` | `/home/ubuntu/mego-api/MeGo.Api` |

**To get SSH key content:**
```bash
cat ~/.ssh/mego-api.pem
# Copy everything including -----BEGIN and -----END lines
```

---

## 📤 Step 6: Push Code to GitHub

```bash
cd "/Users/cybillnerd/Desktop/mego/mego-api (1)"
git push origin main
```

GitHub Actions will automatically deploy! 🎉

---

## ✅ Verify Deployment

### Check GitHub Actions

1. Go to GitHub → **Actions** tab
2. Watch the workflow run
3. Should see: ✅ Build → ✅ Deploy → ✅ Health Check

### Test API

```bash
# Health check
curl http://3.91.9.196/health

# Swagger
curl http://3.91.9.196/swagger
```

### Check on EC2

```bash
# SSH to EC2
ssh -i ~/.ssh/mego-api.pem ubuntu@3.91.9.196

# Check service
sudo systemctl status mego-api

# View logs
sudo journalctl -u mego-api -f
```

---

## 🔧 Important: RDS Security Group

Since your EC2 and RDS are in the **same VPC**, you need to:

1. Go to **RDS Console** → **database-1**
2. Click **"Modify"**
3. Under **"Connectivity"** → **"VPC security groups"**
4. Add your EC2 instance's security group
5. Click **"Continue"** → **"Apply immediately"**

**Or** allow EC2 private IP in RDS security group:
- Type: PostgreSQL
- Port: 5432
- Source: `172.31.23.246/32` (your EC2 private IP)

---

## 📋 Quick Commands Reference

### On EC2:

```bash
# Check service status
sudo systemctl status mego-api

# Restart service
sudo systemctl restart mego-api

# View logs
sudo journalctl -u mego-api -f

# View application logs
tail -f /home/ubuntu/mego-api/MeGo.Api/logs/app-*.log

# Manual deployment
cd /home/ubuntu/mego-api
./deploy.sh
```

### From Local:

```bash
# Connect to EC2
ssh -i ~/.ssh/mego-api.pem ubuntu@3.91.9.196

# Upload files
scp -i ~/.ssh/mego-api.pem file.txt ubuntu@3.91.9.196:/home/ubuntu/

# Test API
curl http://3.91.9.196/health
```

---

## 🎯 Your API Endpoints

After deployment:

- **API Base**: `http://3.91.9.196/api`
- **Health Check**: `http://3.91.9.196/health`
- **Swagger UI**: `http://3.91.9.196/swagger`
- **Detailed Health**: `http://3.91.9.196/health/detailed`

---

## 🐛 Troubleshooting

### Can't Connect via SSH

```bash
# Check key permissions
chmod 400 ~/.ssh/mego-api.pem

# Try connecting
ssh -i ~/.ssh/mego-api.pem ubuntu@3.91.9.196
```

### Service Not Starting

```bash
# Check logs
sudo journalctl -u mego-api -n 50

# Check if port is in use
sudo lsof -i :5144
```

### RDS Connection Failed

```bash
# Test RDS connection from EC2
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com -U postgres

# Check security group (must allow EC2 security group)
```

---

**Your EC2 is ready! Follow the steps above to deploy. 🚀**

