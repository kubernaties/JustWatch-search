#!/bin/bash
set -e

# Determine which service to run based on the SERVICE environment variable
# If not set, default to running the proxy server
SERVICE=${SERVICE:-proxy}

if [ "$SERVICE" = "proxy" ]; then
    echo "Starting JustWatch Proxy Server..."
    cd JustWatchProxy
    dotnet run --project JustWatchProxy.csproj --urls "http://0.0.0.0:${PORT:-8080}"
elif [ "$SERVICE" = "frontend" ]; then
    echo "Starting JustWatch Frontend..."
    cd JustWatchSearch
    dotnet run --project JustWatchSearch.csproj
else
    echo "Unknown service: $SERVICE"
    echo "Please set SERVICE environment variable to 'proxy' or 'frontend'"
    exit 1
fi
