# 🔧 Fix Nginx Configuration

## Problem
Nginx is showing default welcome page instead of proxying to API.

## Quick Fix on EC2

```bash
# 1. Get your EC2 IP
EC2_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)
echo "EC2 IP: $EC2_IP"

# 2. Create/Update Nginx config
sudo tee /etc/nginx/sites-available/mego-api > /dev/null <<NGINX_CONFIG
server {
    listen 80;
    server_name $EC2_IP;

    location / {
        proxy_pass http://localhost:5144;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
NGINX_CONFIG

# 3. Enable site and remove default
sudo ln -sf /etc/nginx/sites-available/mego-api /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default

# 4. Test and restart
sudo nginx -t
sudo systemctl restart nginx

# 5. Test API
curl http://localhost:5144/health
curl http://$EC2_IP/health
```

## Verify

```bash
# Check if API is running
sudo systemctl status mego-api

# Check Nginx config
sudo nginx -t

# Test locally
curl http://localhost:5144/health

# Test via Nginx
curl http://3.91.9.196/health
```
