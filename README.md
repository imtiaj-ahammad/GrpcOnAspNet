## GRPC On ASP.NET 
How to Use gRPC in Asp.Net Core!

We will have 1 API, 1 gRPC Server and 1 gRPC SDK(class library). The API is going to use SDK to communicate with gRPC server.


1. Lets Create a ASP.NET web API called **GrpcOnAspNet.API** with solution name **GrpcOnAspNet**
2. Let create a new project with **ASP.NET Core gRPC Service** framework named **GrpcOnAspNet.Server**
3.  Let's go to **Program.cs** and modify the Grpc injection
    ```
    builder.Services.AddGrpc(options =>
    {
    options.EnableDetailedErrors = true;
    });
    ```
4. Let's add the **SDK** project. We can not call grpc directly from the browser. So, we will use this **SDK** project to call the server. Name it **GrpcOnAspNet.SDK**
5. Add the following packages for the **GrpcOnAspNet.SDK**
    ```
    <PackageReference Include="Google.Protobuf" Version="3.26.1" />
    <PackageReference Include="Grpc.Net.Client" Version="2.62.0" />
    <PackageReference Include="Grpc.Tools" Version="2.62.0">
    <PackageReference Include="Grpc.Net.ClientFactory" Version="2.62.0" />
    ```
6. Let's go to **GrpcOnAspNet.Server.csproj** and copy the following line and paste it in the **GrpcOnAspNet.SDK** with **modified** path and service type
    ```
    <ItemGroup>
        <Protobuf Include="Protos\greet.proto" GrpcServices="Server" />
    </ItemGroup>
    ```
    ```
	<ItemGroup>
		<Protobuf Include="..\GrpcOnAspNet.Server\Protos\greet.proto" GrpcServices="Client" />
	</ItemGroup>
    ```
6. Let's go to **GrpcOnAspNet.SDK** and add a new class **ServiceCollectionExtension**. We are going to use this class to inject all the services. The Uri here is fetched from **GrpcOnAspNet.Server.Properties.launchSettings.json**
    ```
    public static class ServiceCollectionExtension
    {
        public static void AddGrpcSdk(this IServiceCollection services)
        {
            services.AddGrpcClient<Greeter.GreeterClient>(client =>
            {
                client.Address = new Uri("https://localhost:7253");
            });
        }
    }
    ```
7. Now we are going to add new interface from where we might call all the service instead of using the **Greeter.GreeterClient** directly. Also we are adding the implementation class here as well.
    ```
    namespace GrpcOnAspNet.SDK
    {
        public interface IGreeterGrpcService
        {
            Task<string> SayHelloAsync(string name, CancellationToken cancellationToken);
        }

        public class GreeterGrpcService : IGreeterGrpcService 
        {
            private readonly Greeter.GreeterClient grpcClient;
            public GreeterGrpcService(Greeter.GreeterClient grpcClient)
            {
                this.grpcClient = grpcClient;
            }
            public async Task<string> SayHelloAsync(string name, CancellationToken cancellationToken)
            {
                try
                {
                    var result = await grpcClient.SayHelloAsync(new HelloRequest { Name = name, }, cancellationToken: cancellationToken);
                    return result.Message;
                }
                catch (RpcException ex)
                {
                    throw;
                }
            }
        }
    }
    ```
**In gRPC, the exception type is only RpcException. We can use StatusCode.type to identify the specific error devoted for gRPC only**

8. Now lets add the service implementation injection into the **ServiceCollectionExtension.cs**
    ```
    namespace GrpcOnAspNet.SDK
    {
        public static class ServiceCollectionExtension
        {
            public static void AddGrpcSdk(this IServiceCollection services)
            {
                services.AddGrpcClient<Greeter.GreeterClient>(client =>
                {
                    client.Address = new Uri("https://localhost:7253");
                });

                services.AddScoped<IGreeterGrpcService, GreeterGrpcService>();
            }
        }
    }
    ```
9. Let's go to **API** project and add the **SDK** project reference into it
    ```
    <ItemGroup>
        <ProjectReference Include="..\GrpcOnAspNet.SDK\GrpcOnAspNet.SDK.csproj" />
    </ItemGroup>
    ```
10. Now let's go to Program.cs in API project and add the following using first. Then add the extension method of **AddGrpcSdk()**. And lastly call the grpc method in the API call.
    ```
    using GrpcOnAspNet.SDK;
    ```
    ```
    builder.Services.AddGrpcSdk();
    ```
    ```
    app.MapGet("/weatherforecast", async ([FromServices] IGreeterGrpcService grpcService, CancellationToken cancellation) =>
    {
        var result = await grpcService.SayHelloAsync("GRPC", cancellation);

        Console.WriteLine($"GRPC result: {result}");

        Console.WriteLine($"GRPC Result: {result}", result);

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateTime.Now.AddDays(index),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    })
    ```
11. Let's run both the Server and API project and check if grpc log is executed with successful API response.  





#### Rerferences:
- https://www.youtube.com/watch?v=SgCAPjyotLM
- https://learn.microsoft.com/en-us/aspnet/core/tutorials/grpc/grpc-start?view=aspnetcore-8.0&tabs=visual-studio