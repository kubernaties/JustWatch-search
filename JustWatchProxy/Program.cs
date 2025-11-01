using System.Text;
using JustWatchProxy.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Get port from environment variable or use default
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add HttpClient for JustWatch API
builder.Services.AddHttpClient("JustWatchAPI", client =>
{
    client.BaseAddress = new Uri("https://apis.justwatch.com");
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();

// GraphQL Proxy Endpoint
app.MapPost("/graphql", async (HttpContext context, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    try
    {
        if (context.Request.Body == null)
        {
            logger.LogWarning("GraphQL request received with null body");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Request body is required" });
            return;
        }

        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync();
        
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            logger.LogWarning("GraphQL request received with empty body");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Request body cannot be empty" });
            return;
        }
        
        logger.LogInformation("Proxying GraphQL request to JustWatch API");

        var client = httpClientFactory.CreateClient("JustWatchAPI");
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync("/graphql", content);
        
        if (response == null)
        {
            logger.LogError("Received null response from JustWatch API");
            context.Response.StatusCode = 502;
            await context.Response.WriteAsJsonAsync(new { error = "No response from upstream API" });
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsync(responseBody);
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "HTTP error while proxying GraphQL request");
        context.Response.StatusCode = 502;
        await context.Response.WriteAsJsonAsync(new { error = "Failed to connect to upstream API", message = ex.Message });
    }
    catch (TaskCanceledException ex)
    {
        logger.LogError(ex, "GraphQL request timed out");
        context.Response.StatusCode = 504;
        await context.Response.WriteAsJsonAsync(new { error = "Request timed out", message = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error proxying GraphQL request");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Internal server error", message = ex.Message });
    }
});

// Content URLs Proxy Endpoint
app.MapGet("/content/urls", async (HttpContext context, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    try
    {
        var path = context.Request.Query["path"].ToString();
        if (string.IsNullOrWhiteSpace(path))
        {
            logger.LogWarning("Content URL request received with missing or empty path parameter");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid path parameter" });
            return;
        }

        logger.LogInformation("Proxying content URL request for path: {Path}", path);

        var client = httpClientFactory.CreateClient("JustWatchAPI");
        
        if (client == null)
        {
            logger.LogError("Failed to create HTTP client for JustWatch API");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Internal server error - HTTP client unavailable" });
            return;
        }

        var response = await client.GetAsync($"/content/urls?path={Uri.EscapeDataString(path)}");
        
        if (response == null)
        {
            logger.LogError("Received null response from JustWatch API for path: {Path}", LoggingHelper.SanitizeForLogging(path));
            context.Response.StatusCode = 502;
            await context.Response.WriteAsJsonAsync(new { error = "No response from upstream API" });
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsync(responseBody);
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "HTTP error while proxying content URL request");
        context.Response.StatusCode = 502;
        await context.Response.WriteAsJsonAsync(new { error = "Failed to connect to upstream API", message = ex.Message });
    }
    catch (TaskCanceledException ex)
    {
        logger.LogError(ex, "Content URL request timed out");
        context.Response.StatusCode = 504;
        await context.Response.WriteAsJsonAsync(new { error = "Request timed out", message = ex.Message });
    }
    catch (UriFormatException ex)
    {
        logger.LogError(ex, "Invalid URI format in path parameter");
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid path format", message = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error proxying content URL request");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Internal server error", message = ex.Message });
    }
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();