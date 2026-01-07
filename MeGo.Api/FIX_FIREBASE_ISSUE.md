# 🔧 Fix Firebase Issue - Quick Steps

## Problem
Service fails because Firebase file is missing: `mego-app-776ad-firebase-adminsdk-fbsvc-7b18da4e96.json`

## Solution

### Option 1: Make Firebase Optional (Recommended)
The code has been updated to handle missing Firebase file gracefully. Just redeploy:

```bash
cd ~/Mego-backend/MeGo.Api
git pull origin main
./deploy-to-ec2.sh
```

### Option 2: Add Firebase File (If You Have It)
```bash
# Copy Firebase file to published directory
cp mego-app-776ad-firebase-adminsdk-fbsvc-7b18da4e96.json /var/www/mego/

# Restart service
sudo systemctl restart mego-api
```

### Option 3: Stop Running Service First
If port is already in use:

```bash
# Stop the service
sudo systemctl stop mego-api

# Kill any remaining processes
sudo pkill -f "MeGo.Api.dll"

# Then restart
sudo systemctl start mego-api
```

## What Changed
- Firebase initialization is now optional
- App will start even without Firebase file
- Push notifications will be skipped if Firebase not available
- Background service handles missing NotificationService gracefully
