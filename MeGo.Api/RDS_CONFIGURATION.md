# 🔧 AWS RDS Database Configuration Guide

## Your RDS Database Details

- **Endpoint**: `database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com`
- **Master Username**: `postgres`
- **Port**: `5432` (default PostgreSQL port)
- **Password**: Stored in AWS Secrets Manager

---

## Step 1: Get Database Password from AWS Secrets Manager

### Option A: AWS Console

1. Go to [AWS Secrets Manager Console](https://console.aws.amazon.com/secretsmanager/)
2. Find the secret for your RDS database (usually named something like `rds-db-credentials/database-1`)
3. Click on the secret
4. Click **"Retrieve secret value"**
5. Copy the password from the JSON response

### Option B: AWS CLI

```bash
aws secretsmanager get-secret-value \
  --secret-id rds-db-credentials/database-1 \
  --region us-east-1 \
  --query SecretString \
  --output text | jq -r '.password'
```

---

## Step 2: Configure Connection String

### For Local Development

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_dev;Username=postgres;Password=YOUR_PASSWORD_FROM_SECRETS_MANAGER;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### For Production (Environment Variable)

Set environment variable:

```bash
export ConnectionStrings__DefaultConnection="Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_prod;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

---

## Step 3: Create Database (If Not Exists)

Connect to RDS and create the database:

```bash
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d postgres \
     -c "CREATE DATABASE mego_dev;"
```

Or for production:

```bash
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d postgres \
     -c "CREATE DATABASE mego_prod;"
```

---

## Step 4: Run Migrations

After configuring the connection string:

```bash
cd MeGo.Api
dotnet ef database update
```

---

## Step 5: Test Connection

Test the connection:

```bash
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d mego_dev
```

Or test via the API health check:

```bash
curl http://localhost:5144/health/detailed
```

---

## 🔒 Security Best Practices

1. **Never commit passwords** to git
2. **Use environment variables** in production
3. **Use AWS Secrets Manager** for production deployments
4. **Enable SSL** (already in connection string)
5. **Restrict security group** to only allow connections from your application

---

## 🔧 Security Group Configuration

Make sure your RDS security group allows inbound connections:

1. Go to AWS RDS Console
2. Select your database instance
3. Click on **"VPC security groups"**
4. Edit inbound rules to allow:
   - **Type**: PostgreSQL
   - **Port**: 5432
   - **Source**: Your application's IP or security group

---

## ✅ Connection String Format

```
Host=ENDPOINT;Port=5432;Database=DATABASE_NAME;Username=USERNAME;Password=PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

**Important**: Always include `SSL Mode=Require` and `Trust Server Certificate=true` for RDS connections.

---

## 🐛 Troubleshooting

### Connection Timeout

- Check security group rules
- Verify endpoint is correct
- Ensure database is publicly accessible (if connecting from outside AWS)

### Authentication Failed

- Verify password from Secrets Manager
- Check username is correct
- Ensure database exists

### SSL Error

- Make sure `SSL Mode=Require` is in connection string
- Add `Trust Server Certificate=true` for RDS

---

## 📝 Quick Configuration Script

Save this as `configure-rds.sh`:

```bash
#!/bin/bash

echo "🔧 Configuring RDS Connection..."
echo ""

# Get password from AWS Secrets Manager
PASSWORD=$(aws secretsmanager get-secret-value \
  --secret-id rds-db-credentials/database-1 \
  --region us-east-1 \
  --query SecretString \
  --output text | jq -r '.password')

# Set connection string
export ConnectionStrings__DefaultConnection="Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_dev;Username=postgres;Password=$PASSWORD;SSL Mode=Require;Trust Server Certificate=true"

echo "✅ Connection string configured!"
echo ""
echo "Run migrations:"
echo "  dotnet ef database update"
```

---

**Your RDS database is now configured! 🎉**

