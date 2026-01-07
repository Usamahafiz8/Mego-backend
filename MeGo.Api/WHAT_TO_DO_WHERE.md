# 📍 What to Do Where - Local vs EC2

## ⚠️ Important: Scripts Location

- **Local Machine (Your Computer)**: Run `prepare-production-config.sh` and `deploy-from-local.sh`
- **EC2 Server**: Just receive the deployment, no scripts needed!

---

## 🖥️ On Your LOCAL Machine

### Step 1: Prepare Production Config

```bash
cd /Users/cybillnerd/Desktop/mego/mego-api\ \(1\)/MeGo.Api
./prepare-production-config.sh
```

This creates `appsettings.Production.json` locally.

### Step 2: Deploy to EC2

```bash
./deploy-from-local.sh
```

This uploads everything to EC2 automatically.

---

## 🖥️ On EC2 Server

**You DON'T need to run those scripts on EC2!**

Instead, after deploying from local, just:

### Option A: If Using deploy-from-local.sh (Recommended)

The script handles everything automatically. Just verify:

```bash
# Check if service is running
sudo systemctl status mego-api

# If not running, start it (first time only)
sudo systemctl start mego-api
```

### Option B: Manual Steps on EC2 (If needed)

```bash
# 1. Navigate to project
cd ~/Mego-backend/MeGo.Api

# 2. Pull latest code (if using git)
git pull origin main

# 3. Publish
dotnet publish -c Release -o /var/www/mego

# 4. Restart service
sudo systemctl restart mego-api
```

---

## 🔄 Complete Workflow

```
LOCAL MACHINE:
  1. ./prepare-production-config.sh  ← Run this locally
  2. ./deploy-from-local.sh          ← Run this locally
  
EC2 SERVER:
  → Receives code automatically
  → No scripts to run!
  → Just verify service is running
```

---

## 🚨 Current Situation

You're on EC2 and trying to run local scripts. Here's what to do:

### On EC2 Right Now:

```bash
# 1. Pull latest code (if using git)
cd ~/Mego-backend/MeGo.Api
git pull origin main

# 2. Check if appsettings.Production.json exists
ls -la appsettings.Production.json

# 3. If it doesn't exist, create it manually:
cp appsettings.Production.json.example appsettings.Production.json
nano appsettings.Production.json
# Add your RDS password and JWT key

# 4. Publish
dotnet publish -c Release -o /var/www/mego

# 5. Copy config
cp appsettings.Production.json /var/www/mego/

# 6. Restart service
sudo systemctl restart mego-api
```

---

## ✅ For Future Deployments

**Always run scripts on your LOCAL machine:**

```bash
# On your Mac/PC (local machine)
cd /path/to/mego-api/MeGo.Api
./prepare-production-config.sh
./deploy-from-local.sh
```

The scripts will automatically upload to EC2. No need to SSH and run anything!
