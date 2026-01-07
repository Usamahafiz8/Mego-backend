# ✅ Deploy on EC2 - You Already Have Config!

Since you're already on EC2 and `appsettings.Production.json` is created, just run these commands:

## Quick Deploy Commands

```bash
# 1. Make sure you're in the project directory
cd ~/Mego-backend/MeGo.Api

# 2. Create app directory (if not exists)
sudo mkdir -p /var/www/mego
sudo chown ubuntu:ubuntu /var/www/mego

# 3. Publish application
dotnet publish -c Release -o /var/www/mego

# 4. Verify config was copied
ls -la /var/www/mego/appsettings.Production.json

# 5. Run migrations
cd /var/www/mego
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

# Install EF tools if needed
dotnet tool install --global dotnet-ef 2>/dev/null || true

# Run migrations
dotnet ef database update

# 6. Test run (optional - press Ctrl+C to stop)
ASPNETCORE_ENVIRONMENT=Production dotnet MeGo.Api.dll
```

---

## If Service Already Exists

```bash
# Restart the service
sudo systemctl restart mego-api

# Check status
sudo systemctl status mego-api

# View logs
sudo journalctl -u mego-api -f
```

---

## If Service Doesn't Exist Yet

Create the systemd service:

```bash
sudo nano /etc/systemd/system/mego-api.service
```

Paste this:

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

Then:

```bash
sudo systemctl daemon-reload
sudo systemctl enable mego-api
sudo systemctl start mego-api
sudo systemctl status mego-api
```

---

## Verify It's Working

```bash
# Test locally
curl http://localhost:5144/health

# Test from outside (replace with your EC2 IP: 3.91.9.196)
curl http://3.91.9.196/health
```

---

## Note About deploy-from-local.sh

**Don't run `deploy-from-local.sh` on EC2!** That script is for deploying FROM your local machine TO EC2.

Since you're already on EC2, just publish directly using the commands above.
