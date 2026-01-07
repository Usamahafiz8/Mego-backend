#!/bin/bash

# Upload Code to EC2 Script

EC2_IP="3.91.9.196"
EC2_USER="ubuntu"
KEY_PATH="$HOME/.ssh/mego-api.pem"
LOCAL_PATH="/Users/cybillnerd/Desktop/mego/mego-api (1)"
REMOTE_PATH="/home/ubuntu/mego-api"

echo "📤 Uploading Code to EC2"
echo "========================"
echo ""
echo "From: $LOCAL_PATH"
echo "To: $EC2_USER@$EC2_IP:$REMOTE_PATH"
echo ""

# Check if key exists
if [ ! -f "$KEY_PATH" ]; then
    echo "❌ Key file not found at: $KEY_PATH"
    read -p "Key path: " KEY_PATH
fi

chmod 400 "$KEY_PATH"

echo "Uploading..."
echo ""

# Upload code
scp -i "$KEY_PATH" -r "$LOCAL_PATH" "$EC2_USER@$EC2_IP:$REMOTE_PATH"

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Upload successful!"
    echo ""
    echo "Next steps on EC2:"
    echo "  cd $REMOTE_PATH/MeGo.Api"
    echo "  ./deploy-to-ec2.sh"
else
    echo ""
    echo "❌ Upload failed"
fi

