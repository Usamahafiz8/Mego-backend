# 🔍 Debug Service Failure

The service failed to start. Check logs:

## Quick Debug Commands

```bash
# Check service status
sudo systemctl status mego-api.service

# View detailed logs
sudo journalctl -u mego-api.service -n 50 --no-pager

# View full logs
sudo journalctl -u mego-api.service -f

# Try running manually to see errors
cd /var/www/mego
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
/home/ubuntu/.dotnet/dotnet MeGo.Api.dll
```

## Common Issues

### 1. Connection String Issue
- Check if RDS password is correct in `appsettings.Production.json`
- Verify RDS security group allows EC2 access

### 2. Port Already in Use
```bash
sudo netstat -tlnp | grep 5144
```

### 3. Missing Dependencies
- Check if all DLLs are published
- Verify .NET runtime is correct version

### 4. Permission Issues
```bash
ls -la /var/www/mego/
sudo chown -R ubuntu:ubuntu /var/www/mego
```

## Fix and Restart

After fixing the issue:

```bash
sudo systemctl restart mego-api
sudo systemctl status mego-api
```
