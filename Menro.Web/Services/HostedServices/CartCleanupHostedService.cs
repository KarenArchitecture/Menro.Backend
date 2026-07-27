using Menro.Domain.Interfaces;

namespace Menro.Web.Services.HostedServices
{
    public class CartCleanupHostedService : BackgroundService
    {
        private readonly IServiceProvider _services;
        public CartCleanupHostedService(IServiceProvider services) => _services = services;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var expired = await uow.Cart.GetExpiredCartsAsync(DateTime.UtcNow.AddHours(-2), stoppingToken);
                foreach (var cart in expired)
                    await uow.Cart.RemoveCartAsync(cart, stoppingToken);

                if (expired.Count > 0)
                    await uow.Cart.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}