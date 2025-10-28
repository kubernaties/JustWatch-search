
# JustWatch Search

A web application that allows users to search for movies and TV shows on JustWatch and fetches all available streaming offers across all countries.

## 🎯 Features

- Search for movies and TV shows from the JustWatch database
- View streaming availability across multiple countries
- Compare prices and platforms
- Modern, responsive UI built with Blazor WebAssembly
- CORS-enabled proxy server for API access

## 🏗️ Architecture

This project consists of two main components:

1. **JustWatchProxy** - A C# ASP.NET Core API that acts as a proxy to the JustWatch API
2. **JustWatchSearch** - A Blazor WebAssembly frontend application

## 📋 Prerequisites

Before running this application, you need to have installed:

- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** - Framework for building and running the application
- **(Optional) [Docker](https://www.docker.com/get-started)** - For containerized deployment

### Verify .NET Installation

Check if .NET is installed and view the version:

```bash
dotnet --version
```

Expected output: `8.0.x` (where x is the patch version)

## 🚀 Running Locally

The application requires **two terminal windows** running simultaneously - one for the proxy server and one for the frontend.

### Step 1: Start the Proxy Server

The proxy server handles API requests and bypasses CORS restrictions.

```bash
# Navigate to the proxy directory
cd JustWatchProxy

# Restore dependencies (first time only)
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

✅ **Keep this terminal running!**

### Step 2: Start the Frontend Application

Open a **new terminal window** and run:

```bash
# Navigate to the main application directory
cd JustWatchSearch

# Restore dependencies (first time only)
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

### Step 3: Access the Application

Open your web browser and navigate to:

```
http://localhost:5000
```

or

```
https://localhost:5001
```

(Use whichever URL is shown in your terminal)

## 🐳 Running with Docker

### Pull and Run Pre-built Image

```bash
# Pull the latest image from GitHub Container Registry
docker pull ghcr.io/nobraincellsleft/justwatch-search:latest

# Run the container (maps port 8080 on your machine to port 80 in the container)
docker run -p 8080:80 ghcr.io/nobraincellsleft/justwatch-search:latest
```

Then open: **http://localhost:8080**

### Build and Run Locally

```bash
# Build the Docker image with a custom tag
docker build -t justwatch-search .

# Run the container
docker run -p 8080:80 justwatch-search
```

Then open: **http://localhost:8080**

## 📝 Command Reference

### Common .NET Commands

| Command | Description |
|---------|-------------|
| `dotnet --version` | Display the installed .NET SDK version |
| `dotnet restore` | Download and install project dependencies |
| `dotnet build` | Compile the project without running it |
| `dotnet run` | Compile and run the project |
| `dotnet run --urls "http://localhost:8080"` | Run on a specific port |
| `dotnet clean` | Remove build outputs |
| `dotnet publish -c Release` | Create production-ready build |

### Common Docker Commands

| Command | Description |
|---------|-------------|
| `docker build -t <name> .` | Build a Docker image from Dockerfile |
| `docker run -p <host>:<container> <image>` | Run a container and map ports |
| `docker ps` | List running containers |
| `docker stop <container-id>` | Stop a running container |
| `docker images` | List available images |
| `docker pull <image>` | Download an image from registry |

### Port Management (Linux/macOS)

| Command | Description |
|---------|-------------|
| `lsof -i :8080` | Check what's using port 8080 |
| `sudo lsof -i :5000` | Check what's using port 5000 (requires sudo) |
| `kill -9 <PID>` | Force stop a process by its ID |

## 🔧 Configuration

### Proxy Server Endpoints

The proxy server (`JustWatchProxy`) exposes the following endpoints:

- **POST** `/graphql` - GraphQL queries to JustWatch API
- **GET** `/content/urls?path=<path>` - Fetch content metadata
- **GET** `/health` - Health check endpoint

### Frontend Configuration

The frontend is configured to connect to the proxy at `http://localhost:8080`. This is set in:

```
JustWatchSearch/Services/JustWatch/JustwatchApiService.cs
```

## 🛠️ Technology Stack

- **C# / .NET 8.0** - Backend and frontend framework
- **ASP.NET Core** - Web API framework for the proxy
- **Blazor WebAssembly** - Frontend SPA framework
- **Bootstrap 5** - UI styling
- **GraphQL** - API communication with JustWatch
- **Nginx** - Web server (Docker deployment)

## 📂 Project Structure

```
JustWatch-Search/
├── JustWatchProxy/              # Proxy server project
│   ├── Program.cs               # Main proxy server code
│   └── JustWatchProxy.csproj    # Proxy project file
│
├── JustWatchSearch/             # Frontend application
│   ├── Program.cs               # Application entry point
│   ├── Services/                # API services and business logic
│   ├── wwwroot/                 # Static files (HTML, CSS, JS)
│   └── JustWatchSearch.csproj   # Frontend project file
│
├── Dockerfile                   # Docker build instructions
├── nginx.conf                   # Nginx configuration
└── JustWatchSearch.sln          # Solution file
```

## 🐛 Troubleshooting

### Port Already in Use

**Error:** `Address already in use`

**Solution:** Either kill the process using the port or run on a different port

```bash
# Find what's using port 8080
lsof -i :8080

# Kill the process (replace PID with actual process ID)
kill -9 <PID>

# Or run on a different port
dotnet run --urls "http://localhost:9000"
```

### 404 Error on Proxy

**Issue:** Getting 404 when accessing `http://localhost:8080`

**Explanation:** This is normal! The proxy doesn't have a homepage. It only responds to:
- `/graphql` (POST)
- `/content/urls` (GET)
- `/health` (GET)

**Test the proxy:**
```bash
curl http://localhost:8080/health
```

Expected response:
```json
{"status":"healthy","timestamp":"2025-10-28T10:26:43Z"}
```

### CORS Errors

**Issue:** CORS policy errors in browser console

**Solution:** Make sure the proxy server is running **before** starting the frontend application. The proxy must be on port 8080.

### Frontend Won't Start

**Error:** Application won't compile or run

**Solution:**
```bash
# Clean the build
dotnet clean

# Restore packages
dotnet restore

# Try running again
dotnet run
```

## 📄 License

This project is licensed under the GNU General Public License v3.0 - see the LICENSE file for details.

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!

## 👤 Author

**ryan6t4**
- GitHub: [@ryan6t4](https://github.com/ryan6t4)

## ⭐ Show Your Support

Give a ⭐️ if this project helped you!

---

## 📌 Quick Start Cheatsheet

### First Time Setup
```bash
# 1. Clone the repository
git clone https://github.com/ryan6t4/JustWatch-search.git
cd JustWatch-search

# 2. Terminal 1 - Start Proxy
cd JustWatchProxy
dotnet restore
dotnet run --urls "http://localhost:8080"

# 3. Terminal 2 - Start Frontend (new terminal)
cd JustWatchSearch
dotnet restore
dotnet run

# 4. Open browser to http://localhost:5000
```

### Subsequent Runs
```bash
# Terminal 1
cd JustWatchProxy
dotnet run --urls "http://localhost:8080"

# Terminal 2 (new terminal)
cd JustWatchSearch
dotnet run
```

### Docker (Alternative)
```bash
docker pull ghcr.io/nobraincellsleft/justwatch-search:latest
docker run -p 8080:80 ghcr.io/nobraincellsleft/justwatch-search:latest
# Open http://localhost:8080
```
