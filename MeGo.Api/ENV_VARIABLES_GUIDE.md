# 🔐 Environment Variables Guide for .NET

## Do We Need .env Files?

**Short Answer**: **No, but optional for local development convenience.**

---

## How .NET Handles Configuration

.NET Core reads configuration from multiple sources (in priority order):

1. **Environment Variables** (highest priority)
2. **appsettings.{Environment}.json** (e.g., `appsettings.Production.json`)
3. **appsettings.json** (lowest priority)

**Note**: .NET doesn't natively read `.env` files (unlike Node.js).

---

## Current Setup ✅

You're already using:
- ✅ `appsettings.json` - For local development
- ✅ `appsettings.Production.json` - For production (EC2)
- ✅ Environment variables - For PORT, ASPNETCORE_URLS

**This works perfectly!** No .env files needed.

---

## Option 1: Keep Current Approach (Recommended) ✅

### Local Development
```bash
# Use appsettings.json directly
dotnet run
```

### Production (EC2)
```bash
# Use appsettings.Production.json
# Or set environment variables in systemd service
```

**Pros**:
- ✅ No extra packages needed
- ✅ Works out of the box
- ✅ Standard .NET approach
- ✅ Perfect for production

**Cons**:
- ❌ Need to manage multiple appsettings files

---

## Option 2: Add .env Support (Optional)

If you prefer `.env` files for local development:

### Step 1: Install DotNetEnv Package
```bash
dotnet add package DotNetEnv
```

### Step 2: Load .env in Program.cs
```csharp
// At the top of Program.cs
DotNetEnv.Env.Load(); // Loads .env file
```

### Step 3: Create .env File
```bash
# .env (local development)
ConnectionStrings__DefaultConnection=Host=localhost;Database=mego_dev;...
Jwt__Key=your-secret-key-here
Jwt__Issuer=mego-api
```

**Pros**:
- ✅ Familiar if coming from Node.js
- ✅ Easy to manage local secrets
- ✅ Already in .gitignore

**Cons**:
- ❌ Extra package dependency
- ❌ Not standard .NET approach
- ❌ Still need appsettings for production

---

## Option 3: Pure Environment Variables (Best for Production)

### Local Development
```bash
# Set environment variables before running
export ConnectionStrings__DefaultConnection="Host=localhost;..."
export Jwt__Key="your-secret-key"
dotnet run
```

### Production (EC2) - Systemd Service
```ini
[Service]
Environment="ConnectionStrings__DefaultConnection=Host=rds-endpoint;..."
Environment="Jwt__Key=your-production-key"
Environment="ASPNETCORE_ENVIRONMENT=Production"
```

**Pros**:
- ✅ No files with secrets
- ✅ Perfect for Docker/containers
- ✅ Works with AWS Secrets Manager
- ✅ Most secure

**Cons**:
- ❌ Less convenient for local dev

---

## Recommendation 🎯

**For Your Setup**:

1. **Local Development**: Keep using `appsettings.json` ✅
   - Simple and works well
   - Already configured

2. **Production (EC2)**: Use `appsettings.Production.json` + Environment Variables ✅
   - Set secrets via systemd service
   - Or use AWS Secrets Manager

3. **Optional**: Add `.env` support only if you prefer it for local dev
   - Not necessary, but convenient

---

## Example: Setting Environment Variables

### Format for Nested Config
```bash
# appsettings.json has:
# {
#   "Jwt": {
#     "Key": "value"
#   }
# }

# Environment variable format:
export Jwt__Key="your-secret-key"  # Double underscore for nesting
```

### In Systemd Service (EC2)
```ini
[Service]
Environment="ConnectionStrings__DefaultConnection=Host=...;..."
Environment="Jwt__Key=your-production-key"
Environment="Jwt__Issuer=mego-api"
Environment="Jwt__Audience=mego-clients"
```

---

## Summary

| Approach | Local Dev | Production | Recommendation |
|----------|-----------|------------|----------------|
| **appsettings.json** | ✅ Yes | ✅ Yes | ✅ **Recommended** |
| **.env files** | ✅ Optional | ❌ No | ⚠️ Optional convenience |
| **Environment Variables** | ✅ Yes | ✅ **Best** | ✅ **Best for production** |

**Your current setup is perfect!** No changes needed unless you want `.env` convenience.
