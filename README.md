# JustWatch Search

A web application that allows users to search for movies and TV shows on JustWatch and fetches all available streaming offers across all countries.

## 🎯 Features

- **Homepage Display**: See upcoming Hollywood movie & TV series releases and popular titles by default
- **Release Date Information**: View release dates with countdown (days until release)
- **Search Functionality**: Search for movies and TV shows from the JustWatch database
- **Streaming Availability**: View streaming availability across multiple countries
- **Price Comparison**: Compare prices and platforms
- **Modern UI**: Responsive UI built with Blazor WebAssembly
- **CORS-enabled Proxy**: Server for API access
- **Railway Deployment Ready**: Easy deployment to Railway without Docker

## 🏗️ Architecture

This project consists of two main components:

1. **JustWatchProxy** - A C# ASP.NET Core API that acts as a proxy to the JustWatch API
   - Configurable port via `PORT` environment variable (default: 8080)
   - Supports deployment on platforms like Railway with dynamic port assignment
2. **JustWatchSearch** - A Blazor WebAssembly frontend application
   - Configurable proxy URL via `appsettings.json`
   - Default local development: http://localhost:5000

Both components must run simultaneously for the application to work. For production deployments (like Railway), they can be hosted separately or together.

---

## 📋 Prerequisites & Installation

### Step 1: Install .NET 8.0 SDK

Choose the installation method for your operating system:

#### Windows

**Option 1: Using Winget (Recommended)**

```powershell
# Open PowerShell or Command Prompt

# Install .NET SDK 8.0
winget install Microsoft.DotNet.SDK.8

# Verify installation
dotnet --version
```

**Option 2: Manual Download**

1. Download from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Run the downloaded `.exe` installer
3. Follow the installation wizard
4. Open a new terminal and verify:

```cmd
dotnet --version
```

**Expected output:** `8.0.xxx`

---

#### Linux (Ubuntu/Debian)

```bash
# Download Microsoft package repository configuration
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

# Install the repository configuration
sudo dpkg -i packages-microsoft-prod.deb

# Clean up
rm packages-microsoft-prod.deb

# Update package list
sudo apt-get update

# Install .NET SDK 8.0
sudo apt-get install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
```

**Expected output:** `8.0.xxx`

---

#### macOS

```bash
# Install Homebrew if not already installed
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install .NET SDK
brew install --cask dotnet-sdk

# Verify installation
dotnet --version
```

**Expected output:** `8.0.xxx`

---

### Step 2: Clone the Repository

**All Platforms:**

```bash
# Clone the repository
git clone https://github.com/ryan6t4/JustWatch-search.git

# Navigate to the project directory
cd JustWatch-search
```

---

## 🚀 Running the Application Locally

### Complete Setup (First Time)

You need **TWO terminal windows** running simultaneously.

#### Terminal 1: Start the Proxy Server

```bash
# Navigate to the proxy directory
cd JustWatchProxy

# Restore NuGet packages (downloads dependencies)
dotnet restore

# Run the proxy server on port 8080
dotnet run --urls "http://localhost:8080"
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:8080
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

✅ **Keep this terminal running!** Do NOT close it.

> **Note:** If you see 404 errors when accessing `http://localhost:8080/` directly, this is normal. The proxy server doesn't have a homepage - it only responds to specific API endpoints.

---

#### Terminal 2: Start the Frontend Application

Open a **NEW terminal window** (keep Terminal 1 running!) and execute:

```bash
# Navigate to the frontend directory
cd JustWatchSearch

# Restore NuGet packages
dotnet restore

# Run the Blazor WebAssembly application
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

#### Step 3: Access the Application

Open your web browser and navigate to:

```
http://localhost:5000
```

or (if HTTPS is configured):

```
https://localhost:5001
```

🎉 **You should now see the JustWatch Search application!**

---

### Subsequent Runs

After the first setup, you only need to run the applications:

**Terminal 1 - Proxy Server:**
```bash
cd JustWatchProxy
dotnet run --urls "http://localhost:8080"
```

**Terminal 2 - Frontend:**
```bash
cd JustWatchSearch
dotnet run
```

Then open **http://localhost:5000** in your browser.

---

## 🚂 Deploying to Railway

Railway is a modern platform that makes deploying .NET applications simple, with automatic port detection and no Docker required.

This project includes Railway configuration files (`start.sh`, `nixpacks.toml`, `railway.json`, and `Procfile`) that enable automatic deployment.

### Prerequisites

1. A [Railway account](https://railway.app/) (free tier available)
2. [Railway CLI](https://docs.railway.app/develop/cli) (optional, but recommended)

### Deployment Architecture

This application consists of two services that can be deployed separately on Railway:

1. **JustWatchProxy** - Backend API service (runs on port assigned by Railway's `PORT` variable)
2. **JustWatchSearch** - Frontend Blazor WebAssembly app

### Deployment Steps

#### Option 1: Deploy via Railway Web UI (Recommended)

**Step 1: Deploy the Proxy Service**

1. **Create a New Project on Railway**
   - Go to [Railway Dashboard](https://railway.app/dashboard)
   - Click "New Project"
   - Select "Deploy from GitHub repo"
   - Connect and select your `JustWatch-search` repository

2. **Configure the Proxy Service**
   - Railway will auto-detect the .NET application using the provided configuration files
   - Set the environment variable `SERVICE` to `proxy`
   - Railway will automatically set the `PORT` environment variable
   - Click "Deploy"
   - Note the public URL assigned to this service (e.g., `https://your-proxy.railway.app`)

