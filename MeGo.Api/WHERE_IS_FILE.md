# 📁 Where is appsettings.Production.json.example?

## ✅ Local Machine (Your Computer)

**Location:**
```
/Users/cybillnerd/Desktop/mego/mego-api (1)/MeGo.Api/appsettings.Production.json.example
```

**Status:** ✅ File EXISTS locally

---

## 🖥️ On EC2 Server

**Should be at:**
```
~/Mego-backend/MeGo.Api/appsettings.Production.json.example
```

**Check if it exists:**
```bash
cd ~/Mego-backend/MeGo.Api
ls -la appsettings*.json*
```

---

## 🔧 If File is Missing on EC2

### Option 1: Pull Latest Code (If Using Git)
```bash
cd ~/Mego-backend/MeGo.Api
git pull origin main
```

### Option 2: Create It Manually
```bash
cd ~/Mego-backend/MeGo.Api
cat > appsettings.Production.json.example << 'CONFIG'
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_prod;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Key": "YOUR_SUPER_SECRET_JWT_KEY_MIN_32_CHARACTERS_LONG",
    "Issuer": "mego-api",
    "Audience": "mego-clients",
    "ExpireMinutes": "60"
  },
  "Firebase": {
    "ServiceAccountPath": "PATH_TO_FIREBASE_SERVICE_ACCOUNT_JSON"
  },
  "Smtp": {
    "Host": "smtp.sendgrid.net",
    "Port": "587",
    "Username": "apikey",
    "Password": "YOUR_SENDGRID_API_KEY",
    "FromEmail": "mego.com.pk.1@gmail.com",
    "FromName": "MEGO"
  },
  "Twilio": {
    "AccountSid": "YOUR_TWILIO_ACCOUNT_SID",
    "AuthToken": "YOUR_TWILIO_AUTH_TOKEN",
    "ServiceSid": "YOUR_TWILIO_SERVICE_SID"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "EnableSwagger": false
}
CONFIG
```

### Option 3: Upload from Local Machine
```bash
# From your local machine
scp -i your-key.pem appsettings.Production.json.example ubuntu@your-ec2-ip:~/Mego-backend/MeGo.Api/
```

---

## ✅ Quick Fix on EC2

```bash
cd ~/Mego-backend/MeGo.Api

# Check if file exists
if [ -f "appsettings.Production.json.example" ]; then
    echo "✅ File exists"
else
    echo "❌ File missing - creating it..."
    # Create from the content above (Option 2)
fi

# Then create production config
cp appsettings.Production.json.example appsettings.Production.json
nano appsettings.Production.json  # Add your RDS password
```
