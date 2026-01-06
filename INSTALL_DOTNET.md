# 🔧 Install .NET SDK 8.0 for macOS

## Quick Installation

### Option 1: Using Homebrew (Requires Password)

Open Terminal and run:
```bash
brew install --cask dotnet-sdk
```

This will prompt for your password. Enter it when asked.

### Option 2: Direct Download (No Password Required)

1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download: **.NET SDK 8.0** for macOS (ARM64 or x64)
3. Run the installer (.pkg file)
4. Follow the installation wizard

### Option 3: Using Installer Script

```bash
# Download and run the install script
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 8.0.0

# Add to PATH (add to ~/.zshrc or ~/.bash_profile)
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
```

---

## ✅ Verify Installation

After installation, verify it works:

```bash
dotnet --version
# Should show: 8.0.x or higher
```

---

## 🚀 After Installation

Once .NET SDK is installed, you can run the backend:

```bash
cd "/Users/cybillnerd/Desktop/mego/mego-api (1)/MeGo.Api"
dotnet run
```

The API will start on: **http://localhost:5144**

---

## 📝 Note

The backend API requires:
- ✅ .NET SDK 8.0 (to be installed)
- ✅ PostgreSQL database (check if running)
- ✅ Database: `mego_dev` (check if exists)