**Step 2: Deploy the Frontend Service**

1. **Add a New Service to the Same Project**
   - In your Railway project, click "New Service"
   - Select "GitHub Repo" and choose the same repository
   
2. **Configure the Frontend Service**
   - Set the environment variable `SERVICE` to `frontend`
   - Update the frontend to point to your proxy service:
     - Either set an environment variable `ProxyUrl` to your proxy service URL
     - Or update `JustWatchSearch/wwwroot/appsettings.json` with the proxy URL before deploying
   - Click "Deploy"

3. **Access Your Application**
   - The frontend will be accessible at its Railway-assigned URL
   - Both services are now running and communicating

#### Option 2: Deploy via Railway CLI

```bash
# Install Railway CLI (if not already installed)
npm i -g @railway/cli

# Login to Railway
railway login

# Initialize project
railway init

# Deploy Proxy Service
railway up
# Then set SERVICE=proxy in Railway dashboard

# For the frontend, create another service in the same project via the dashboard
# Set SERVICE=frontend and configure the ProxyUrl
```

#### Option 3: Deploy Using Root Directory (Alternative)

Instead of using the `SERVICE` environment variable, you can deploy each service separately by setting the **Root Directory**:

**For Proxy Service:**
- Set **Root Directory** to `JustWatchProxy`
- Set **Start Command**: `dotnet run --project JustWatchProxy.csproj --urls "http://0.0.0.0:${PORT:-8080}"`

**For Frontend Service:**
- Set **Root Directory** to `JustWatchSearch`
- Set **Start Command**: `dotnet run --project JustWatchSearch.csproj`
- Set environment variable `ProxyUrl` to your proxy service URL

### Configuration Files

This repository includes the following Railway configuration files:

- **`start.sh`**: Startup script that determines which service to run based on the `SERVICE` environment variable
- **`nixpacks.toml`**: Build configuration for Railway's Nixpacks builder
- **`railway.json`**: Railway-specific deployment configuration
- **`Procfile`**: Process file for service startup

### Environment Variables

**Proxy Service:**
- `SERVICE=proxy` - Tells the startup script to run the proxy server
- `PORT` - Automatically set by Railway (default: 8080)

**Frontend Service:**
- `SERVICE=frontend` - Tells the startup script to run the frontend
- `ProxyUrl` - URL of your deployed proxy service (e.g., `https://your-proxy.railway.app`)

### Configuration for Railway

The application is already configured to work with Railway:

- **Proxy Server**: Reads port from `PORT` environment variable (Railway auto-assigns this)
- **Frontend**: Reads proxy URL from `appsettings.json` or environment configuration
- **Nixpacks**: Automatically installs .NET 8 SDK and builds both projects

### Important Notes

- Railway automatically assigns ports - no need for Docker port mapping
- Both services need to be running for the application to work
- Update the `ProxyUrl` in the frontend's configuration to point to your deployed proxy service
- Railway's free tier includes 500 hours of usage per month (across all projects)
- The `start.sh` script is executable and will be run by Railway on startup

---

## 📝 Command Reference

### .NET CLI Commands

| Command | Description |
|---------|-------------|
| `dotnet --version` | Display installed .NET SDK version |
| `dotnet restore` | Download and install project dependencies |
| `dotnet build` | Compile the project |
| `dotnet run` | Compile and run the project |
| `dotnet run --urls "http://localhost:8080"` | Run on a specific port |
| `dotnet clean` | Remove build outputs |
| `dotnet watch run` | Run with hot reload (auto-restart on changes) |

### Port Management Commands

#### Windows (PowerShell/Command Prompt)

```powershell
# Find what's using port 8080
netstat -ano | findstr :8080

# Kill a process by PID
taskkill /PID <PID> /F
```

#### Linux/macOS

```bash
# Find what's using port 8080
lsof -i :8080

# Kill a process by PID
kill -9 <PID>
```

---

## 🔧 Configuration

### Proxy Server Endpoints

