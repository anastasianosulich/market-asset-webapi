using Magnise_Test_Task.Interfaces;

namespace Magnise_Test_Task.Services
{
    public class AssetFetchingService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public AssetFetchingService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var assetService = scope.ServiceProvider.GetRequiredService<IAssetService>();
                await assetService.UpdateAssetsAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

}
