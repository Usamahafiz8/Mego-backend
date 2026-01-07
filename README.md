# 🚀 MeGo API Backend

A professional, production-ready .NET 8.0 ASP.NET Core Web API for the MeGo marketplace platform.

---

## 📋 Table of Contents

- [Features](#-features)
- [Prerequisites](#-prerequisites)
- [Quick Start](#-quick-start)
- [Configuration](#-configuration)
- [API Endpoints](#-api-endpoints)
- [Project Structure](#-project-structure)
- [Development](#-development)
- [Deployment](#-deployment)

---

## ✨ Features

- ✅ **Global Exception Handling** - Centralized error handling with standardized responses
- ✅ **Health Checks** - `/health`, `/health/detailed`, `/health/ready`, `/health/live`
- ✅ **Request Logging** - Automatic request/response logging with timing
- ✅ **Security Headers** - X-Content-Type-Options, X-Frame-Options, etc.
- ✅ **Standardized Responses** - Consistent API response structure
- ✅ **Database Resilience** - Automatic retry logic for database connections
- ✅ **Swagger Documentation** - Professional API documentation
- ✅ **JWT Authentication** - Secure token-based authentication
- ✅ **SignalR Hubs** - Real-time communication (Admin, Chat, User)
- ✅ **CORS Support** - Configurable CORS policies

---

## 🔧 Prerequisites

- **.NET SDK 8.0** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL Database** - Version 12 or higher
- **Node.js** (optional) - For frontend development

---

## 🚀 Quick Start

### 1. Clone and Navigate

```bash
cd "/Users/cybillnerd/Desktop/mego/mego-api (1)/MeGo.Api"
```

### 2. Set Up .NET SDK Path (if needed)

If you get `dotnet: command not found`, add to your `~/.zshrc`:

```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
source ~/.zshrc
```

### 3. Configure Database

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mego_dev;Username=mego;Password=YourPassword"
  }
}
```

### 4. Restore Dependencies

```bash
dotnet restore
```

### 5. Run Database Migrations (First Time)

```bash
dotnet ef database update
```

### 6. Run the Backend

```bash
dotnet run
```

Or use the startup script:

```bash
./start.sh
```

### 7. Verify It's Running

- **Swagger UI**: `http://localhost:5144/swagger`
- **Health Check**: `http://localhost:5144/health`
- **API Base**: `http://localhost:5144/api`

---

## ⚙️ Configuration

### AWS RDS Database Configuration

If you're using AWS RDS PostgreSQL, use the configuration script:

```bash
cd MeGo.Api
./configure-rds.sh
```

Or manually configure in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_dev;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

**Get Password from AWS Secrets Manager:**
1. Go to [AWS Secrets Manager Console](https://console.aws.amazon.com/secretsmanager/)
2. Find secret for your RDS database
3. Retrieve secret value and copy password

See `RDS_CONFIGURATION.md` for detailed RDS setup instructions.

### Environment Variables

```bash
# Database (Local)
export ConnectionStrings__DefaultConnection="Host=localhost;Database=mego_dev;Username=mego;Password=password"

# Database (AWS RDS)
export ConnectionStrings__DefaultConnection="Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_dev;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"

# JWT
export Jwt__Key="your-secret-key-here"

# Environment
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://0.0.0.0:5144"

# Enable Swagger in Production (optional)
export EnableSwagger="true"
```

### appsettings.json

See `appsettings.json.example` for configuration template.

**Important**: Never commit `appsettings.json` with secrets. Use environment variables or AWS Secrets Manager in production.

---

## 📡 API Endpoints

### Base URL
```
http://localhost:5144/api
```

### Key Endpoints

| Endpoint | Method | Description | Auth Required |
|----------|--------|-------------|---------------|
| `/health` | GET | Basic health check | No |
| `/health/detailed` | GET | Detailed health with DB status | No |
| `/api/v1/auth/login` | POST | User login | No |
| `/api/v1/auth/signup` | POST | User registration | No |
| `/api/v1/users/me` | GET | Get current user | Yes |
| `/api/v1/ads` | GET | List all ads | No |
| `/swagger` | GET | API documentation | No |

### Authentication

Most endpoints require JWT authentication. Include token in header:

```
Authorization: Bearer {your-token}
```

---

## 🏗️ Project Structure

```
MeGo.Api/
├── Controllers/          # API Controllers
│   ├── Admin/           # Admin panel controllers
│   └── ...
├── Data/                # Database context and migrations
├── Models/              # Domain models
│   └── Responses/       # Standardized response models
├── Services/            # Business logic services
├── Hubs/                # SignalR hubs (real-time)
├── Middleware/          # Custom middleware
│   ├── GlobalExceptionHandlerMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   └── SecurityHeadersMiddleware.cs
├── Filters/             # Swagger filters
├── Extensions/          # Extension methods
├── Attributes/          # Custom attributes
├── Dtos/                # Data transfer objects
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration
```

---

## 💻 Development

### Running in Development Mode

```bash
dotnet run
```

The API will start on `http://localhost:5144`

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

### View Logs

Logs are output to console. In production, configure logging to file or cloud service.

---

## 🏥 Health Checks

| Endpoint | Purpose | Use Case |
|----------|---------|----------|
| `/health` | Basic health | General monitoring |
| `/health/detailed` | Full health check | Detailed diagnostics |
| `/health/ready` | Readiness probe | Kubernetes/Docker |
| `/health/live` | Liveness probe | Kubernetes/Docker |

---

## 🔒 Security

- **JWT Bearer Authentication** - Token-based auth
- **Security Headers** - X-Content-Type-Options, X-Frame-Options, etc.
- **CORS Configuration** - Configurable origins
- **Input Validation** - Model validation
- **SQL Injection Protection** - Entity Framework Core
- **Error Handling** - No sensitive data in error responses

---

## 📚 API Documentation

### Swagger UI

Access interactive API documentation at:
```
http://localhost:5144/swagger
```

### Features:
- Try out endpoints directly
- View request/response schemas
- Test authentication
- Filter endpoints

---

## 🚀 Deployment

### AWS EC2 Deployment (Recommended)

**Quick Deploy:**

1. **Launch EC2 Instance** (Ubuntu 22.04)
2. **Connect via SSH:**
   ```bash
   ssh -i your-key.pem ubuntu@YOUR_EC2_IP
   ```
3. **Run Setup Script:**
   ```bash
   wget -O ec2-setup.sh YOUR_SCRIPT_URL
   chmod +x ec2-setup.sh
   ./ec2-setup.sh
   ```
4. **Upload Code:**
   ```bash
   # From local machine
   scp -r mego-api ubuntu@EC2_IP:/home/ubuntu/
   ```
5. **Deploy:**
   ```bash
   # On EC2
   cd mego-api/MeGo.Api
   ./deploy-to-ec2.sh
   ```

**See `EC2_DEPLOYMENT.md` for complete deployment guide.**

### Docker

```bash
docker build -t mego-api .
docker run -p 5144:5144 mego-api
```

### AWS Elastic Beanstalk

1. Install EB CLI
2. Initialize: `eb init`
3. Create environment: `eb create`
4. Deploy: `eb deploy`

### Environment Variables for Production

Set these in your deployment platform:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:5144`

---

## 🐛 Troubleshooting

### Port Already in Use

```bash
lsof -ti:5144 | xargs kill -9
```

### Database Connection Failed

1. Verify PostgreSQL is running
2. Check connection string in `appsettings.json`
3. Test connection: `psql -h localhost -U mego -d mego_dev`

### .NET SDK Not Found

```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
```

### Build Errors

```bash
dotnet clean
dotnet restore
dotnet build
```

---

## 📝 License

Proprietary - MeGo Platform

---

## 📞 Support

For API support, contact: support@mego.com.pk

---

**Built with ❤️ for MeGo Platform**