The proxy server (port 8080) exposes:

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/graphql` | POST | GraphQL queries to JustWatch API |
| `/content/urls?path=<path>` | GET | Fetch content metadata |
| `/health` | GET | Health check endpoint |

### Test the Proxy

**Windows (PowerShell):**
```powershell
Invoke-WebRequest -Uri http://localhost:8080/health
```

**Linux/macOS:**
```bash
curl http://localhost:8080/health
```

**Expected response:**
```json
{"status":"healthy","timestamp":"2025-10-28T10:32:33Z"}
```

### Frontend Configuration

The frontend connects to the proxy at `http://localhost:8080`.

**Configuration file:**
```
JustWatchSearch/Services/JustWatch/JustwatchApiService.cs
Line 23: private readonly string _baseAddress = "http://localhost:8080";
```

---

## 🛠️ Technology Stack

- **C# / .NET 8.0** - Backend and frontend framework
- **ASP.NET Core** - Web API framework for the proxy server
- **Blazor WebAssembly** - Frontend SPA framework
- **Bootstrap 5** - UI styling and components
- **GraphQL Client** - API communication with JustWatch
- **Nginx** - Web server (Docker deployment only)

---

## 📂 Project Structure

```
JustWatch-Search/
├── JustWatchProxy/              # Proxy server (Port 8080)
│   ├── Program.cs               # Main proxy server code
│   └── JustWatchProxy.csproj    # Project configuration
│
├── JustWatchSearch/             # Frontend app (Port 5000)
│   ├── Program.cs               # Application entry point
│   ├── Services/                # API services and business logic
│   │   ├── JustWatch/           # JustWatch API integration
│   │   └── CurrencyConverter.cs # Currency conversion
│   ├── Models/                  # Data models
│   ├── Pages/                   # Blazor pages/components
│   └── wwwroot/                 # Static files (HTML, CSS, JS)
│
├── Dockerfile                   # Docker build instructions
├── nginx.conf                   # Nginx configuration
├── JustWatchSearch.sln          # Visual Studio solution
└── README.md                    # This file
```

---

## 🐛 Troubleshooting

### Port Already in Use

**Error:** `Failed to bind to address http://localhost:8080: address already in use`

**Windows:**
```powershell
# Find the process
netstat -ano | findstr :8080

# Kill it (replace PID with actual number)
taskkill /PID <PID> /F
```

**Linux/macOS:**
```bash
# Find the process
lsof -i :8080

# Kill it (replace PID with actual number)
kill -9 <PID>
```

**Alternative:** Run on a different port
```bash
dotnet run --urls "http://localhost:9000"
```

---

### 404 Error on Proxy

**Issue:** Getting 404 when accessing `http://localhost:8080/`

**Explanation:** This is **normal**! The proxy doesn't have a homepage. Test with:

```bash
# Windows (PowerShell)
Invoke-WebRequest -Uri http://localhost:8080/health

# Linux/macOS
curl http://localhost:8080/health
```

---

### CORS Errors

**Issue:** CORS policy errors in browser

**Solution:** Make sure the proxy is running on port 8080 **before** starting the frontend.

**Checklist:**
1. ✅ Proxy running on `http://localhost:8080`
2. ✅ Frontend running on `http://localhost:5000`
3. ✅ Both terminals are open

---

### Build Errors

**Issue:** Application won't compile

**Solution:**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
dotnet run
```

---

### .NET Command Not Found

**Windows:**
1. Close and reopen terminal
2. If still not working, reinstall .NET SDK

**Linux/macOS:**
```bash
# Add to PATH
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

# Make permanent (add to ~/.bashrc or ~/.zshrc)
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
source ~/.bashrc
```

---

## 📄 License

This project is licensed under the GNU General Public License v3.0.

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 👤 Author

**ryan6t4**
- GitHub: [@ryan6t4](https://github.com/ryan6t4)

---

## ⭐ Show Your Support

Give a ⭐️ if this project helped you!

---

## 📌 Quick Start Cheatsheet

### First Time Setup (Local Development)

```bash
# 1. Clone
git clone https://github.com/ryan6t4/JustWatch-search.git
cd JustWatch-search

# 2. Terminal 1 - Start Proxy
cd JustWatchProxy
dotnet restore
dotnet run --urls "http://localhost:8080"

# 3. Terminal 2 - Start Frontend (new window)
cd JustWatchSearch
dotnet restore
dotnet run

# 4. Open http://localhost:5000 in browser
```

### Subsequent Runs (Local Development)

```bash
# Terminal 1
cd JustWatchProxy
dotnet run --urls "http://localhost:8080"

# Terminal 2 (new window)
cd JustWatchSearch
dotnet run
```

### Railway Deployment (Production)

```bash
# 1. Deploy Proxy Service
# - Set Root Directory: JustWatchProxy
# - Railway auto-assigns PORT

# 2. Deploy Frontend Service  
# - Set Root Directory: JustWatchSearch
# - Update appsettings.json with proxy URL
# - Deploy

# Both services get their own URLs
# Access via the frontend service URL
```

---

**Last Updated:** 2025-11-01
