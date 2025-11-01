# Architecture & Deployment Flow

## Local Development

```
┌─────────────────────────────────────────────────────────────┐
│                    Your Local Machine                        │
│                                                              │
│  ┌──────────────────┐              ┌──────────────────┐     │
│  │   Frontend App   │              │   Proxy Server   │     │
│  │  (Blazor WASM)   │              │  (ASP.NET Core)  │     │
│  │                  │──────────────▶│                  │     │
│  │ localhost:5000   │   GraphQL    │ localhost:8080   │     │
│  │                  │   Requests   │                  │     │
│  └──────────────────┘              └──────────────────┘     │
│         │                                    │               │
│         │                                    │               │
│         │ appsettings.json:                  │               │
│         │ "ProxyUrl": "http://localhost:8080"│               │
│         │                                    │               │
│         │                                    ▼               │
│         │                          ┌──────────────────┐      │
│         │                          │  JustWatch API   │      │
│         │                          │ (apis.justwatch) │      │
│         │                          └──────────────────┘      │
│         │                                                     │
│         ▼                                                     │
│   Browser renders                                            │
│   the UI                                                     │
└─────────────────────────────────────────────────────────────┘
```

## Production Deployment (Railway)

### Before Fix (Broken ❌)
```
┌──────────────────────────────────────────────────────────────┐
│                    Railway Platform                           │
│                                                               │
│  ┌──────────────────┐              ┌──────────────────┐      │
│  │   Frontend App   │      ❌      │   Proxy Server   │      │
│  │  (Blazor WASM)   │   Can't      │  (ASP.NET Core)  │      │
│  │                  │   Connect    │                  │      │
│  │ frontend.up.     │              │ proxy.up.        │      │
│  │ railway.app      │              │ railway.app      │      │
│  └──────────────────┘              └──────────────────┘      │
│         │                                    │                │
│         │                                    │                │
│         │ ❌ Looking for:                    │                │
│         │ "http://localhost:8080"            │                │
│         │ (doesn't exist!)                   │                │
│         │                                    ▼                │
│         │                          ┌──────────────────┐       │
│         │                          │  JustWatch API   │       │
│         │                          │ (apis.justwatch) │       │
│         │                          └──────────────────┘       │
│         │                                                      │
│         ▼                                                      │
│   User sees errors                                            │
│   or loading spinner                                          │
└──────────────────────────────────────────────────────────────┘
```

### After Fix (Working ✅)
```
┌──────────────────────────────────────────────────────────────┐
│                    Railway Platform                           │
│                                                               │
│  ┌──────────────────┐              ┌──────────────────┐      │
│  │   Frontend App   │      ✅      │   Proxy Server   │      │
│  │  (Blazor WASM)   │──────────────▶│  (ASP.NET Core)  │      │
│  │                  │   GraphQL    │                  │      │
│  │ frontend.up.     │   Requests   │ proxy.up.        │      │
│  │ railway.app      │              │ railway.app      │      │
│  └──────────────────┘              └──────────────────┘      │
│         │                                    │                │
│         │                                    │                │
│         │ ✅ Configured with:                │                │
│         │ PROXY_URL environment var          │                │
│         │ → "https://proxy.up.railway.app"   │                │
│         │ (injected during build)            │                │
│         │                                    ▼                │
│         │                          ┌──────────────────┐       │
│         │                          │  JustWatch API   │       │
│         │                          │ (apis.justwatch) │       │
│         │                          └──────────────────┘       │
│         │                                                      │
│         ▼                                                      │
│   User sees working UI                                        │
│   with movie data                                             │
└──────────────────────────────────────────────────────────────┘
```

## Build Process Flow

```
┌─────────────────────────────────────────────────────────────┐
│                  Railway Build Process                        │
│                                                              │
│  1. Railway detects SERVICE environment variable             │
│                                                              │
│  2. Nixpacks runs build commands:                            │
│     ├─ dotnet restore                                        │
│     ├─ dotnet build                                          │
│     └─ ./update-appsettings.sh                               │
│                                                              │
│  3. update-appsettings.sh script checks:                     │
│                                                              │
│     If SERVICE = "proxy":                                    │
│     ├─ Skip (proxy doesn't need frontend config)            │
│     └─ Continue to start proxy                               │
│                                                              │
│     If SERVICE = "frontend":                                 │
│     ├─ Check if PROXY_URL is set                            │
│     ├─ If yes: Update appsettings.json with PROXY_URL       │
│     ├─ If no: Warning + use localhost (for local dev)       │
│     └─ Continue to start frontend                            │
│                                                              │
│  4. Application starts with correct configuration            │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Environment Variables

### Proxy Service (Railway)
```
SERVICE=proxy
PORT=<auto-assigned by Railway>
```

### Frontend Service (Railway)
```
SERVICE=frontend
PROXY_URL=https://your-proxy.up.railway.app  ← YOU MUST SET THIS!
```

### Local Development
```
No environment variables needed
Uses default appsettings.json with localhost:8080
```

## Key Files

| File | Purpose |
|------|---------|
| `update-appsettings.sh` | Injects PROXY_URL into configuration during build |
| `nixpacks.toml` | Defines the build process for Railway |
| `start.sh` | Determines which service to start (proxy or frontend) |
| `appsettings.json` | Frontend configuration (default: localhost) |
| `FIXING_PRODUCTION.md` | Step-by-step fix instructions |

## The Technology Stack is Perfect!

C# Blazor WebAssembly is an excellent choice:
- Runs entirely in browser (like React/Vue)
- Strongly typed with great tooling
- Fast performance
- Easy to maintain

**You don't need to switch to JavaScript!** This was just a configuration issue.
