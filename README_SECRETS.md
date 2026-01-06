# 🔐 Secrets Management

## ⚠️ IMPORTANT: Never Commit Secrets!

This repository uses GitHub's secret scanning protection. The following files contain sensitive information and should **NEVER** be committed to git:

### Files to Exclude:
- `MeGo.Api/mego-app-776ad-firebase-adminsdk-fbsvc-7b18da4e96.json` - Firebase service account credentials
- `MeGo.Api/appsettings.Development.json` - Contains SendGrid API key and Twilio credentials
- `MeGo.Api/appsettings.json` - Contains SendGrid API key and Twilio credentials
- `MeGo.Api/bin/**/appsettings*.json` - Build output files with secrets

### Setup Instructions:

1. **Copy the example files:**
   ```bash
   cp MeGo.Api/appsettings.json.example MeGo.Api/appsettings.json
   cp MeGo.Api/appsettings.Development.json.example MeGo.Api/appsettings.Development.json
   ```

2. **Fill in your actual secrets** in the copied files (these are gitignored)

3. **For Firebase:**
   - Download your Firebase service account JSON file
   - Place it in `MeGo.Api/` directory
   - Update the path in `appsettings.json`

### Environment Variables (Alternative):

For production, consider using environment variables instead of appsettings files:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=mego_dev;..."
export Jwt__Key="your-jwt-secret-key"
export Smtp__Password="your-sendgrid-api-key"
export Twilio__AccountSid="your-twilio-sid"
export Twilio__AuthToken="your-twilio-token"
```

### If Secrets Were Already Committed:

If you've already committed secrets, you need to:

1. **Remove from git history** (use BFG Repo-Cleaner or git filter-branch)
2. **Rotate all exposed secrets immediately**
3. **Update .gitignore** to prevent future commits
4. **Force push** (only if you've rotated secrets)

**⚠️ WARNING:** If secrets were pushed to a public repository, consider them compromised and rotate immediately!

