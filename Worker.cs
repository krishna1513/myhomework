using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using WebsocketServer;
using Grpc.Core;
using Google.Protobuf;

namespace ServerTime_Service
{
    public class ClientOperations
    {
        public readonly ServerConnection.ServerConnectionClient _client;
        public ClientOperations(Channel ch)
        {
            this._client = new ServerConnection.ServerConnectionClient(ch);
        }
        public int ServerLoad()
        {
            ConnectionInfo result = _client.TotalClientConnections(new ServerStatus { Live = 1 });

            return result.CurrentCount;
        }
    }
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private List<string> urls = new List<string> { "https://evsnstest.idrive.com/servertime?" };
        private readonly string FilePath = "";

        const string ipaddress = "192.168.1.134:41561";

        public ClientOperations grpc_client = new ClientOperations(new Channel(ipaddress, ChannelCredentials.Insecure));

       
        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            FilePath = @"D:\gRpc_Server_client\logs\status.txt";
           
    }

        private void WriteToFile(string text)
        {
            using (StreamWriter f = new StreamWriter(FilePath,append:true))
            {
                f.WriteLine(text);
                f.Flush();
                f.Close();
            }
           
            
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {                   
                    await PollUrls();
                }
                catch(Exception e)
                {
                    _logger.LogInformation(e,"some error happend in execute async");
                }
                finally
                {
                    await Task.Delay(8000, stoppingToken);
                }
            }
        }
        private async Task PollUrls()
        {
            var tasks = new List<Task>();
            foreach(var t in urls)
            {
                tasks.Add(PollUrls(t));
            }

            await Task.WhenAll(tasks);
        }

        private async Task PollUrls(string url)
        {
            try
            {
               // int serverload = grpc_client.ServerLoad();
               // WriteToFile("server load currently  -  "+ serverload.ToString());

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);
                if(response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("{0} is online",url);
                    WriteToFile("online  - " + DateTime.Now);
                }
                else
                {
                    _logger.LogInformation("{1} is offline", url);
                    WriteToFile("offline  - " + DateTime.Now);
                }
            }
            catch(Exception e)
            {
                _logger.LogInformation(e,"some exception happend in pollurls");
            }
        }
    }
}
