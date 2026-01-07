# 🔄 CI/CD Pipeline Setup Guide

Complete guide to set up CI/CD pipeline for MeGo API using GitHub Actions and EC2.

---

## 📋 Overview

This CI/CD pipeline:
- ✅ Builds and tests on every push/PR
- ✅ Automatically deploys to EC2 on push to `main` branch
- ✅ Runs health checks after deployment
- ✅ Supports manual deployment trigger

---

## 🏗️ Architecture

```
GitHub Repository
    ↓ (push to main)
GitHub Actions (CI/CD)
    ↓ (build & test)
    ↓ (deploy via SSH)
EC2 Instance
    ↓ (systemd service)
MeGo API Running
```

---

## 🚀 Step 1: Setup EC2 Instance

### 1.1: Launch EC2 Instance

1. Go to **EC2 Console** → **Launch Instance**
2. **AMI**: Ubuntu Server 22.04 LTS
3. **Instance Type**: `t3.small` (recommended) or `t3.micro`
4. **Key Pair**: Create new or use existing
5. **Security Group**: 
   - SSH (22) from your IP
   - HTTP (80) from anywhere
   - HTTPS (443) from anywhere
   - Custom TCP (5144) from anywhere
6. **Storage**: 20 GB minimum
7. Launch instance

### 1.2: Connect to EC2

```bash
ssh -i your-key.pem ubuntu@YOUR_EC2_IP
```

### 1.3: Run CI/CD Setup Script

On EC2, run:

```bash
# Download setup script
wget -O setup-cicd-ec2.sh YOUR_SCRIPT_URL
# OR copy from your local machine:
# scp -i your-key.pem MeGo.Api/setup-cicd-ec2.sh ubuntu@EC2_IP:/home/ubuntu/

# Make executable and run
chmod +x setup-cicd-ec2.sh
./setup-cicd-ec2.sh
```

This will install:
- ✅ .NET SDK 8.0
- ✅ PostgreSQL client
- ✅ Nginx
- ✅ Systemd service
- ✅ Deployment scripts
- ✅ Firewall configuration

---

## 🔐 Step 2: Configure GitHub Secrets

### 2.1: Get EC2 Details

```bash
# On EC2, get public IP
curl http://169.254.169.254/latest/meta-data/public-ipv4

# Or check in AWS Console
```

### 2.2: Add GitHub Secrets

1. Go to your GitHub repository
2. **Settings** → **Secrets and variables** → **Actions**
3. Click **"New repository secret"**
4. Add these secrets:

| Secret Name | Value | Description |
|------------|-------|------------|
| `EC2_HOST` | `your-ec2-public-ip` | EC2 instance public IP |
| `EC2_USER` | `ubuntu` | EC2 username |
| `EC2_SSH_KEY` | `-----BEGIN RSA PRIVATE KEY-----...` | Your EC2 private key (full content) |
| `EC2_APP_PATH` | `/home/ubuntu/mego-api/MeGo.Api` | Application path on EC2 |

### 2.3: Get SSH Private Key

```bash
# On your local machine, get the private key content
cat your-ec2-key.pem

# Copy the entire content including:
# -----BEGIN RSA PRIVATE KEY-----
# ...key content...
# -----END RSA PRIVATE KEY-----
```

**Important**: Copy the entire key including BEGIN/END lines.

---

## 📝 Step 3: Configure RDS Connection

### 3.1: Create appsettings.Production.json on EC2

```bash
# On EC2
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
    "Key": "YOUR_PRODUCTION_JWT_SECRET",
    "Issuer": "mego-api",
    "Audience": "mego-clients",
    "ExpireMinutes": "60"
  },
  "ASPNETCORE_ENVIRONMENT": "Production",
  "ASPNETCORE_URLS": "http://0.0.0.0:5144"
}
```

### 3.2: Setup Database

```bash
# On EC2
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

# Create database
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d postgres \
     -c "CREATE DATABASE mego_prod;"

# Run migrations (first time)
cd /home/ubuntu/mego-api/MeGo.Api
dotnet ef database update
```

---

## 🔄 Step 4: Push Code to GitHub

### 4.1: Initialize Git (if not done)

```bash
cd "/Users/cybillnerd/Desktop/mego/mego-api (1)"
git init
git remote add origin YOUR_GITHUB_REPO_URL
```

### 4.2: Push Code

```bash
git add .
git commit -m "Initial commit with CI/CD"
git push -u origin main
```

---

## ✅ Step 5: Test CI/CD Pipeline

### 5.1: Automatic Deployment

1. Make a small change to code
2. Commit and push:
   ```bash
   git add .
   git commit -m "Test CI/CD deployment"
   git push origin main
   ```
3. Go to GitHub → **Actions** tab
4. Watch the workflow run:
   - ✅ Build job completes
   - ✅ Deploy job runs
   - ✅ Health check passes

### 5.2: Manual Deployment

1. Go to GitHub → **Actions**
2. Select **"Deploy to EC2"** workflow
3. Click **"Run workflow"**
4. Select branch: `main`
5. Click **"Run workflow"**

---

## 📊 Monitoring Deployments

### View Deployment Status

```bash
# On EC2, check service status
sudo systemctl status mego-api

# View logs
sudo journalctl -u mego-api -f

# View application logs
tail -f /home/ubuntu/mego-api/MeGo.Api/logs/app-*.log
```

### GitHub Actions

- Go to **Actions** tab in GitHub
- View workflow runs
- Check build and deployment logs

---

## 🔧 Troubleshooting

### Deployment Fails

1. **Check GitHub Actions logs**
   - Go to Actions → Failed workflow → Check logs

2. **Check EC2 connection**
   ```bash
   # Test SSH connection
   ssh -i your-key.pem ubuntu@EC2_IP
   ```

3. **Check service status on EC2**
   ```bash
   sudo systemctl status mego-api
   sudo journalctl -u mego-api -n 50
   ```

### Build Fails

- Check `.github/workflows/deploy.yml`
- Verify .NET SDK version matches
- Check for build errors in Actions logs

### Health Check Fails

- Verify service is running: `sudo systemctl status mego-api`
- Check Nginx configuration: `sudo nginx -t`
- Test endpoint manually: `curl http://localhost:5144/health`

---

## 📝 Workflow Files

### `.github/workflows/deploy.yml`
- Builds application
- Creates deployment package
- Deploys to EC2 via SSH
- Runs health check

### `.github/workflows/ci.yml`
- Runs on pull requests
- Builds and tests code
- No deployment

---

## 🔒 Security Best Practices

1. **Never commit secrets** to repository
2. **Use GitHub Secrets** for sensitive data
3. **Restrict SSH access** to specific IPs
4. **Use strong passwords** for RDS
5. **Enable firewall** on EC2
6. **Regular updates** for EC2 instance

---

## 📋 Quick Reference

### EC2 Commands

```bash
# Check service
sudo systemctl status mego-api

# Restart service
sudo systemctl restart mego-api

# View logs
sudo journalctl -u mego-api -f

# Manual deployment
cd /home/ubuntu/mego-api
./deploy.sh
```

### GitHub Actions

- **Automatic**: Push to `main` triggers deployment
- **Manual**: Actions → Deploy to EC2 → Run workflow

---

## ✅ Checklist

- [ ] EC2 instance launched
- [ ] CI/CD setup script run on EC2
- [ ] GitHub Secrets configured
- [ ] RDS connection configured
- [ ] Database created and migrated
- [ ] Code pushed to GitHub
- [ ] First deployment successful
- [ ] Health check passing

---

**Your CI/CD pipeline is ready! 🎉**

