using Polly;
using Polly.Extensions.Http;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient + Polly
builder.Services.AddHttpClient("external-api", client =>
{
    client.BaseAddress = new Uri("https://httpstat.us/");
    client.Timeout = TimeSpan.FromSeconds(3);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("ResilientApi"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    const string header = "X-Correlation-ID";

    if (!context.Request.Headers.TryGetValue(header, out var correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
        context.Request.Headers[header] = correlationId;
    }

    context.Response.Headers[header] = correlationId;

    using (context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Correlation")
        .BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId.ToString()
        }))
    {
        await next();
    }
});


app.MapGet("/health", () => Results.Ok("OK"));

app.MapGet("/external", async (
    IHttpClientFactory factory,
    ILogger<Program> logger) =>
{
    var client = factory.CreateClient("external-api");

    logger.LogInformation("Calling external API");

    try
    {
        var response = await client.GetAsync("500");
        return Results.StatusCode((int)response.StatusCode);
    }
    catch (TaskCanceledException ex)
    {
        logger.LogWarning(ex, "Timeout calling external API");
        return Results.Problem(
            title: "External service timeout",
            statusCode: StatusCodes.Status504GatewayTimeout);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error calling external API");
        return Results.Problem(
            title: "External service error",
            statusCode: StatusCodes.Status502BadGateway);
    }

});

app.Run();

// 🔹 Polly policies

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt =>
                TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (outcome, timespan, retry, _) =>
            {
                Console.WriteLine($"Retry {retry} after {timespan.TotalSeconds}s");
            });

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(20),
            onBreak: (_, ts) =>
            {
                Console.WriteLine($"Circuit opened for {ts.TotalSeconds}s");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit closed");
            });
