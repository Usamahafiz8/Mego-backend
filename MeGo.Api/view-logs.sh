#!/bin/bash

# View MeGo API Backend Logs Script

echo "📋 MeGo API - View Logs"
echo "======================="
echo ""

# Find the backend process
BACKEND_PID=$(lsof -ti:5144 2>/dev/null)

if [ -z "$BACKEND_PID" ]; then
    echo "❌ Backend is not running on port 5144"
    echo ""
    echo "Start the backend first:"
    echo "  cd MeGo.Api"
    echo "  ./start.sh"
    exit 1
fi

echo "✅ Backend is running (PID: $BACKEND_PID)"
echo ""
echo "📊 Log Options:"
echo ""
echo "1. View recent logs (last 50 lines)"
echo "2. Follow logs in real-time (Ctrl+C to stop)"
echo "3. View logs with timestamps"
echo "4. View error logs only"
echo "5. View all logs"
echo ""
read -p "Select option [1-5]: " OPTION

case $OPTION in
    1)
        echo ""
        echo "📋 Recent logs (last 50 lines):"
        echo "================================"
        # Try to get logs from the process
        # Note: If running in background, logs might not be accessible this way
        # Better to check if there's a log file or restart with logging
        echo "⚠️  Logs are output to the terminal where dotnet run was started."
        echo ""
        echo "To see logs, restart the backend in foreground:"
        echo "  cd MeGo.Api"
        echo "  ./start.sh"
        ;;
    2)
        echo ""
        echo "📋 Following logs in real-time..."
        echo "Press Ctrl+C to stop"
        echo "================================"
        # This would require the process to be running in foreground
        echo "⚠️  To follow logs, restart backend in foreground mode"
        ;;
    3)
        echo ""
        echo "📋 Logs with timestamps:"
        echo "================================"
        # Check for log files
        if [ -f "logs/app.log" ]; then
            tail -f logs/app.log | while read line; do echo "[$(date '+%Y-%m-%d %H:%M:%S')] $line"; done
        else
            echo "No log file found. Logs are output to console."
        fi
        ;;
    4)
        echo ""
        echo "📋 Error logs only:"
        echo "================================"
        if [ -f "logs/app.log" ]; then
            grep -i "error\|exception\|fail" logs/app.log | tail -20
        else
            echo "No log file found. Configure file logging first."
        fi
        ;;
    5)
        echo ""
        echo "📋 All logs:"
        echo "================================"
        if [ -f "logs/app.log" ]; then
            cat logs/app.log
        else
            echo "No log file found. Logs are output to console."
        fi
        ;;
    *)
        echo "Invalid option"
        exit 1
        ;;
esac

echo ""
echo "💡 Tip: To see logs in real-time, run the backend in foreground:"
echo "   cd MeGo.Api"
echo "   ./start.sh"

