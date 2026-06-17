using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Data;
using Data.Interface;
using Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Dashboard;

public class Function
{
    public ServiceProvider serviceProvider = Initializer();
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
    {
        Console.WriteLine("Function Execution started");
        Console.WriteLine(input.RouteKey);
        if (!input.Headers.TryGetValue("authorization", out var authHeader))
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

        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPOSRepository>();
        var sales = await repository.GetSales();
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = System.Text.Json.JsonSerializer.Serialize(sales),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static ServiceProvider Initializer()
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
}
