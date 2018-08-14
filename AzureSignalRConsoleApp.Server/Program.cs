using AzureSignalRConsoleApp.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AzureSignalRConsoleApp.Server
{
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string hubName = "ConsoleAppBroadcaster";
        private static readonly string serverName = "Azure_SignalR_Server_1";
        private static ServiceUtils serviceUtils;

        static void Main(string[] args)
        {
            //Loading the Configuration Objects from UserSecrets
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddUserSecrets<Program>()
               .Build();

            serviceUtils = new ServiceUtils(configuration["Azure:SignalR:ConnectionString"]);

            Console.WriteLine(" Azure SignalR Server Started.\n " +
                              "Start typing and press enter to broadcast messages to all the connected clients.\n " +
                              "Type quit to shut down the server!");
            while (true)
            {
                var data = Console.ReadLine();
                if (data.ToLower() == "quit") break;
                Broadcast(data);
            }

            Console.WriteLine("SignalR Server is shutting down");
        }

        private static async void Broadcast(string message)
        {
            var url = $"{serviceUtils.Endpoint}:5002/api/v1-preview/hub/{hubName.ToLower()}";
            var request = new HttpRequestMessage(HttpMethod.Post, new UriBuilder(url).Uri);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", serviceUtils.GenerateAccessToken(url, serverName));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var messageContent = new MessageContent() { Target = "SendMessage", Arguments = new[] { serverName, message } };
            request.Content = new StringContent(JsonConvert.SerializeObject(messageContent), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                Console.WriteLine($"Sent error: {response.StatusCode}");
            }
        }
    }

    public class MessageContent
    {
        public string Target { get; set; }

        public object[] Arguments { get; set; }
    }
}
