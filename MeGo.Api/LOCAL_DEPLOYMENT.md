# 🚀 Deploy from Local Machine (No Server Editing!)

## Quick Start

### Step 1: Prepare Production Config (One Time)

```bash
cd MeGo.Api
./prepare-production-config.sh
```

This will:
- ✅ Create `appsettings.Production.json` from example
- ✅ Prompt you for RDS credentials
- ✅ Prompt you for JWT secret key
- ✅ Automatically configure everything

**Note:** This file is in `.gitignore`, so it won't be committed.

---

### Step 2: Deploy to EC2

**Option A: Automated Deployment (Recommended)**

```bash
./deploy-from-local.sh
```

This script will:
- ✅ Upload your code to EC2
- ✅ Run `dotnet publish` on EC2
- ✅ Copy `appsettings.Production.json` automatically
- ✅ Run migrations
- ✅ Restart the service

**Option B: Manual Upload**

```bash
# 1. Create deployment package
tar czf deploy.tar.gz --exclude='bin' --exclude='obj' --exclude='.vs' --exclude='.vscode' --exclude='*.md' --exclude='*.sh' .

# 2. Upload to EC2
scp -i your-key.pem deploy.tar.gz ubuntu@your-ec2-ip:~/

# 3. SSH and extract
ssh -i your-key.pem ubuntu@your-ec2-ip
cd ~
tar xzf deploy.tar.gz
mv MeGo.Api Mego-backend/
cd Mego-backend/MeGo.Api

# 4. Publish (appsettings.Production.json will be included)
dotnet publish -c Release -o /var/www/mego
```

---

## What Gets Deployed

✅ **Included:**
- All source code
- `appsettings.Production.json` (with your RDS credentials)
- All necessary files

❌ **Excluded:**
- `bin/` and `obj/` folders
- `.vs/` and `.vscode/` folders
- Documentation files (`.md`)
- Scripts (`.sh`) - they're already on EC2

---

## Configuration Flow

```
Local Machine:
  1. Run prepare-production-config.sh
     → Creates appsettings.Production.json
     → Contains RDS password, JWT key, etc.
  
  2. Run deploy-from-local.sh
     → Uploads code + appsettings.Production.json
     → EC2 automatically uses the config

EC2 Server:
  → Receives appsettings.Production.json
  → Uses it automatically (no editing needed!)
```

---

## First Time Setup on EC2

After first deployment, you still need to:

1. **Create Systemd Service** (one time):
   ```bash
   sudo nano /etc/systemd/system/mego-api.service
   # (Copy config from EC2_STEP_BY_STEP.md)
   ```

2. **Configure Nginx** (one time):
   ```bash
   sudo nano /etc/nginx/sites-available/mego-api
   # (Copy config from EC2_STEP_BY_STEP.md)
   ```

3. **Start Service**:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable mego-api
   sudo systemctl start mego-api
   ```

---

## Updating Configuration

If you need to change RDS password or JWT key:

```bash
# 1. Update locally
./prepare-production-config.sh

# 2. Deploy again
./deploy-from-local.sh
```

That's it! No server editing needed.

---

## Security Notes

- ✅ `appsettings.Production.json` is in `.gitignore`
- ✅ Never commit secrets to git
- ✅ Use strong JWT keys (min 32 characters)
- ✅ Keep RDS password secure

---

## Troubleshooting

### "appsettings.Production.json not found" on EC2
- Make sure you ran `prepare-production-config.sh` locally first
- Check that the file exists before deploying

### "Connection string is null"
- Verify `appsettings.Production.json` was copied to `/var/www/mego/`
- Check file permissions: `ls -la /var/www/mego/appsettings*.json`

### "dotnet ef not found"
- Run: `dotnet tool install --global dotnet-ef` on EC2
