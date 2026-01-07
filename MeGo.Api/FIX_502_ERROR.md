# 🔧 Fix 502 Bad Gateway Error

## Problem
Nginx returns 502 Bad Gateway - means API isn't responding on port 5144.

## Quick Fix Steps

### Step 1: Check if API is actually running
```bash
# Check service status
sudo systemctl status mego-api

# Check if port 5144 is listening
sudo ss -tlnp | grep 5144
# OR
sudo netstat -tlnp | grep 5144
```

### Step 2: Check API logs
```bash
# View recent service logs
sudo journalctl -u mego-api -n 50 --no-pager

# View application logs
tail -f /var/www/mego/logs/app-*.log
```

### Step 3: Test API directly (bypass Nginx)
```bash
# Test if API responds locally
curl http://localhost:5144/health

# If this works, the issue is Nginx config
# If this fails, the API isn't running properly
```

### Step 4: Check if API crashed
```bash
# Check if process is running
ps aux | grep MeGo.Api.dll

# Check service status
sudo systemctl status mego-api
```

## Common Causes

1. **API crashed after startup** - Check logs
2. **API not binding to 0.0.0.0** - Should listen on all interfaces
3. **Port conflict** - Something else using port 5144
4. **Firewall** - Unlikely on EC2, but check

## Quick Restart
```bash
# Restart service
sudo systemctl restart mego-api

# Wait a few seconds
sleep 5

# Check status
sudo systemctl status mego-api

# Test again
curl http://localhost:5144/health
curl http://3.91.9.196/health
```
