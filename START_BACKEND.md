# 🚀 MeGo Backend API - Setup & Run Guide

## 📋 Project Overview

**MeGo API** is a .NET 8.0 ASP.NET Core Web API backend for the MeGo marketplace platform.

- **Framework**: .NET 8.0
- **Database**: PostgreSQL
- **Port**: 5144
- **Swagger**: Available at `/swagger`

---

## ⚠️ Prerequisites

### 1. Install .NET SDK 8.0

**On macOS:**
```bash
# Option 1: Using Homebrew (Recommended)
brew install --cask dotnet-sdk

# Option 2: Download from Microsoft
# Visit: https://dotnet.microsoft.com/download/dotnet/8.0
# Download and install the .NET 8.0 SDK for macOS
```

**Verify Installation:**
```bash
dotnet --version
# Should show: 8.0.x or higher
```

### 2. Install PostgreSQL

**On macOS:**
```bash
brew install postgresql@14
brew services start postgresql@14
```

**Create Database:**
```bash
# Connect to PostgreSQL
psql postgres

# Create database and user
CREATE DATABASE mego_dev;
CREATE USER mego WITH PASSWORD 'StrongPassword123';
GRANT ALL PRIVILEGES ON DATABASE mego_dev TO mego;
\q
```

---

## 🔧 Configuration

### Database Connection

The API uses PostgreSQL. Update `appsettings.Development.json` if needed:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mego_dev;Username=mego;Password=StrongPassword123"
  }
}
```

### Port Configuration

The API runs on port **5144** (configured in `Program.cs`):
- URL: `http://localhost:5144`
- Swagger: `http://localhost:5144/swagger`

---

## 🚀 Running the Backend

### Method 1: Using dotnet CLI (Recommended)

```bash
cd "/Users/cybillnerd/Desktop/mego/mego-api (1)/MeGo.Api"
dotnet run
```

### Method 2: Run Built DLL

```bash
cd "/Users/cybillnerd/Desktop/mego/mego-api (1)/MeGo.Api/bin/Debug/net8.0"
dotnet MeGo.Api.dll
```

### Method 3: Using Visual Studio / Rider

1. Open the solution file (if exists) or project
2. Set `MeGo.Api` as startup project
3. Press F5 or click Run

---

## 📡 API Endpoints

### Base URL
```
http://localhost:5144/api
```

### Key Endpoints:
- **Auth**: `/api/auth/*`
- **Ads**: `/api/ads/*`
- **Users**: `/api/users/*`
- **Admin**: `/api/admin/*`
- **Loyalty**: `/api/loyalty/*`
- **Wallet**: `/api/wallet/*`
- **Messages**: `/api/messages/*`
- **Categories**: `/api/categories/*`

### Swagger Documentation
```
http://localhost:5144/swagger
```

---

## 🔌 SignalR Hubs

Real-time communication hubs:
- **Admin Hub**: `/hubs/admin`
- **Chat Hub**: `/chatHub`
- **User Hub**: `/userHub`

---

## 🗄️ Database Migrations

If you need to apply migrations:

```bash
cd "/Users/cybillnerd/Desktop/mego/mego-api (1)/MeGo.Api"
dotnet ef database update
```

---

## ✅ Verify Backend is Running

1. **Check Swagger**: http://localhost:5144/swagger
2. **Check Root**: http://localhost:5144/ (should return "🚀 MeGo Backend Running")
3. **Check Health**: http://localhost:5144/api/health (if endpoint exists)

---

## 🔍 Troubleshooting

### Issue: "dotnet: command not found"
**Solution**: Install .NET SDK 8.0 (see Prerequisites)

### Issue: Database Connection Error
**Solution**: 
- Ensure PostgreSQL is running: `brew services start postgresql@14`
- Verify database exists: `psql -U mego -d mego_dev`
- Check connection string in `appsettings.Development.json`

### Issue: Port Already in Use
**Solution**: 
```bash
# Kill process on port 5144
lsof -ti:5144 | xargs kill -9
```

### Issue: Build Errors
**Solution**:
```bash
cd "/Users/cybillnerd/Desktop/mego/mego-api (1)/MeGo.Api"
dotnet clean
dotnet restore
dotnet build
```

---

## 📝 Environment Variables

The API uses `appsettings.Development.json` for configuration:
- Database connection string
- JWT settings
- Firebase configuration
- SMTP settings (SendGrid)
- Twilio settings

---

## 🎯 Next Steps

1. ✅ Install .NET SDK 8.0
2. ✅ Install and configure PostgreSQL
3. ✅ Run database migrations (if needed)
4. ✅ Start the backend API
5. ✅ Verify Swagger is accessible
6. ✅ Connect frontend applications

---

**Status**: Ready to run once .NET SDK is installed!

