# Fixing the Production Deployment

## Problem
The web UI at `https://justwatch-search-production.up.railway.app/` is not working because the frontend is trying to connect to `http://localhost:8080` which doesn't exist in production.

## Solution
You need to set the `PROXY_URL` environment variable in your Railway frontend service to point to your deployed proxy service.

## Steps to Fix

### 1. Find Your Proxy Service URL
1. Go to your Railway dashboard
2. Open the **proxy service** (the one with `SERVICE=proxy`)
3. Look at the **Settings** tab and find the **Public Domain** (it will look like `https://your-proxy-name.up.railway.app`)
4. Copy this URL

### 2. Configure the Frontend Service
1. In Railway, open your **frontend service** (the one with `SERVICE=frontend`)
2. Go to the **Variables** tab
3. Add a new environment variable:
   - **Name:** `PROXY_URL`
   - **Value:** The proxy service URL you copied (e.g., `https://your-proxy-name.up.railway.app`)
4. Click **Add** or **Save**

### 3. Redeploy the Frontend
1. After adding the variable, Railway should automatically redeploy
2. If not, go to the **Deployments** tab and click **Deploy** to trigger a new deployment
3. During the build, the `update-appsettings.sh` script will automatically inject the proxy URL into the configuration

### 4. Verify It's Working
1. Wait for the deployment to complete
2. Open your frontend URL (e.g., `https://justwatch-search-production.up.railway.app/`)
3. You should now see the app working with upcoming releases and popular titles
4. Try searching for a movie to verify the connection to the proxy service

## Example Configuration

If your services are deployed like this:
- **Proxy Service URL:** `https://justwatch-proxy-prod.up.railway.app`
- **Frontend Service URL:** `https://justwatch-search-production.up.railway.app`

Then in the **Frontend Service**, set:
```
PROXY_URL=https://justwatch-proxy-prod.up.railway.app
```

## Important Notes

- **Do NOT include** a trailing slash in the `PROXY_URL`
- **Make sure** both services are running before testing
- **The proxy service** does not need the `PROXY_URL` variable (only the frontend needs it)
- **Local development** will continue to use `http://localhost:8080` by default

## Troubleshooting

### Frontend still shows errors after deployment
- Check that the `PROXY_URL` variable is set correctly (no typos)
- Verify the proxy service is running and accessible
- Check the build logs to see if `update-appsettings.sh` ran successfully
- You should see: "Updating appsettings.json with ProxyUrl: [your-url]"

### Can't find the proxy service URL
- Go to Railway dashboard → Your Project → Proxy Service
- Look for the **Public Networking** section
- If no domain is shown, you may need to generate one in the Settings

### Both services are running but still not connecting
- Verify CORS is enabled in the proxy service (it should be by default)
- Check the browser console for any error messages
- Ensure the proxy URL uses HTTPS (not HTTP) in production

## Need Help?
If you're still having issues, check:
1. Railway deployment logs for both services
2. Browser console for frontend errors
3. Network tab to see if requests to the proxy are being made
