# 🔍 How to Find Your RDS Endpoint

## Method 1: AWS Console (Easiest)

### Step 1: Go to RDS Console
1. Open [AWS Console](https://console.aws.amazon.com/)
2. Search for **"RDS"** in the top search bar
3. Click on **"RDS"** service

### Step 2: Find Your Database
1. In the left sidebar, click **"Databases"**
2. You'll see a list of your databases
3. Click on your database (likely named `database-1`)

### Step 3: Get the Endpoint
1. Scroll down to **"Connectivity & security"** section
2. Look for **"Endpoint & port"**
3. Copy the **Endpoint** (looks like: `database-1.xxxxx.us-east-1.rds.amazonaws.com`)
4. Note the **Port** (usually `5432` for PostgreSQL)

---

## Method 2: AWS CLI

```bash
aws rds describe-db-instances \
  --region us-east-1 \
  --query 'DBInstances[*].[DBInstanceIdentifier,Endpoint.Address,Endpoint.Port]' \
  --output table
```

---

## Method 3: From Previous Conversations

Based on your previous setup, your RDS endpoint is:
- **Endpoint**: `database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com`
- **Port**: `5432`
- **Database Name**: `mego_prod` (or `mego_dev` for development)
- **Username**: `postgres`

---

## Quick Visual Guide

```
AWS Console → RDS → Databases → Click database-1
                                    ↓
                        Connectivity & security
                                    ↓
                        Endpoint & port
                                    ↓
                    database-1.xxxxx.rds.amazonaws.com
```

---

## What You Need for Connection String

Once you have the endpoint, your connection string will be:

```
Host=database-1.c27g0uwm43k1.us-east-1.rds.amazonaws.com;Port=5432;Database=mego_prod;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

Replace `YOUR_PASSWORD` with your actual RDS master password.
