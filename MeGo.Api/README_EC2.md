# 🚀 EC2 Deployment - One Command Setup

## Prerequisites
- EC2 instance with Ubuntu
- .NET SDK 8.0 installed
- Nginx installed
- Code cloned to `~/Mego-backend/MeGo.Api`
- `appsettings.Production.json` configured with RDS password

## One Command Deployment

```bash
cd ~/Mego-backend/MeGo.Api
chmod +x deploy-to-ec2.sh
./deploy-to-ec2.sh
```

**That's it!** The script automatically:
- ✅ Installs EF Core tools if needed
- ✅ Publishes the application
- ✅ Copies configuration
- ✅ Runs database migrations
- ✅ Creates systemd service
- ✅ Starts the service
- ✅ Configures Nginx
- ✅ Verifies deployment

## First Time Setup

Before first deployment, create `appsettings.Production.json`:

```bash
cd ~/Mego-backend/MeGo.Api
cp appsettings.Production.json.example appsettings.Production.json
nano appsettings.Production.json
```

Update:
- `ConnectionStrings.DefaultConnection` - Add your RDS password
- `Jwt.Key` - Add a strong JWT key (min 32 characters)

Then run:
```bash
./deploy-to-ec2.sh
```

## Updating Deployment

After code changes, just run again:

```bash
cd ~/Mego-backend/MeGo.Api
git pull  # If using git
./deploy-to-ec2.sh
```

The script handles everything automatically!

## Troubleshooting

### Service won't start
```bash
sudo journalctl -u mego-api -f
```

### Check if running
```bash
sudo systemctl status mego-api
curl http://localhost:5144/health
```

### Restart service
```bash
sudo systemctl restart mego-api
```
