﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class HttpCommModule : CommModule
    {

        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }

        private CancellationTokenSource _tokenSource;
        private HttpClient _client;
        public HttpCommModule(string connectAddress, int connectPort)
        {
            ConnectAddress = connectAddress;
            ConnectPort = connectPort;
        }

        public override void Init(AgentMetadata metadata)
        {
            base.Init(metadata);

            _client = new HttpClient();
            _client.BaseAddress = new System.Uri($"http://{ConnectAddress}:{ConnectPort}");
            _client.DefaultRequestHeaders.Clear();

            var encodedMetadata = Convert.ToBase64String(AgentMetadata.Serialise());

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer { encodedMetadata}");

        }
        public override async Task Start()
        {
            _tokenSource = new CancellationTokenSource();

            while (!_tokenSource.IsCancellationRequested)
            {
                //check to see if we have data to send
                if (!Outbound.IsEmpty)
                {
                    await PostData();
                }
                else
                {
                    await CheckIn();
                }

                Task.Delay(1000);                          
            }
        }

        private async Task CheckIn()
        {
            var response = await _client.GetByteArrayAsync("/");
            HandleResponse(response);

        }

        public async Task PostData()
        {
            var outbound = GetOutbound().Serialise();
            var content = new StringContent(Encoding.UTF8.GetString(outbound), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/", content);
            var responseContent = await response.Content.ReadAsByteArrayAsync();

            HandleResponse(responseContent);

        }

        private void HandleResponse(byte[] response)
        {
            var tasks = response.Deserialize<AgentTask[]>();
            if (tasks != null && tasks.Any())
            {
                foreach ( var task in tasks)
                {
                    Inbound.Enqueue(task);
                }
            }
        }
        

        public override void Stop()
        {
            _tokenSource.Cancel();
        }

    }
}
