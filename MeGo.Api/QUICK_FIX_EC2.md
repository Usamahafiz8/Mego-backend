# 🚨 Quick Fix for EC2 Deployment Issue

## Problem
The app is failing because `appsettings.Production.json` is missing after `dotnet publish`.

## Solution

### Option 1: Quick Fix (Run on EC2)

```bash
# Navigate to your project
cd ~/Mego-backend/MeGo.Api

# Create production config from example
cp appsettings.Production.json.example appsettings.Production.json

# Edit it with your RDS password
nano appsettings.Production.json
# Update the ConnectionStrings.DefaultConnection with your RDS password

# Publish again (this time it will include appsettings.Production.json)
dotnet publish -c Release -o /var/www/mego

# Copy the config file manually (just to be sure)
cp appsettings.Production.json /var/www/mego/

# Run the app
cd /var/www/mego
ASPNETCORE_ENVIRONMENT=Production dotnet MeGo.Api.dll
```

### Option 2: Use the Fix Script

```bash
# Upload fix-ec2-deployment.sh to EC2, then:
chmod +x fix-ec2-deployment.sh
./fix-ec2-deployment.sh
```

### Option 3: Create Config Manually

```bash
cd /var/www/mego

# Create appsettings.Production.json
cat > appsettings.Production.json << 'CONFIG'
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_prod;Username=postgres;Password=YOUR_RDS_PASSWORD_HERE;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Key": "YOUR_PRODUCTION_JWT_SECRET_KEY_MIN_32_CHARACTERS",
    "Issuer": "mego-api",
    "Audience": "mego-clients",
    "ExpireMinutes": "60"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
CONFIG

# Replace YOUR_RDS_PASSWORD_HERE with your actual RDS password
nano appsettings.Production.json

# Run the app
ASPNETCORE_ENVIRONMENT=Production dotnet MeGo.Api.dll
```

## Why This Happened

`dotnet publish` only copies files that are:
- Compiled binaries
- Files explicitly marked in `.csproj` to be copied
- Files in `wwwroot/`

Example files (`.example`) are not copied by default.

## Permanent Fix

The `.csproj` file has been updated to automatically copy `appsettings.Production.json` during publish. After pulling the latest code, this won't happen again.
