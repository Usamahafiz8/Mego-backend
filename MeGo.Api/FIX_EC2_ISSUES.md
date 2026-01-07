# 🔧 Fix EC2 Issues - Step by Step

## Issue 1: dotnet ef not found

```bash
# Install EF tools
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version
```

## Issue 2: Run Migrations

```bash
cd /var/www/mego
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

# Run migrations
dotnet ef database update
```

**Note:** If migrations fail, you might need to run from the source directory:

```bash
cd ~/Mego-backend/MeGo.Api
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
dotnet ef database update --project . --startup-project .
```

## Issue 3: Create Systemd Service

```bash
sudo nano /etc/systemd/system/mego-api.service
```

Paste this configuration:

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

Save: `Ctrl+X`, then `Y`, then `Enter`

## Issue 4: Start the Service

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

## Issue 5: View Logs (If Service Fails)

```bash
# View service logs
sudo journalctl -u mego-api -f

# View application logs
tail -f /var/www/mego/logs/app-*.log
```

## Issue 6: Test Connection

```bash
# Test locally
curl http://localhost:5144/health

# Test from outside
curl http://3.91.9.196/health
```

## Troubleshooting

### If migrations fail with "connection string is null"
```bash
# Verify config file exists
ls -la /var/www/mego/appsettings.Production.json

# Check connection string
cat /var/www/mego/appsettings.Production.json | grep -A 2 ConnectionStrings
```

### If service fails to start
```bash
# Check logs
sudo journalctl -u mego-api -n 50

# Check if port is in use
sudo netstat -tlnp | grep 5144

# Try running manually to see errors
cd /var/www/mego
ASPNETCORE_ENVIRONMENT=Production /home/ubuntu/.dotnet/dotnet MeGo.Api.dll
```
