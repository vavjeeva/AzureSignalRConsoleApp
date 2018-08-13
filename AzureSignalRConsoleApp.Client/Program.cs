using AzureSignalRConsoleApp.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureSignalRConsoleApp.Client
{
    class Program
    {
        private static readonly string userId = $"User {new Random().Next(1, 99)}";
        private static ServiceUtils serviceUtils;
        private static readonly string hubName = "ConsoleAppBroadcaster";
        private static HubConnection hubConnection;

        async static Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>()
                .Build();

            serviceUtils = new ServiceUtils(configuration["Azure:SignalR:ConnectionString"]);

            var url = $"{serviceUtils.Endpoint}:5001/client/?hub={hubName}";

            hubConnection = new HubConnectionBuilder()
                .WithUrl(url, option =>
                {
                    option.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(serviceUtils.GenerateAccessToken(url, userId));
                    };
                }).Build();

            hubConnection.On<string, string>("SendMessage",
                (string server, string message) =>
                {
                    Console.WriteLine($"Message from server {server}: {message}");
                });

            await hubConnection.StartAsync();
            Console.WriteLine("Client started... Press any key to close the connection");
            Console.ReadLine();
            await hubConnection.DisposeAsync();
            Console.WriteLine("Client is shutting down...");
        }
    }
}
