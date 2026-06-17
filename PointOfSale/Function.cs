using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Data;
using Data.Entities;
using Data.Interface;
using Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var serviceProvider = Initializer();

Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, Task<APIGatewayHttpApiV2ProxyResponse>> handler = async (APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) =>
{
    using var scope = serviceProvider.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<IPOSRepository>();
    Console.WriteLine("Function Execution started");
    Console.WriteLine(request.RouteKey);

    if (!request.Headers.TryGetValue("authorization", out var authHeader))
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 401,
            Body = "Authorization header missing",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        ? authHeader.Substring("Bearer ".Length).Trim()
        : authHeader;

    if (string.IsNullOrWhiteSpace(token) || token != "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ0ZXN0Ijp0cnVlLCJhcHAiOiJ0ZXN0In0.zYzfLyd09ZakINqIoq1Qotz8Y0mfQTMGcCz7cWl6i9k")
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 401,
            Body = "Authorization header missing",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    if (request.RouteKey.Contains("GET /"))
    {
        string? name = string.Empty;
        long Id = 0;
        if(request != null && request.QueryStringParameters != null)
        {
            request.QueryStringParameters.TryGetValue("name", out name);
            try
            {
                Id = Convert.ToInt64(name);
            }
            catch(Exception ex)
            {
                Id = 0;
            }
        }
        if (Id <= 0)
        {
            var sales = await repository.GetSales();
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(sales),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        else
        {
            Sale? sale = await repository.GetSaleById(Id);
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = sale != null ? JsonSerializer.Serialize(sale) : JsonSerializer.Serialize("Sale not found"),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
    else if (request.RouteKey.Contains("POST /") && request.Body != null)
    {
        Sale? sale = JsonSerializer.Deserialize<Sale>(request.Body);
        if (sale is null) return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400, Body = "Invalid request body" };
        bool isSuccess = await repository.CreateSale(sale);
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = isSuccess ? 200 : 500,
            Body = isSuccess ? "Sale was created successfully" : "Something went wrong",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
    else if (request.RouteKey.Contains("PUT /") && request.Body != null)
    {
        Sale? sale = JsonSerializer.Deserialize<Sale>(request.Body);
        if (sale is null) return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400, Body = "Invalid request body" };
        bool isSuccess = await repository.UpdateSale(sale);
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = isSuccess ? 200 : 500,
            Body = isSuccess ? "Sale was updated successfully" : "Something went wrong",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
    else if (request.RouteKey.Contains("DELETE /") && request.Body != null)
    {
        long saleId = JsonSerializer.Deserialize<long>(request.Body);
        bool isSuccess = await repository.DeleteSale(saleId);
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = isSuccess ? 200 : 500,
            Body = isSuccess ? "Sale was deleted successfully" : "Something went wrong",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
    else if (request.RouteKey.Contains("PATCH /") && request.Body != null)
    {
        Sale? sale = JsonSerializer.Deserialize<Sale>(request.Body);
        if (sale is null) return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400, Body = "Invalid request body" };
        bool isSuccess = await repository.ArchiveSale(sale.Id);
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = isSuccess ? 200 : 500,
            Body = isSuccess ? "Sale was archived successfully" : "Something went wrong",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
    else
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 404,
            Body = JsonSerializer.Serialize("Resource Not Found"),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();

static ServiceProvider Initializer()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

    var services = new ServiceCollection();
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    services.AddScoped<IPOSRepository, POSRepository>();
    return services.BuildServiceProvider();
}
