# 🔍 How to Check RDS Database Connection

This guide shows you multiple ways to verify that your MeGo API is connected to AWS RDS PostgreSQL.

---

## ✅ Method 1: API Health Check Endpoints (Easiest)

Your API has built-in health check endpoints that test the database connection.

### Basic Health Check
```bash
# On EC2 or locally
curl http://localhost:5144/health

# Via Nginx (from outside)
curl http://3.91.9.196/health
```

**Expected Response:**
```json
{
  "status": "Healthy"
}
```

### Detailed Health Check (Shows Database Status)
```bash
# On EC2 or locally
curl http://localhost:5144/health/detailed

# Via Nginx
curl http://3.91.9.196/health/detailed
```

**Expected Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2026-01-07T08:30:00Z",
  "database": {
    "status": "Healthy",
    "responseTime": 45
  }
}
```

**If Database is NOT Connected:**
```json
{
  "status": "Degraded",
  "database": {
    "status": "Unhealthy",
    "error": "Connection timeout"
  }
}
```

---

## ✅ Method 2: Check Application Logs

The API logs database connection attempts and errors.

### View Service Logs (EC2)
```bash
# View recent logs
sudo journalctl -u mego-api -n 50 --no-pager

# Follow logs in real-time
sudo journalctl -u mego-api -f
```

### View Application Logs
```bash
# On EC2
tail -f /var/www/mego/logs/app-*.log

# Look for:
# ✅ "Database connection successful"
# ❌ "Failed to connect to database"
# ❌ "Npgsql.PostgresException"
```

**Good Signs:**
- `🚀 MeGo API starting up...`
- `Database connection established`
- No database errors

**Bad Signs:**
- `Npgsql.PostgresException`
- `Connection timeout`
- `role "postgres" does not exist`
- `database "mego_dev" does not exist`

---

## ✅ Method 3: Check Connection String Configuration

Verify the connection string is pointing to RDS.

### On EC2
```bash
cd ~/Mego-backend/MeGo.Api

# Check production config
cat appsettings.Production.json | grep -A 2 "ConnectionStrings"

# Check published config
cat /var/www/mego/appsettings.Production.json | grep -A 2 "ConnectionStrings"
```

**Expected RDS Connection String:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_dev;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

**Key Indicators:**
- ✅ Host contains `rds.amazonaws.com`
- ✅ Port is `5432`
- ✅ Database name matches your RDS database
- ✅ Username matches your RDS master username
- ✅ Password is set

---

## ✅ Method 4: Test Database Connection Directly

### Using the Test Script (On EC2)
```bash
cd ~/Mego-backend/MeGo.Api
chmod +x test-connection.sh
./test-connection.sh
```

This script will:
1. Extract connection string from config
2. Test connection via `psql` (if installed)
3. Test via API health check

### Using psql Directly (If Installed)
```bash
# Get connection details from config
cd ~/Mego-backend/MeGo.Api
CONN_STRING=$(grep -A 1 "ConnectionStrings" appsettings.Production.json | grep "DefaultConnection" | cut -d'"' -f4)

# Extract details
HOST=$(echo "$CONN_STRING" | grep -oP 'Host=\K[^;]+')
PORT=$(echo "$CONN_STRING" | grep -oP 'Port=\K[^;]+' || echo "5432")
USER=$(echo "$CONN_STRING" | grep -oP 'Username=\K[^;]+')
DB=$(echo "$CONN_STRING" | grep -oP 'Database=\K[^;]+')
PASSWORD=$(echo "$CONN_STRING" | grep -oP 'Password=\K[^;]+')

# Test connection
PGPASSWORD="$PASSWORD" psql -h "$HOST" -p "$PORT" -U "$USER" -d "$DB" -c "SELECT version();"
```

**Success Output:**
```
PostgreSQL 15.x on x86_64-pc-linux-gnu...
```

---

## ✅ Method 5: Check via Swagger UI

Once your API is running and accessible:

1. Open Swagger UI: `http://3.91.9.196/swagger`
2. Find the **Health** endpoints section
3. Click on `/health/detailed`
4. Click **"Try it out"**
5. Click **"Execute"**
6. Check the response for database status

---

## ✅ Method 6: Test an API Endpoint That Uses Database

Try calling an endpoint that requires database access:

```bash
# Example: Get categories (requires DB)
curl http://3.91.9.196/api/Categories

# Example: Health check (requires DB)
curl http://3.91.9.196/health/detailed
```

**If Database is Connected:**
- ✅ Returns data or success response
- ✅ No connection errors

**If Database is NOT Connected:**
- ❌ Returns 500 Internal Server Error
- ❌ Error message about database connection
- ❌ Timeout errors

---

## 🔧 Quick Diagnostic Commands (Run on EC2)

```bash
# 1. Check if API is running
sudo systemctl status mego-api

# 2. Check health endpoint
curl http://localhost:5144/health/detailed | jq .

# 3. Check logs for database errors
sudo journalctl -u mego-api -n 100 | grep -i "database\|postgres\|npgsql"

# 4. Check connection string
cat /var/www/mego/appsettings.Production.json | jq .ConnectionStrings

# 5. Test database connection (if psql installed)
cd ~/Mego-backend/MeGo.Api && ./test-connection.sh
```

---

## 🚨 Common Issues & Solutions

### Issue 1: Health Check Shows "Unhealthy"
**Solution:**
- Check connection string in `appsettings.Production.json`
- Verify RDS password is correct
- Check RDS security group allows EC2 access
- Verify RDS is publicly accessible (if needed)

### Issue 2: "Connection timeout"
**Solution:**
- Check RDS security group inbound rules
- Verify EC2 security group can reach RDS
- Check if RDS is in the same VPC

### Issue 3: "role does not exist" or "database does not exist"
**Solution:**
- Verify database name matches RDS database
- Verify username matches RDS master username
- Create database/user if needed

### Issue 4: "SSL connection required"
**Solution:**
- Add `SSL Mode=Require` to connection string
- Add `Trust Server Certificate=true` for self-signed certs

---

## 📊 Summary Checklist

- [ ] Health endpoint `/health/detailed` shows database as "Healthy"
- [ ] Connection string points to RDS endpoint (`*.rds.amazonaws.com`)
- [ ] No database errors in logs
- [ ] API endpoints that use database work correctly
- [ ] Swagger UI shows successful health checks

---

## 🎯 Quick Test Command

Run this on EC2 to get a complete status:

```bash
echo "🔍 RDS Connection Status Check"
echo "=============================="
echo ""
echo "1. Service Status:"
sudo systemctl status mego-api --no-pager | head -5
echo ""
echo "2. Health Check:"
curl -s http://localhost:5144/health/detailed | jq . || curl -s http://localhost:5144/health/detailed
echo ""
echo "3. Connection String:"
cat /var/www/mego/appsettings.Production.json | jq -r '.ConnectionStrings.DefaultConnection' | sed 's/Password=[^;]*/Password=***/'
echo ""
echo "4. Recent Database Errors:"
sudo journalctl -u mego-api -n 50 --no-pager | grep -i "database\|postgres\|npgsql" | tail -5 || echo "No database errors found"
```

---

**Need Help?** Check the logs first, then verify your RDS configuration matches your connection string.

