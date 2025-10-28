using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync();
        
        logger.LogInformation("Proxying GraphQL request to JustWatch API");

        var client = httpClientFactory.CreateClient("JustWatchAPI");
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync("/graphql", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsync(responseBody);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error proxying GraphQL request");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Proxy error", message = ex.Message });
    }
});

// Content URLs Proxy Endpoint
app.MapGet("/content/urls", async (HttpContext context, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    try
    {
        var path = context.Request.Query["path"].ToString();
        if (string.IsNullOrEmpty(path))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Missing path parameter" });
            return;
        }

        logger.LogInformation("Proxying content URL request for path: {Path}", path);

        var client = httpClientFactory.CreateClient("JustWatchAPI");
        var response = await client.GetAsync($"/content/urls?path={Uri.EscapeDataString(path)}");
        var responseBody = await response.Content.ReadAsStringAsync();

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsync(responseBody);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error proxying content URL request");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Proxy error", message = ex.Message });
    }
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();