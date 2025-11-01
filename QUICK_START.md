# 🚀 Quick Start: Fixing Your Production Deployment

## Is Your Web UI Not Working on Railway?

If you're seeing `https://justwatch-search-production.up.railway.app/` not loading or showing errors, here's the **quick fix**:

### 3-Step Fix ⚡

#### 1. Get Your Proxy URL
- Go to Railway Dashboard
- Open your **Proxy Service** (the one with `SERVICE=proxy`)
- Copy the **Public Domain** URL (e.g., `https://your-proxy-abc123.up.railway.app`)

#### 2. Set Environment Variable
- Go to your **Frontend Service** (the one with `SERVICE=frontend`)
- Click **Variables** tab
- Add new variable:
  ```
  PROXY_URL=https://your-proxy-abc123.up.railway.app
  ```
  *(Use your actual proxy URL from step 1)*

#### 3. Redeploy
- Railway will automatically redeploy
- Wait for deployment to complete
- ✅ Your web UI should now work!

---

## Need More Help?

### Detailed Guides
- **[FIXING_PRODUCTION.md](FIXING_PRODUCTION.md)** - Step-by-step with troubleshooting
- **[ARCHITECTURE_DIAGRAM.md](ARCHITECTURE_DIAGRAM.md)** - Visual explanation
- **[SOLUTION_SUMMARY.md](SOLUTION_SUMMARY.md)** - Complete technical details

### Documentation
- **[README.md](README.md)** - Full project documentation
- **[RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md)** - Railway deployment guide

---

## Common Questions

### Q: Do I need to switch from C# to JavaScript?
**A: NO!** C# Blazor WebAssembly is great. This is just a configuration issue.

### Q: Will this break my local development?
**A: NO!** Local development continues to use `localhost:8080` as before.

### Q: What if I don't have a proxy service?
**A: You need both services!** Deploy the proxy service first, then the frontend.

### Q: My frontend still doesn't work after setting PROXY_URL
**A: Check these:**
- Proxy service is running
- PROXY_URL has no typos
- PROXY_URL has no trailing slash
- Both services are in the same Railway project

---

## What Was Changed?

This PR adds automatic proxy URL configuration:
- ✅ Build script (`update-appsettings.sh`) that injects `PROXY_URL` into config
- ✅ Updated build process to run the script
- ✅ Complete documentation
- ✅ No code changes needed - just set the environment variable!

---

## Summary

**Before:** Frontend hardcoded to `http://localhost:8080` ❌  
**After:** Frontend reads from `PROXY_URL` environment variable ✅

**Action Required:** Set `PROXY_URL` in your Railway frontend service

That's it! 🎉
