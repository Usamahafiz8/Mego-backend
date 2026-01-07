# 🚨 Quick Fix - What to Do on EC2 RIGHT NOW

You're on EC2 and the scripts aren't there (they're for local use). Here's what to do:

## Option 1: Quick Manual Setup (Fastest)

```bash
# 1. Navigate to project
cd ~/Mego-backend/MeGo.Api

# 2. Create production config from example
cp appsettings.Production.json.example appsettings.Production.json

# 3. Edit it
nano appsettings.Production.json
```

**In nano, update:**
- `ConnectionStrings.DefaultConnection` - Replace `YOUR_PASSWORD` with your RDS password
- `Jwt.Key` - Replace with a strong JWT key (min 32 characters)

**Save:** `Ctrl+X`, then `Y`, then `Enter`

```bash
# 4. Publish
dotnet publish -c Release -o /var/www/mego

# 5. Copy config
cp appsettings.Production.json /var/www/mego/

# 6. Run migrations (if needed)
cd /var/www/mego
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
dotnet tool install --global dotnet-ef 2>/dev/null || true
dotnet ef database update

# 7. Restart service (if exists)
sudo systemctl restart mego-api 2>/dev/null || echo "Service not configured yet"
```

---

## Option 2: Pull Latest Code (If Using Git)

```bash
cd ~/Mego-backend/MeGo.Api
git pull origin main

# Then follow Option 1 steps 2-7
```

---

## Option 3: Use deploy-to-ec2.sh (If It Exists)

```bash
cd ~/Mego-backend/MeGo.Api
chmod +x deploy-to-ec2.sh
./deploy-to-ec2.sh
```

---

## ✅ After This, Use Local Scripts

For future deployments, run these on your **LOCAL machine**:

```bash
# On your Mac/PC
cd /Users/cybillnerd/Desktop/mego/mego-api\ \(1\)/MeGo.Api
./prepare-production-config.sh
./deploy-from-local.sh
```

No more SSH editing needed!
