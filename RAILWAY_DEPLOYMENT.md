# Railway Deployment Quick Guide

This guide provides a quick reference for deploying the JustWatch Search application to Railway.

## Files Overview

The repository includes the following Railway configuration files:

- **`start.sh`** - Startup script that runs either the proxy or frontend service
- **`nixpacks.toml`** - Build configuration for Railway's Nixpacks builder
- **`railway.json`** - Railway-specific deployment settings
- **`Procfile`** - Process definition file

## Quick Deployment Steps

### Option 1: Using SERVICE Environment Variable (Recommended)

Deploy both services from the same repository:

**Proxy Service:**
1. Create a new service on Railway
2. Connect to this GitHub repository
3. Set environment variable: `SERVICE=proxy`
4. Deploy

**Frontend Service:**
1. Add another service to the same Railway project
2. Connect to this GitHub repository
3. Set environment variables:
   - `SERVICE=frontend`
   - `PROXY_URL=<your-proxy-service-url>` (e.g., `https://your-proxy.railway.app`)
4. Deploy

### Option 2: Using Root Directory

**Proxy Service:**
1. Create a new service on Railway
2. Connect to this GitHub repository
3. Set **Root Directory**: `JustWatchProxy`
4. Set **Start Command**: `dotnet run --project JustWatchProxy.csproj --urls "http://0.0.0.0:${PORT:-8080}"`
5. Deploy

**Frontend Service:**
1. Add another service to the same Railway project
2. Connect to this GitHub repository
3. Set **Root Directory**: `JustWatchSearch`
4. Set environment variable: `PROXY_URL=<your-proxy-service-url>` (e.g., `https://your-proxy.railway.app`)
5. Set **Start Command**: `dotnet run --project JustWatchSearch.csproj`
6. Deploy

## Environment Variables

### Proxy Service
- `SERVICE=proxy` (if using Option 1)
- `PORT` - Automatically set by Railway

### Frontend Service
- `SERVICE=frontend` (if using Option 1)
- `PROXY_URL` - URL of your deployed proxy service (e.g., `https://your-proxy.railway.app`)

## Build Process

Railway uses Nixpacks to automatically:
1. Install .NET 8 SDK and Runtime
2. Restore NuGet packages for both projects
3. Build both projects in Release mode
4. Run the `start.sh` script

## Troubleshooting

### Build Fails
- Check that all environment variables are set correctly
- Verify the `start.sh` script is executable (should be by default)
- Check Railway build logs for specific errors

### Service Won't Start
- Ensure `SERVICE` environment variable is set to either `proxy` or `frontend`
- For proxy: Railway's `PORT` variable should be automatically set
- For frontend: `ProxyUrl` must be set to your deployed proxy service URL

### Frontend Can't Connect to Proxy
- Verify the `PROXY_URL` environment variable is set correctly in the frontend service
- Ensure the proxy service is running and accessible
- Check that CORS is properly configured in the proxy service
- Verify that `appsettings.json` was updated during build with the correct proxy URL

## Testing Locally

You can test the Railway configuration locally:

```bash
# Test proxy service
SERVICE=proxy PORT=8080 ./start.sh

# Test frontend service (in another terminal)
SERVICE=frontend ./start.sh
```

## Additional Resources

- [Railway Documentation](https://docs.railway.app/)
- [Nixpacks Documentation](https://nixpacks.com/)
- [.NET on Railway](https://docs.railway.app/languages/dotnet)

## Notes

- Both services must be running for the application to work properly
- The proxy service provides the API endpoints that the frontend consumes
- Railway's free tier includes 500 hours of usage per month
- Each service gets its own public URL on Railway
