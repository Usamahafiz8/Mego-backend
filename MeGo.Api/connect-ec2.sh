#!/bin/bash

# Quick EC2 Connection Script
# Makes it easy to connect to your EC2 instance

EC2_IP="3.91.9.196"
EC2_USER="ubuntu"
KEY_PATH="$HOME/.ssh/mego-api.pem"

echo "🔌 Connecting to EC2 Instance"
echo "============================="
echo ""
echo "Instance: $EC2_IP"
echo "User: $EC2_USER"
echo ""

# Check if key exists
if [ ! -f "$KEY_PATH" ]; then
    echo "❌ Key file not found at: $KEY_PATH"
    echo ""
    echo "Please provide key path:"
    read -p "Key path: " KEY_PATH
fi

# Set correct permissions
chmod 400 "$KEY_PATH" 2>/dev/null

echo "Connecting..."
echo ""

# Connect
ssh -i "$KEY_PATH" "$EC2_USER@$EC2_IP"

