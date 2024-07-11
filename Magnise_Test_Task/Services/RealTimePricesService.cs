using Magnise_Test_Task.Data;
using Magnise_Test_Task.Data.Entities;
using Magnise_Test_Task.Interfaces;
using Magnise_Test_Task.Models.FinTechApi.Responses;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;

namespace Magnise_Test_Task.Services
{
    public class RealTimePricesService : BackgroundService
    {
        private readonly object _lock = new object();
        private readonly IServiceProvider _serviceProvider;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private ClientWebSocket _clientWebSocket;

        public RealTimePricesService(IConfiguration configuration, IServiceProvider serviceProvider, ITokenService tokenService, ILogger<RealTimePricesService> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _tokenService = tokenService;
            _clientWebSocket = new ClientWebSocket();
        }

        private async void WebSocket_Opened()
        {
            _logger.LogInformation("WebSocket connection opened.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MarketDbContext>();
                var assets = await dbContext.Assets.Include(a => a.Mappings).ToListAsync();

                foreach (var asset in assets)
                {
                    foreach (var mapping in asset.Mappings)
                    {
                        var subscriptionMessage = new
                        {
                            type = "l1-subscription",
                            instrumentId = asset.Id.ToString(),
                            provider = mapping.Provider,
                            subscribe = true,
                            kinds = new[] { "last" }
                        };

                        var messageString = JsonConvert.SerializeObject(subscriptionMessage);
                        await _clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageString)), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }

        private async Task ReceiveMessagesAsync(CancellationToken stoppingToken)
        {
            var buffer = new byte[1024 * 4];

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, stoppingToken);
                    _logger.LogInformation("WebSocket connection closed.");
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    WebSocket_MessageReceived(message);
                }
            }
        }

        private async void WebSocket_MessageReceived(string message)
        {
            _logger.LogInformation("WebSocket message received: {Message}", message);

            var jsonObject = JObject.Parse(message);
            var messageType = jsonObject["type"].ToString();

            if (messageType == "l1-update")
            {
                var updateMessage = JsonConvert.DeserializeObject<L1UpdateMessage>(message);
                if (updateMessage != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<MarketDbContext>();

                        lock (_lock)
                        {
                            var mapping = dbContext.AssetProviderMappings
                                .Include(m => m.Asset)
                                .FirstOrDefault(m => m.Asset.Id.ToString() == updateMessage.InstrumentId && m.Provider == updateMessage.Provider);

                            if (mapping != null)
                            {
                                var price = dbContext.Prices.FirstOrDefault(p => p.AssetProviderMappingId == mapping.Id);

                                if (price != null)
                                {
                                    // Update existing price
                                    price.Value = updateMessage.Last.price;
                                    price.Timestamp = DateTime.UtcNow;
                                    dbContext.Prices.Update(price);
                                }
                                else
                                {
                                    // Add new price
                                    price = new Price
                                    {
                                        AssetProviderMappingId = mapping.Id,
                                        Value = updateMessage.Last.price,
                                        Timestamp = DateTime.UtcNow
                                    };
                                    dbContext.Prices.Add(price);
                                }

                                dbContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var token = await _tokenService.GetTokenAsync();
            var wsUri = new Uri(_configuration["ThirdPartyApi:URI_WSS"] + $"?token={token}");

            _logger.LogInformation($"Connecting to WebSocket at {wsUri}");

            await _clientWebSocket.ConnectAsync(wsUri, stoppingToken);
            if (_clientWebSocket.State == WebSocketState.Open)
            {
                WebSocket_Opened();
            }

            await ReceiveMessagesAsync(stoppingToken);
        }


        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            if (_clientWebSocket != null)
            {
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service stopping", stoppingToken);
                _clientWebSocket.Dispose();
            }
            await base.StopAsync(stoppingToken);
        }
    }

}
