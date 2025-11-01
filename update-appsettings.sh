#!/bin/bash
# Update appsettings.json with ProxyUrl from environment variable if frontend service

if [ "$SERVICE" = "frontend" ]; then
    if [ -n "$PROXY_URL" ]; then
        echo "Updating appsettings.json with ProxyUrl: $PROXY_URL"
        cat > JustWatchSearch/wwwroot/appsettings.json << EOF
{
  "ProxyUrl": "$PROXY_URL"
}
EOF
        echo "appsettings.json updated successfully"
    else
        echo "Warning: PROXY_URL environment variable not set for frontend service"
        echo "Using default localhost configuration"
    fi
else
    echo "Not a frontend service, skipping appsettings.json update"
fi
