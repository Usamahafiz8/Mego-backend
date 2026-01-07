# 🔧 RDS Database Setup Guide

## Your RDS Database Details

- **Endpoint**: `database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com`
- **Port**: `5432`
- **Engine**: PostgreSQL
- **Master Username**: `postgres`
- **Status**: Available
- **Publicly Accessible**: ❌ **No** (Important!)
- **VPC**: `vpc-0adb29a28ef24ac66`
- **Security Group**: `default (sg-0a3d1bdab1a2a4393)`

---

## ⚠️ Important Issues to Fix

### 1. Set Master Password

Your RDS instance needs a password. Set it via AWS Console:

**Steps:**
1. Go to RDS Console → Databases → `database-1`
2. Click **"Modify"**
3. Scroll to **"Database authentication"** section
4. Set **"Master password"** (or use "Manage master credentials in AWS Secrets Manager")
5. Click **"Continue"** → **"Apply immediately"**

**Or via AWS CLI:**
```bash
aws rds modify-db-instance \
  --db-instance-identifier database-1 \
  --master-user-password YOUR_NEW_PASSWORD \
  --apply-immediately
```

### 2. Public Access Configuration

**Current Status**: `Publicly accessible: No`

This means you **cannot connect from your local machine** unless:
- You're using a VPN connected to the VPC
- You're connecting from an EC2 instance in the same VPC
- You enable public access (less secure)

**Option A: Enable Public Access (for development)**
1. Go to RDS Console → Databases → `database-1`
2. Click **"Modify"**
3. Under **"Connectivity"** → **"Public access"** → Select **"Publicly accessible"**
4. Click **"Continue"** → **"Apply immediately"**
5. ⚠️ **Security Warning**: This exposes your database to the internet

**Option B: Connect from EC2 (Recommended for production)**
- Deploy your backend to an EC2 instance in the same VPC
- Or use AWS Elastic Beanstalk/ECS

**Option C: Use VPN or AWS Client VPN**
- Set up VPN connection to access RDS from your local machine

### 3. Security Group Configuration

**Current Security Group**: `default (sg-0a3d1bdab1a2a4393)`

**Required**: Allow inbound PostgreSQL traffic (port 5432)

**Steps:**
1. Go to EC2 Console → Security Groups
2. Select security group: `sg-0a3d1bdab1a2a4393`
3. Click **"Edit inbound rules"**
4. Add rule:
   - **Type**: PostgreSQL
   - **Port**: 5432
   - **Source**: 
     - For public access: `0.0.0.0/0` (⚠️ Not recommended)
     - For specific IP: Your IP address
     - For VPC only: `sg-0a3d1bdab1a2a4393` (same security group)
     - For EC2: Your EC2 instance security group
5. Click **"Save rules"**

---

## 🔐 Password Setup Options

### Option 1: Set Password via AWS Console

1. Go to RDS → Databases → `database-1`
2. Click **"Modify"**
3. Under **"Database authentication"**:
   - **Password**: Enter new password
   - Or select **"Manage master credentials in AWS Secrets Manager"**
4. Click **"Continue"** → **"Apply immediately"**

### Option 2: Use AWS Secrets Manager (Recommended)

1. Go to AWS Secrets Manager
2. Create secret:
   - **Secret type**: Credentials for Amazon RDS database
   - **Database**: Select `database-1`
   - **Credentials**: Auto-generate or enter manually
3. Secret will be created automatically
4. Retrieve password:
   ```bash
   aws secretsmanager get-secret-value \
     --secret-id rds-db-credentials/database-1 \
     --region us-east-1 \
     --query SecretString \
     --output text | jq -r '.password'
   ```

### Option 3: Reset Password via AWS CLI

```bash
aws rds modify-db-instance \
  --db-instance-identifier database-1 \
  --master-user-password YOUR_NEW_PASSWORD \
  --apply-immediately \
  --region us-east-1
```

---

## 🔌 Connection String Configuration

### For Local Development (if public access enabled):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_dev;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### For EC2/Production (within VPC):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_prod;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require"
  }
}
```

---

## ✅ Setup Checklist

- [ ] Set master password for RDS instance
- [ ] Configure security group to allow PostgreSQL (port 5432)
- [ ] Enable public access (if connecting from local machine) OR deploy to EC2
- [ ] Create database: `mego_dev` or `mego_prod`
- [ ] Update `appsettings.json` with connection string
- [ ] Test connection: `psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com -U postgres -d postgres`
- [ ] Run migrations: `dotnet ef database update`

---

## 🧪 Test Connection

### From Local Machine (if public access enabled):

```bash
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d postgres \
     -p 5432
```

### From EC2 Instance (within VPC):

```bash
psql -h database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d postgres
```

---

## 🚀 Quick Setup Script

After setting password and enabling public access, run:

```bash
cd MeGo.Api
./configure-rds.sh
```

---

## 📝 Next Steps

1. **Set Password**: Use AWS Console or CLI to set master password
2. **Enable Public Access** (if needed for local dev) OR deploy to EC2
3. **Configure Security Group**: Allow port 5432
4. **Create Database**: `CREATE DATABASE mego_dev;`
5. **Update Connection String**: In `appsettings.json`
6. **Test Connection**: Verify you can connect
7. **Run Migrations**: `dotnet ef database update`

---

## 🔒 Security Best Practices

1. **Don't use default security group** - Create a dedicated security group
2. **Restrict source IP** - Don't use `0.0.0.0/0` for production
3. **Use Secrets Manager** - Store passwords securely
4. **Enable SSL** - Always use `SSL Mode=Require`
5. **Disable public access** - For production, keep it private
6. **Use RDS Proxy** - For connection pooling and security

---

**Your RDS database is ready to configure! 🎉**

