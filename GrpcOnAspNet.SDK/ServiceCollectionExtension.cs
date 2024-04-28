using GrpcOnAspNet.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
